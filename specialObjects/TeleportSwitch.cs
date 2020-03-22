using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSwitch : Interactive {
    public Sprite upSprite;
    public Sprite downSprite;
    public bool on;
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D pad1;
    public BoxCollider2D pad2;
    public AudioClip teleportSound;
    private AudioSource audioSource;
    private bool doTeleport;
    public GameObject teleportFx;
    public void Awake() {
        Interaction teleport = new Interaction(this, "Throw switch", "Teleport");
        interactions.Add(teleport);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public void Teleport() {
        on = !on;
        if (on)
            spriteRenderer.sprite = upSprite;
        else spriteRenderer.sprite = downSprite;
        // do teleport
        audioSource.PlayOneShot(teleportSound);
        doTeleport = true;
    }
    public string Teleport_desc() {
        return "Throw switch";
    }
    void FixedUpdate() {
        if (doTeleport) {
            doTeleport = false;
            HashSet<GameObject> pad1Objects = new HashSet<GameObject>();
            HashSet<GameObject> pad2Objects = new HashSet<GameObject>();
            foreach (Transform obj in GameObject.FindObjectsOfType<Transform>()) {

                if (Controller.forbiddenTags.Contains(obj.tag))
                    continue;

                if (pad1.bounds.Contains(obj.position)) {
                    pad1Objects.Add(Controller.Instance.GetBaseInteractive(obj));
                }
                if (pad2.bounds.Contains(obj.position)) {
                    pad2Objects.Add(Controller.Instance.GetBaseInteractive(obj));
                }
            }

            pad1Objects.Remove(pad1.gameObject);
            pad2Objects.Remove(pad2.gameObject);

            foreach (GameObject obj in pad1Objects) {
                GameObject.Instantiate(teleportFx, obj.transform.position, Quaternion.identity);
                Vector3 relativePos = obj.transform.position - pad1.transform.position;
                obj.transform.position = pad2.transform.position + relativePos;
            }

            foreach (GameObject obj in pad2Objects) {
                GameObject.Instantiate(teleportFx, obj.transform.position, Quaternion.identity);
                Vector3 relativePos = obj.transform.position - pad2.transform.position;
                obj.transform.position = pad1.transform.position + relativePos;
            }

        }
    }
}
