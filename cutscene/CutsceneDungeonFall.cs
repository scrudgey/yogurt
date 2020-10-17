using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CutsceneDungeonFall : CutsceneFall {
    override protected float fallDist() { return -2.91f; }
    private float catchPoint = 0.317f;
    private bool caught;
    private bool dumping;
    private float dumpTimer;
    private float dumpInterval = 1f;
    private Inventory playerInventory;
    private Outfit playerOutfit;
    private GameObject ejectorDump;
    private Vector2 catchPosition;
    private AudioClip dumpSound;
    public ParticleSystem magicEffect;
    public AudioSource magicAudio;
    public bool rejecting;
    public float rejectTimer;
    public GameObject hench;
    public Speech henchSpeech;
    public bool henchSpeechHappened;
    public override void Configure() {
        base.Configure();
        ejectorDump = Resources.Load("particles/ejectorDump") as GameObject;
        dumpSound = Resources.Load("sounds/8bit_impact2") as AudioClip;
        Toolbox.Instance.SwitchAudioListener(player);
        magicEffect = GameObject.Find("magic").GetComponent<ParticleSystem>();
        magicAudio = GameObject.Find("magic").GetComponent<AudioSource>();
        hench = GameObject.Find("hench") as GameObject;
        hench.SetActive(false);
        henchSpeech = hench.GetComponent<Speech>();
        playerInventory = player.GetComponent<Inventory>();
        playerOutfit = player.GetComponent<Outfit>();
    }
    public override void Update() {
        if (rejecting) {
            RejectUpdate();
        } else if (!caught || (caught && !dumping)) {
            base.Update();
            if (player.transform.position.y < catchPoint && !caught) {
                catchPosition = player.transform.position;
                caught = true;
                // stop fall
                if (playerBody) {
                    playerBody.gravityScale = 0f;
                    playerBody.drag = initDrag;
                    playerBody.velocity = Vector3.zero;
                }
                if (Toolbox.Instance.CloneRemover(GameManager.Instance.playerObject.name) == "vampyr") {
                    // reject dracula
                    rejecting = true;
                } else {
                    dumping = true;
                    magicEffect.Play();
                    magicAudio.Play();
                    magicEffect.transform.position = player.transform.position;
                }
            }
        } else if (dumping) {
            DumpUpdate();
        }
    }
    public void RejectUpdate() {
        rejectTimer += Time.deltaTime;
        player.transform.position = catchPosition;
        if (rejectTimer > 1f && !hench.activeInHierarchy) {
            hench.SetActive(true);
        } else if (rejectTimer > 4f && !henchSpeechHappened) {
            DialogueMenu menu = henchSpeech.SpeakWith();
            menu.menuClosed += MenuWasClosed;
            henchSpeechHappened = true;
        }
    }
    public void DumpUpdate() {
        dumpTimer += Time.deltaTime;
        player.transform.position = catchPosition + UnityEngine.Random.insideUnitCircle * 0.01f;
        // shake
        // rotate
        if (dumpTimer > dumpInterval) {
            dumpTimer = 0;
            dumpInterval *= 0.9f;
            if (playerInventory != null && playerInventory.holding != null) {
                GameObject toDump = playerInventory.holding.gameObject;
                playerInventory.SoftDropItem();
                Dump(toDump);
            } else if (playerInventory != null && playerInventory.items.Count > 0) {
                GameObject pickup = playerInventory.items[0];
                playerInventory.items.RemoveAt(0);
                Dump(pickup);
            } else if (playerOutfit != null && !playerOutfit.nude) {
                // dump outfit
                GameObject removedUniform = playerOutfit.RemoveUniform();
                playerOutfit.GoNude();
                Dump(removedUniform);
            } else {
                player.transform.position = catchPosition;
                dumping = false;
                magicEffect.Stop();
                magicAudio.Stop();
                if (playerBody != null) {
                    playerBody.gravityScale = 1f;
                    playerBody.drag = 0;
                }
            }
        }
    }
    void Dump(GameObject obj) {
        // play sfx
        SpriteRenderer objSprite = obj.GetComponent<SpriteRenderer>();
        GameObject dumpObject = GameObject.Instantiate(ejectorDump);
        dumpObject.transform.position = player.transform.position;
        ParticleSystem system = dumpObject.GetComponent<ParticleSystem>();
        system.randomSeed = (uint)UnityEngine.Random.Range(0, 9999999);
        system.textureSheetAnimation.SetSprite(0, objSprite.sprite);
        system.Play();
        GameObject.Destroy(obj);
        GameManager.Instance.PlayPublicSound(dumpSound);
    }
    public void MenuWasClosed() {
        SceneManager.LoadScene("vampire_house");
    }
}