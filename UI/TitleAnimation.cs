using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Easings;
using UnityEngine.InputSystem;


public class TitleAnimation : MonoBehaviour {
    public enum State { splatter, whiteout, main }
    public Mask mask;
    public State state;
    public Image maskImage;
    public float timer;
    public float splatInterval;
    public AudioClip[] splatSounds;
    private int width = 160;
    private int height = 100;
    public Sprite splatSprites;
    public Image whiteOut;
    public int splats;
    // public ImagePulser pulser;
    public GameObject topLogo;
    public GameObject bottomLogo;
    public AudioClip whiteOutSound;
    public AudioClip revealSound;
    public GameObject mainMenu;
    public GameObject yogurtDropPrefab;
    public Camera renderingCamera;
    public List<Drip> drips = new List<Drip>();
    public ParticleSystem particles;
    Texture2D InitializeTexture() {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        int y = 0;
        while (y < texture.height) {
            int x = 0;
            while (x < texture.width) {
                texture.SetPixel(x, y, Color.clear);
                ++x;
            }
            ++y;
        }
        texture.name = ($"blanko");
        texture.Apply();
        return texture;
    }
    void Start() {
        Sprite blankSprite = Sprite.Create(
          InitializeTexture(),
          new Rect(0, 0, width, height),
          new Vector2(0.5f, 0.5f),
          100,
          1,
          SpriteMeshType.Tight,
          new Vector4(0, 0, width, height) // a guess
          );
        maskImage.sprite = blankSprite;
        renderingCamera = FindObjectOfType<Camera>();
        if (GameManager.Instance.titleIntroPlayed) {
            Skip();
        } else {
            StartCoroutine(doSplat(Random.Range(0, 1f)));
        }
    }
    void Skip() {
        GameObject.FindObjectOfType<StartMenu>().ShowPrompt();
        timer = 0;
        state = State.whiteout;
        GameManager.Instance.PlayPublicSound(whiteOutSound);
        for (int i = 0; i < 10; i++) {
            StartCoroutine(doSplat(Random.Range(0, 1f / (float)i)));
        }
    }
    void Update() {
        if (state == State.splatter) {
            if (Keyboard.current.anyKey.isPressed) {
                Skip();
            }
            timer += Time.deltaTime;
            if (timer > splatInterval) {
                splatInterval *= 0.8f;
                timer = 0f;
                // Splat();
                StartCoroutine(doSplat(Random.Range(0, 1f)));
                if (Random.Range(0, 1f) < 0.5f)
                    StartCoroutine(doSplat(Random.Range(0, 1f)));
                if (Random.Range(0, 1f) < 0.5f)
                    StartCoroutine(doSplat(Random.Range(0, 1f)));

                splats += 1;
                if (splats >= 5) {
                    timer = 0f;
                    state = State.whiteout;
                    GameManager.Instance.PlayPublicSound(whiteOutSound);
                }
            }
        } else if (state == State.whiteout) {
            timer += Time.deltaTime;
            Color color = whiteOut.color;
            color.a = (float)PennerDoubleAnimation.QuintEaseIn(timer, 0, 1, 0.75);
            whiteOut.color = color;

            if (timer > 1f) {
                state = State.main;
                ClearEffects();
            }
        }
        foreach (Drip drip in drips) {
            drip.Update();
        }
        if (drips.Count > 0)
            maskImage.sprite.texture.Apply();
    }

    void ClearEffects() {
        GameObject.FindObjectOfType<StartMenu>().ShowPrompt();
        GameManager.Instance.titleIntroPlayed = true;
        GameManager.Instance.PlayPublicSound(revealSound);
        whiteOut.enabled = false;
        mask.enabled = false;
        // pulser.enabled = true;
        topLogo.SetActive(true);
        bottomLogo.SetActive(false);
        MusicController.Instance.EnqueueMusic(new MusicTitle());
        mainMenu.SetActive(true);
        particles.Play();
    }
    IEnumerator doSplat(float delay) {
        yield return new WaitForSeconds(delay);
        Splat();
    }
    void Splat() {
        GameManager.Instance.PlayPublicSound(splatSounds[Random.Range(0, splatSounds.Length)]);
        AddSplatToSprite(maskImage.sprite);
    }

    void AddSplatToSprite(Sprite inSprite) {
        PasteSplatIntoTexture(inSprite.texture);
    }

    void PasteSplatIntoTexture(Texture2D texture) {
        Texture2D splat = splatSprites.texture;

        int splatOffsetY = Random.Range(0, 4) * 64;

        double mean = width / 2;
        double stdDev = height / 2;
        int splatX = (int)(width / 5 * (Toolbox.NextGaussianDouble() + 2));
        int splatY = (int)(height / 5 * (Toolbox.NextGaussianDouble() + 1));
        // Debug.Log($"{splatX} {splatY}");

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        int y = 0;
        while (y < texture.height) {
            int x = 0;
            while (x < texture.width) {
                if (x > splatX && x < splatX + 64 && y > splatY && y < splatY + 64) {
                    if (splat.GetPixel(x - splatX, splatOffsetY + y - splatY) == Color.black) {
                        texture.SetPixel(x, y, Color.white);
                    } else {
                        // texture.SetPixel(x, y, Color.red);
                    }
                }
                ++x;
            }
            ++y;
        }
        // texture.name = ($"mod");
        texture.Apply();

        // spawn droplets
        for (int i = 0; i < 10; i++) {
            if (Random.Range(0, 1f) < 0.5f)
                SpawnDroplet(new Vector2(splatX, splatY));
        }
        // spawn drips
        for (int i = 0; i < 10; i++) {
            if (Random.Range(0, 1f) < 0.5f)
                SpawnDrip(splatX + Random.Range(16, 48), splatY + Random.Range(16, 48));
        }
    }
    void SpawnDroplet(Vector2 position) {
        // convert texture coordinates to world position
        // we get: texture coordinates
        // this converts to screen coordinates, if we can determine the lower-left screen position of the texture
        // texture has aspect ratio fitter. so its width is height * 1.6. 
        // (camera width - (heigh * 1.6)) / 2 is x coordinate

        // texture to screen width
        // texture runs 0 - 160 in x, 0 - 100 in y
        // (tex.y) * renderingCamera.pixelHeigh / 100

        position.y *= renderingCamera.pixelHeight / height;
        position.x *= renderingCamera.pixelWidth / width;

        float offsetX = (renderingCamera.pixelWidth - (renderingCamera.pixelHeight * 1.6f));

        position.x += offsetX;
        position.y += renderingCamera.pixelHeight / 2f;

        Vector3 newPos = renderingCamera.ScreenToWorldPoint(position);
        newPos.z = 0;
        GameObject drop = GameObject.Instantiate(yogurtDropPrefab, newPos, Quaternion.identity);
        Rigidbody2D dropBody = drop.GetComponent<Rigidbody2D>();
        dropBody.velocity = Random.insideUnitCircle * Random.Range(10, 15);

        // float scale = Random.Range(10, 20);
        dropBody.transform.localScale = Vector2.one * Random.Range(5, 20);
    }


    public void SpawnDrip(int x, int y) {
        Drip drip = new Drip(x, y, maskImage.sprite.texture);
        drips.Add(drip);
    }
    public class Drip {
        int x;
        int y;
        int amount;
        int startY;
        float timeConst;
        Texture2D texture;
        public Drip(int x, int y, Texture2D texture) {
            this.x = x;
            this.y = y;
            this.startY = y;
            this.texture = texture;
            this.timeConst = Random.Range(0, 1f);
            amount = Random.Range(50, 75);
        }
        public void Update() {
            if (Random.Range(0, 1f) / Time.deltaTime < timeConst)
                return;
            if (amount > 0) {
                amount -= 1;
                y -= 1;
                texture.SetPixel(x, y, Color.white);
            }
        }

    }
}
