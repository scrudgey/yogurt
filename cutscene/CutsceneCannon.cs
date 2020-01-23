using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CutsceneCannon : Cutscene {
    public Cannon cannon;
    public CameraControl camControl;
    public Transform ejectionPoint;
    private float timer;
    private bool shot;
    public override void Configure() {
        cannon = GameObject.FindObjectOfType<Cannon>();
        Toolbox.Instance.SwitchAudioListener(cannon.gameObject);
        ejectionPoint = cannon.transform.Find("ejectionPoint");
        GameManager.Instance.playerObject.SetActive(false);
        camControl = GameObject.FindObjectOfType<CameraControl>();
        camControl.focus = cannon.gameObject;
        configured = true;
    }
    public override void Update() {
        if (timer == 0) {
            UINew.Instance.RefreshUI();
        }
        timer += Time.deltaTime;
        if (timer > 2.5f && !shot) {
            shot = true;
            cannon.ShootPlayer();
            Toolbox.Instance.SwitchAudioListener(GameManager.Instance.playerObject);
            GameManager.Instance.playerObject.SetActive(true);
            RotateTowardMotion rot = GameManager.Instance.playerObject.AddComponent<RotateTowardMotion>();
            rot.angleOffset = 270f;
            camControl.focus = GameManager.Instance.playerObject;
            foreach (Collider2D collider in GameManager.Instance.playerObject.GetComponentsInChildren<Collider2D>()) {
                collider.enabled = false;
            }
            GameManager.Instance.playerObject.transform.position = ejectionPoint.position;
            GameManager.Instance.playerObject.transform.rotation = ejectionPoint.rotation;
            AdvancedAnimation playerAnimation = GameManager.Instance.playerObject.GetComponent<AdvancedAnimation>();
            if (playerAnimation != null) {
                playerAnimation.SetFrame(14);
                playerAnimation.enabled = false;
            }
            HeadAnimation playerHeadAnimation = GameManager.Instance.playerObject.GetComponent<HeadAnimation>();
            if (playerHeadAnimation != null) {
                playerHeadAnimation.SetFrame(4);
                playerHeadAnimation.enabled = false;
            }
            Controllable playerControllable = GameManager.Instance.playerObject.GetComponent<Controllable>();
            if (playerControllable != null) {
                playerControllable.enabled = false;
            }
            Rigidbody2D playerBody = GameManager.Instance.playerObject.GetComponent<Rigidbody2D>();
            playerBody.gravityScale = GameManager.Instance.gravity * 0.8f;
            playerBody.drag = 0f;
            AudioSource playerAudio = Toolbox.GetOrCreateComponent<AudioSource>(GameManager.Instance.playerObject);
            AudioClip charlierAugh = Resources.Load("sounds/auugh") as AudioClip;
            playerAudio.PlayOneShot(charlierAugh);

            playerBody.AddForce(12000f * ejectionPoint.up, ForceMode2D.Force);
            camControl.Shake(0.25f);
        }
        if (timer > 4f) {
            // switch scenes
            complete = true;
            GameManager.Instance.data.entryID = 1;
            SceneManager.LoadScene("space");
        }
    }
}