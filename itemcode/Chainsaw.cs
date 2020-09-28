using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chainsaw : Interactive, ISaveable {
    public bool power;
    private bool vibrate;
    public Sprite[] spriteSheet;
    public int spriteIndex;
    public SpriteRenderer spriteRenderer;
    public AudioClip blendStart;
    public AudioClip blendNoise;
    public AudioClip blendStop;
    private AudioSource audioSource;
    private PhysicalBootstrapper pb;
    private float damageTimer;
    public ParticleSystem smoke;
    private HashSet<GameObject> damageQueue = new HashSet<GameObject>();
    public MessageDamage damageMessage = new MessageDamage(25f, damageType.cutting);
    public Pickup pickup;
    void Awake() {
        // spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        pb = GetComponent<PhysicalBootstrapper>();

        Interaction powerAct = new Interaction(this, "Power", "Power");
        powerAct.holdingOnOtherConsent = false;
        powerAct.defaultPriority = 1;
        // powerAct.
        pickup = GetComponent<Pickup>();

        interactions.Add(powerAct);
        smoke.Stop();
    }

    void FixedUpdate() {

        damageTimer += Time.deltaTime;
        if (damageTimer > 0.1f) {
            damageTimer = 0;
            foreach (GameObject obj in damageQueue) {
                if (obj == null)
                    continue;
                if (transform.IsChildOf(obj.transform.root))
                    continue;
                if (obj != null) {
                    if (pickup != null && pickup.holder != null) {
                        damageMessage.responsibleParty = pickup.holder.gameObject;
                    } else {
                        damageMessage.responsibleParty = gameObject;
                    }

                    OccurrenceViolence violence = new OccurrenceViolence();
                    violence.amount = damageMessage.amount;
                    violence.attacker = damageMessage.responsibleParty;
                    violence.victim = obj;
                    violence.lastMessage = damageMessage;

                    if (damageMessage.responsibleParty != null) {
                        Toolbox.Instance.OccurenceFlag(damageMessage.responsibleParty, violence);
                    } else {
                        Toolbox.Instance.OccurenceFlag(gameObject, violence);
                    }

                    Toolbox.Instance.SendMessage(obj, damageMessage.responsibleParty.transform, damageMessage);
                }
            }
            damageQueue = new HashSet<GameObject>();
        }

        if (power) {
            if (!audioSource.isPlaying) {
                audioSource.clip = blendNoise;
                audioSource.Play();
            }

            // vibrate the blender
            // i'm sorry for my garbage code
            Vector3 pos = transform.root.position;
            Vector3 physicalPos = Vector3.zero;
            if (pb != null && pb.physical != null) {
                physicalPos = pb.physical.transform.position;
            }
            vibrate = !vibrate;
            if (vibrate) {
                pos.y += 0.01f;
                if (pb != null && pb.physical != null) {
                    physicalPos.y += 0.01f;
                }
            } else {
                pos.y -= 0.01f;
                if (pb != null && pb.physical != null) {
                    physicalPos.y -= 0.01f;
                }
            }
            transform.root.position = pos;
            if (pb != null && pb.physical != null) {
                pb.physical.transform.position = physicalPos;
            }

            // cycle sprite
            spriteIndex += 1;
            if (spriteIndex >= spriteSheet.Length) {
                spriteIndex = 0;
            }
            spriteRenderer.sprite = spriteSheet[spriteIndex];
        }
    }
    public void Power() {
        power = !power;
        if (power) {
            GetComponent<AudioSource>().clip = blendStart;
            GetComponent<AudioSource>().Play();
            smoke.Play();
        } else {
            GetComponent<AudioSource>().clip = blendStop;
            GetComponent<AudioSource>().Play();
            smoke.Stop();
        }
    }
    public string Power_desc() {
        if (power) {
            return "Stop chainsaw";
        } else {
            return "Start chainsaw";
        }
    }
    public void SaveData(PersistentComponent data) {
        data.bools["power"] = power;
    }
    public void LoadData(PersistentComponent data) {
        power = data.bools["power"];
    }

    public void OnTriggerStay2D(Collider2D other) {
        if (!power)
            return;
        if (InputController.forbiddenTags.Contains(other.tag))
            return;
        if (other.transform.IsChildOf(transform.root))
            return;
        damageQueue.Add(other.gameObject);
    }
    public void StartSwingWeapon() {
        // if (weapon.swingRotation != 0) {
        Vector2 leftTiltVector = Vector2.zero;
        leftTiltVector.x = Mathf.Cos(1.57f - (-90 * 6.28f / 360f));
        leftTiltVector.y = Mathf.Sin(1.57f - (-90 * 6.28f / 360f));
        spriteRenderer.transform.localRotation = Quaternion.LookRotation(Vector3.forward, leftTiltVector);
        Debug.Log(spriteRenderer.transform.localRotation);
        // }
    }
    public void EndSwingWeapon() {
        spriteRenderer.transform.localRotation = Quaternion.identity;
        Debug.Log(spriteRenderer.transform.localRotation);
        // }
    }
}
