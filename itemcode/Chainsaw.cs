using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chainsaw : Pickup, ISaveable {
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
    // public Pickup pickup;
    public float revTimer;
    public AudioClip revStart;
    public AudioClip rev;
    public AudioClip revStop;
    void Awake() {
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        pb = GetComponent<PhysicalBootstrapper>();

        Interaction spray = new Interaction(this, "Power", "Rev");
        spray.defaultPriority = 2;
        spray.continuous = true;
        spray.dontWipeInterface = true;
        spray.otherOnSelfConsent = false;
        spray.selfOnOtherConsent = false;
        interactions.Add(spray);

        smoke.Stop();
    }
    public void Rev() {
        revTimer = 0.5f;
    }

    void FixedUpdate() {
        bool powerValue = holder != null;
        if (power != powerValue) {
            power = powerValue;
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

        damageTimer += Time.fixedDeltaTime;
        if (damageTimer > 0.1f) {
            damageTimer = 0;
            foreach (GameObject obj in damageQueue) {
                if (obj == null)
                    continue;
                if (transform.IsChildOf(obj.transform.root))
                    continue;
                if (InputController.forbiddenTags.Contains(obj.tag))
                    continue;
                if (obj != null) {
                    if (holder != null) {
                        damageMessage.responsibleParty = holder.gameObject;
                    } else {
                        damageMessage.responsibleParty = gameObject;
                    }
                    damageMessage.weaponName = "a chainsaw";
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
                    damageMessage.suppressImpactSound = true;
                    Toolbox.Instance.SendMessage(obj, damageMessage.responsibleParty.transform, damageMessage);
                }
            }
            damageQueue = new HashSet<GameObject>();
        }
        if (revTimer > 0) {
            if (audioSource.clip != rev) {// && audioSource.clip != revStop) {
                audioSource.clip = rev;
                audioSource.Play();
            }
            if (!audioSource.isPlaying && revTimer > 0) {
                audioSource.clip = rev;
                audioSource.Play();
            }
            revTimer -= Time.deltaTime;
            if (revTimer <= 0) {
                audioSource.clip = revStop;
                audioSource.Play();
            }
            DoVibrate();
        }
        if (power) {
            if (!audioSource.isPlaying) {
                audioSource.clip = blendNoise;
                audioSource.Play();
            }
            // cycle sprite
            spriteIndex += 1;
            if (spriteIndex >= spriteSheet.Length) {
                spriteIndex = 0;
            }
            spriteRenderer.sprite = spriteSheet[spriteIndex];
        }
    }
    void DoVibrate() {
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
        if (!(revTimer > 0))
            return;
        if (InputController.forbiddenTags.Contains(other.tag))
            return;
        if (other.transform.IsChildOf(transform.root))
            return;
        damageQueue.Add(other.gameObject);
    }
    // public void StartSwingWeapon() {
    //     Vector2 leftTiltVector = Vector2.zero;
    //     leftTiltVector.x = Mathf.Cos(1.57f - (-90 * 6.28f / 360f));
    //     leftTiltVector.y = Mathf.Sin(1.57f - (-90 * 6.28f / 360f));
    //     spriteRenderer.transform.localRotation = Quaternion.LookRotation(Vector3.forward, leftTiltVector);
    //     Debug.Log(spriteRenderer.transform.localRotation);
    // }
    // public void EndSwingWeapon() {
    //     spriteRenderer.transform.localRotation = Quaternion.identity;
    //     Debug.Log(spriteRenderer.transform.localRotation);
    // }
}
