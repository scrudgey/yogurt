using UnityEngine;

public class Blender : Container, ISaveable {
    public bool power;
    private bool vibrate;
    public Sprite[] spriteSheet;
    private SpriteRenderer spriteRenderer;
    public AudioClip blendStart;
    public AudioClip blendNoise;
    public AudioClip blendStop;
    public AudioClip lidOn;
    public AudioClip lidOff;
    private LiquidContainer liquidContainer;
    private AudioSource audioSource;
    private PhysicalBootstrapper pb;
    protected override void Awake() {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        liquidContainer = GetComponent<LiquidContainer>();
        PopulateContentActions();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        pb = GetComponent<PhysicalBootstrapper>();
    }
    protected override void PopulateContentActions() {
        base.PopulateContentActions();
        Interaction powerAct = new Interaction(this, "Power", "Power");
        Interaction lidAct = new Interaction(this, "Lid", "Lid");
        powerAct.holdingOnOtherConsent = false;
        lidAct.holdingOnOtherConsent = false;

        interactions.Add(powerAct);
        interactions.Add(lidAct);
    }
    void FixedUpdate() {
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

            //blend contained item
            if (items.Count > 0) {
                MessageDamage message = new MessageDamage();
                message.suppressGibs = true;
                message.amount = Time.deltaTime * 10;
                message.force = Vector2.up;
                message.type = damageType.physical;
                message.suppressImpactSound = true;

                Toolbox.Instance.SendMessage(items[0].gameObject, this, message, false);
            }
            if (liquidContainer.amount > 0 && !liquidContainer.lid) {
                liquidContainer.Spill(1f);
            }
        }
    }
    public void Power() {
        power = !power;
        if (power) {
            GetComponent<AudioSource>().clip = blendStart;
            GetComponent<AudioSource>().Play();
        } else {
            GetComponent<AudioSource>().clip = blendStop;
            GetComponent<AudioSource>().Play();
        }
    }
    public string Power_desc() {
        if (power) {
            return "Turn off power";
        } else {
            return "Turn on power";
        }
    }
    public void Lid() {
        liquidContainer.lid = !liquidContainer.lid;
        if (liquidContainer.lid) {
            spriteRenderer.sprite = spriteSheet[0];
            audioSource.PlayOneShot(lidOn);
        } else {
            spriteRenderer.sprite = spriteSheet[1];
            audioSource.PlayOneShot(lidOff);
        }
    }
    public string Lid_desc() {
        if (liquidContainer.lid) {
            return "Remove lid";
        } else {
            return "Put on lid";
        }
    }
    public override void Store(Inventory inv) {
        if (liquidContainer.lid) {
            Toolbox.Instance.SendMessage(inv.gameObject, this, new MessageSpeech("The lid is on!"));
        } else {
            base.Store(inv);
        }
    }
    public override void Remove(Inventory inv, Pickup pickup) {
        base.Remove(inv, pickup);
        if (power) {
            MessageDamage message = new MessageDamage(5f, damageType.cutting);
            Toolbox.Instance.SendMessage(inv.gameObject, this, message);
            MessageSpeech speech = new MessageSpeech("Ouch!");
            Toolbox.Instance.SendMessage(inv.gameObject, this, speech);
        }
    }
    public override void WasDestroyed(GameObject obj) {
        base.WasDestroyed(obj);
        Edible edible = obj.GetComponent<Edible>();
        if (edible && edible.blendable) {
            liquidContainer.FillWithLiquid(edible.Liquify());
        }
        // Gibs[] gibses = obj.GetComponents<Gibs>();
        // foreach (Gibs gibs in gibses) {
        //     if (gibs.notPhysical)
        //         continue;
        //     // EmitParticle(gibs.particle);
        //     Destroy(gibs);
        // }
    }
    public void EmitParticle(GameObject particle) {
        GameObject bit = Instantiate(particle, transform.position, Quaternion.identity) as GameObject;
        PhysicalBootstrapper bitPhys = Toolbox.GetOrCreateComponent<PhysicalBootstrapper>(bit);
        PhysicalBootstrapper myBoot = GetComponent<PhysicalBootstrapper>();
        bitPhys.impactsMiss = true;
        bitPhys.noCollisions = true;
        if (bitPhys.size == PhysicalBootstrapper.shadowSize.normal)
            bitPhys.size = PhysicalBootstrapper.shadowSize.medium;
        bitPhys.initHeight = Random.Range(0, 0.05f);
        if (myBoot) {
            if (myBoot.physical != null) {
                bitPhys.initHeight = myBoot.physical.height;
            }
        }
        Vector3 force = 3f * transform.up;
        force.z = Random.Range(2f, 3f);
        bitPhys.Set3Motion(force);
    }
    public override void SaveData(PersistentComponent data) {
        base.SaveData(data);
        data.bools["power"] = power;
        data.bools["lid"] = liquidContainer.lid;
    }
    public override void LoadData(PersistentComponent data) {
        base.LoadData(data);
        power = data.bools["power"];
        if (data.bools["lid"]) {
            spriteRenderer.sprite = spriteSheet[0];
        } else {
            spriteRenderer.sprite = spriteSheet[1];
        }
    }
}
