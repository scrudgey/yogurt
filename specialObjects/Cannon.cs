using UnityEngine;
using System.Collections;

public class Cannon : Interactive, ISaveable {
    public ParticleSystem shootEffect;
    public AudioSource audioSource;
    public AudioClip shootSound;
    public bool charged;
    public Interaction shootPlayerAction;
    public Interaction shootItemAction;
    public Interaction chargeAction;

    Transform ejectionPoint;
    void Start() {
        ejectionPoint = transform.Find("ejectionPoint");

        shootPlayerAction = new Interaction(this, "Enter", "StartShootPlayer");
        shootPlayerAction.descString = "Climb into cannon";
        interactions.Add(shootPlayerAction);

        shootItemAction = new Interaction(this, "Shoot", "StartShootItem");
        // shootItemAction.descString = "Shoot cannon";
        interactions.Add(shootItemAction);

        chargeAction = new Interaction(this, "Fill", "ChargeCannon");
        chargeAction.descString = "Fill cannon with gunpowder";
        chargeAction.inertOnPlayerConsent = true;
        chargeAction.otherOnPlayerConsent = true;
        interactions.Add(chargeAction);

        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public void ShootPlayer() {
        shootEffect.Play();
        audioSource.PlayOneShot(shootSound);
    }
    public void StartShootPlayer() {
        if (charged) {
            charged = false;
            MySaver.Save();
            disableInteractions = true;
            CutsceneManager.Instance.InitializeCutscene<CutsceneCannon>();
        } else {
            MessageSpeech message = new MessageSpeech("No gunpowder!");
            Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        }
    }

    public void StartShootItem(PhysicalBootstrapper pb) {
        if (charged) {
            charged = false;
            StartCoroutine(ShootItemRoutine(pb));
        } else {
            MessageSpeech message = new MessageSpeech("No gunpowder!");
            Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        }
    }
    public string StartShootItem_desc(PhysicalBootstrapper item) {
        string itemName = Toolbox.Instance.GetName(item.gameObject);
        return "Shoot " + itemName + " out of cannon";
    }
    public void ChargeCannon(GunpowderKeg keg) {
        if (charged) {
            // Debug.Log("it's full");
        } else {
            charged = true;
        }
    }
    public void SaveData(PersistentComponent data) {
        data.bools["charged"] = charged;
    }
    public void LoadData(PersistentComponent data) {
        charged = data.bools["charged"];
    }
    IEnumerator ShootItemRoutine(PhysicalBootstrapper pb) {

        pb.transform.position = ejectionPoint.position;
        pb.transform.rotation = ejectionPoint.rotation;

        ClaimsManager.Instance.WasDestroyed(pb.gameObject);
        pb.DestroyPhysical();
        pb.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        pb.gameObject.SetActive(true);

        pb.transform.position = ejectionPoint.position;
        pb.transform.rotation = ejectionPoint.rotation;
        // pb.initHeight = 0.15f;
        pb.InitPhysical(0.15f, ejectionPoint.up * 5f);

        shootEffect.Play();
        audioSource.PlayOneShot(shootSound);
        disableInteractions = false;
        // pb.Set3Motion(ejectionPoint.up * 50f);
        // GameManager.Instance.playerObject.SetActive(true);
        //     RotateTowardMotion rot = GameManager.Instance.playerObject.AddComponent<RotateTowardMotion>();
        //     rot.angleOffset = 270f;
        //     camControl.focus = GameManager.Instance.playerObject;
        //     foreach (Collider2D collider in GameManager.Instance.playerObject.GetComponentsInChildren<Collider2D>()) {
        //         collider.enabled = false;
        //     }
        //     GameManager.Instance.playerObject.transform.position = ejectionPoint.position;
        //     GameManager.Instance.playerObject.transform.rotation = ejectionPoint.rotation;
        //     AdvancedAnimation playerAnimation = GameManager.Instance.playerObject.GetComponent<AdvancedAnimation>();
        //     if (playerAnimation != null) {
        //         playerAnimation.SetFrame(14);
        //         playerAnimation.enabled = false;
        //     }
        //     HeadAnimation playerHeadAnimation = GameManager.Instance.playerObject.GetComponent<HeadAnimation>();
        //     if (playerHeadAnimation != null) {
        //         playerHeadAnimation.SetFrame(4);
        //         playerHeadAnimation.enabled = false;
        //     }
        //     Controllable playerControllable = GameManager.Instance.playerObject.GetComponent<Controllable>();
        //     if (playerControllable != null) {
        //         playerControllable.enabled = false;
        //     }
        //     Rigidbody2D playerBody = GameManager.Instance.playerObject.GetComponent<Rigidbody2D>();
        //     playerBody.gravityScale = GameManager.Instance.gravity * 0.8f;
        //     playerBody.drag = 0f;
        //     AudioSource playerAudio = Toolbox.GetOrCreateComponent<AudioSource>(GameManager.Instance.playerObject);
        //     AudioClip charlierAugh = Resources.Load("sounds/auugh") as AudioClip;
        //     playerAudio.PlayOneShot(charlierAugh);

        //     playerBody.AddForce(12000f * ejectionPoint.up, ForceMode2D.Force);
    }
}
