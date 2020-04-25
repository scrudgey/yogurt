using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapDoor : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public SpriteMask mask;
    public AudioClip activeSound;
    public AudioClip leaveSound;
    public List<ParticleSystem> particles;
    public bool active = false;
    private float timer;
    public void Activate() {
        active = true;
        spriteRenderer.enabled = true;
        GameManager.Instance.publicAudio.PlayOneShot(activeSound);
        mask.enabled = true;
        foreach (ParticleSystem system in particles) {
            system.Stop();
            system.Play();
        }
    }
    void OnTriggerEnter2D(Collider2D collider) {
        Exit(collider);
    }
    void OnTriggerStay2D(Collider2D collider) {
        Exit(collider);
    }
    void Update() {
        if (timer > 0) {
            timer += Time.deltaTime;
            if (timer > 3f) {
                GameManager.Instance.publicAudio.PlayOneShot(leaveSound);
                GameManager.Instance.LeaveScene("dungeon", 0);
            }
        }
    }
    void Exit(Collider2D collider) {
        if (!active)
            return;
        if (collider.gameObject == GameManager.Instance.playerObject) {
            InputController.Instance.state = InputController.ControlState.cutscene;
            UINew.Instance.RefreshUI(active: false);
            timer = 0.1f;
            collider.gameObject.SetActive(false);
            Toolbox.Instance.SwitchAudioListener(gameObject);
        } else {
            collider.gameObject.SetActive(false);
        }
    }
}
