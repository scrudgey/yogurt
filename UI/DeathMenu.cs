using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class DeathMenu : MonoBehaviour {
    public bool buttonVisible;
    public GameObject button;
    public GameObject resurrectButton;
    public List<Rigidbody2D> letters;
    public bool lettersInactive;
    private float timer;
    bool keypressedThisFrame;
    void Awake() {
        InputController.Instance.PrimaryAction.action.performed += _ => keypressedThisFrame = true;
    }
    void Start() {
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = GameManager.Instance.cam;

        button = transform.Find("Button").gameObject;
        resurrectButton = transform.Find("ResurrectButton").gameObject;
        lettersInactive = false;
        button.SetActive(false);
        resurrectButton.SetActive(false);
        letters.Add(transform.Find("Y").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("O").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("U").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("D").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("I").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("E").GetComponent<Rigidbody2D>());
        letters.Add(transform.Find("D2").GetComponent<Rigidbody2D>());

        UINew.Instance.Start();
        UIColliderManager colliderManager = UINew.Instance.UICanvas.GetComponent<UIColliderManager>();
        colliderManager.Start();
        Collider2D UICollider = colliderManager.topEdge;
        foreach (Rigidbody2D letter in letters) {
            Collider2D letterCollider = letter.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(letterCollider, UICollider, true);
        }
        GameManager.Instance.StartCoroutine(ChangeMusic());
    }
    public void ButtonCallback() {
        GameManager.Instance.NewDayCutscene();
    }
    public void ResurrectButtonCallback() {
        GameManager.Instance.data.lichRevivalToday = true;
        GameObject lich = GameObject.Instantiate(Resources.Load("prefabs/Lich"), GameManager.Instance.lastPlayerPosition, Quaternion.identity) as GameObject;

        GameObject playerObject = GameManager.Instance.playerObject;
        Toolbox.CleanUpChildren(GameManager.Instance.playerObject);
        Destroy(GameManager.Instance.playerObject);



        GameManager.Instance.SetFocus(lich);
        Toolbox.SetGender(lich, GameManager.Instance.data.defaultGender, changeHead: false);
        Destroy(gameObject);
        GameObject.Instantiate(Resources.Load("particles/licheffect"), GameManager.Instance.lastPlayerPosition, Quaternion.identity);
    }
    IEnumerator ChangeMusic() {
        MusicController.Instance.StopMusic();
        yield return new WaitForSeconds(2f);
        // if (MusicController.Instance.nowPlayingTrack == null)
        MusicController.Instance.SetMusic(new Music(new Track(TrackName.tweakDelay)));
    }
    void Update() {
        timer += Time.deltaTime;
        if (keypressedThisFrame && timer > 1) {
            if (!buttonVisible) {
                ShowButtons();
            }
        }
        bool tempLettersInactive = true;
        if (!lettersInactive && timer > 1) {
            foreach (Rigidbody2D letter in letters) {
                if (letter.velocity.sqrMagnitude > 0.000001f) {
                    tempLettersInactive = false;
                    break;
                }
            }
            if (tempLettersInactive) {
                lettersInactive = true;
            }
        }
        if (lettersInactive && !buttonVisible) {
            ShowButtons();
        }

        keypressedThisFrame = false;
    }
    public void ShowButtons() {
        buttonVisible = true;
        button.SetActive(true);
        if (GameManager.Instance.data.perks["resurrection"] == true && !GameManager.Instance.data.lichRevivalToday) {
            resurrectButton.SetActive(true);
        }
    }
}
