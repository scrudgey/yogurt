using UnityEngine;
using System.Collections.Generic;
public class Flammable : MonoBehaviour, ISaveable {
    public float heat;
    public float flashpoint;
    public bool onFire;
    public bool fireSource;
    public ParticleSystem smoke;
    public ParticleSystem fireParticles;
    private CircleCollider2D fireRadius;
    private AudioClip[] igniteSounds = new AudioClip[2];
    private AudioClip burnSounds;
    private AudioSource audioSource;
    private float flagTimer;
    public GameObject responsibleParty;
    public bool playSounds = true;
    public Pickup pickup;
    public float fireRetardantBuffer = 2f;
    public bool fireproof;
    public bool silent; // if true, flammable will not generate occurrence flags
    // public bool coldFire;
    private float burnTimer;
    public void SetBurnTimer() {
        burnTimer = 0.1f;
    }
    void Start() {
        pickup = GetComponent<Pickup>();

        //add the particle effect and set its position
        Vector3 flamePosition = transform.position;
        Transform flamepoint = transform.Find("flamepoint");
        if (flamepoint != null) {
            flamePosition = flamepoint.position;
        }
        GameObject thing = Instantiate(Resources.Load("particles/smoke"), flamePosition, Quaternion.identity) as GameObject;
        smoke = thing.GetComponent<ParticleSystem>();
        smoke.transform.parent = transform;
        thing = Instantiate(Resources.Load("particles/fire"), flamePosition, Quaternion.identity) as GameObject;
        fireParticles = thing.GetComponent<ParticleSystem>();
        fireParticles.transform.parent = transform;

        //add the fire object - can pare this down probably
        GameObject fireChild = new GameObject();
        fireChild.transform.position = transform.position;
        fireChild.transform.parent = transform;
        fireChild.tag = "fire";
        Rigidbody2D fireBody = fireChild.AddComponent<Rigidbody2D>();
        fireBody.isKinematic = true;
        fireBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
        Fire fire = fireChild.AddComponent<Fire>();
        fire.flammable = this;
        fireRadius = fireChild.AddComponent<CircleCollider2D>();
        fireRadius.isTrigger = true;
        fireRadius.radius = 0.1f;
        fireRadius.name = "fire";
        fire.gameObject.layer = 13;
        //ensure that there is a speaker
        audioSource = Toolbox.Instance.SetUpAudioSource(fireChild);
        burnSounds = Resources.Load("sounds/Crackling Fire", typeof(AudioClip)) as AudioClip;
        igniteSounds[0] = Resources.Load("sounds/Flash Fire Ignite 01", typeof(AudioClip)) as AudioClip;
        igniteSounds[1] = Resources.Load("sounds/Flash Fire Ignite 02", typeof(AudioClip)) as AudioClip;

        Toolbox.RegisterMessageCallback<MessageDamage>(this, HandleDamageMessage);
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsic);
    }
    void HandleNetIntrinsic(MessageNetIntrinsic message) {
        // netBuffs = message.netBuffs;
        if (message.netBuffs[BuffType.fireproof].boolValue || message.netBuffs[BuffType.fireproof].floatValue > 0) {
            fireproof = true;
        } else {
            fireproof = false;
        }
    }
    public void HandleDamageMessage(MessageDamage message) {
        // TODO: rate limit this step
        if (message.type == damageType.fire) {
            // heat += message.amount;
            burnTimer = 1f;
            responsibleParty = message.responsibleParty;
        }
        if (message.type == damageType.asphyxiation) {
            heat = -999f;
            onFire = false;
        }
    }
    void Update() {
        if (burnTimer > 0) {
            burnTimer -= Time.deltaTime;
            heat += 2f * Time.deltaTime;
        }
        if (fireSource) {
            onFire = true;
            heat = 100;
        }
        if (fireproof && heat > 1)
            heat = 1;
        if (fireproof && onFire) {
            onFire = false;
        }
        if (heat > (-1f * fireRetardantBuffer) && !onFire) {
            heat -= Time.deltaTime;
        }
        if (heat < (-1f * fireRetardantBuffer - 1f)) {
            heat += Time.deltaTime;
        }
        if (heat <= (-1f * fireRetardantBuffer) && smoke.isPlaying) {
            smoke.Stop();
        }
        if (heat > 1 && smoke.isStopped) {
            smoke.Play();
        }
        if (heat > flashpoint && fireParticles.isStopped && !fireproof) {
            fireParticles.Play();
            onFire = true;
            if (playSounds) {
                audioSource.PlayOneShot(igniteSounds[Random.Range(0, 1)]);
                audioSource.loop = true;
                audioSource.clip = burnSounds;
                audioSource.Play();
            }
            OccurrenceFire fireData = new OccurrenceFire();
            fireData.flamingObject = gameObject;
            Toolbox.Instance.OccurenceFlag(gameObject, fireData);
            if (responsibleParty == GameManager.Instance.playerObject && gameObject != GameManager.Instance.playerObject) {
                GameManager.Instance.IncrementStat(StatType.othersSetOnFire, 1);
            }
        }
        if (onFire) {
            if (pickup) {
                if (pickup.holder != null) {
                    responsibleParty = pickup.holder.gameObject;
                }
            }
            flagTimer += Time.deltaTime;
            if (flagTimer > 0.5f) {
                flagTimer = 0;
                if (!fireSource && !silent) {
                    OccurrenceFire fireData = new OccurrenceFire();
                    fireData.flamingObject = gameObject;
                    Toolbox.Instance.OccurenceFlag(gameObject, fireData);
                }
            }
            // if i am on fire, i take damage.
            MessageDamage message = new MessageDamage(0.55f, damageType.fire);
            message.responsibleParty = gameObject;
            Toolbox.Instance.SendMessage(gameObject, this, message, sendUpwards: false);
            if (Random.Range(0, 300f) < 1) {
                string name = Toolbox.Instance.GetName(gameObject);
                MessageSpeech speechMessage = new MessageSpeech("this " + name + " is hot!");
                Toolbox.Instance.SendMessage(gameObject, this, speechMessage, sendUpwards: true);
            }
        } else {
            if (fireParticles.isPlaying) {
                fireParticles.Stop();
                audioSource.Stop();
            }
        }
    }
    public void SaveData(PersistentComponent data) {
        data.floats["heat"] = heat;
        data.floats["flashpoint"] = flashpoint;
        data.bools["onFire"] = onFire;
        data.bools["fireproof"] = fireproof;
    }
    public void LoadData(PersistentComponent data) {
        heat = data.floats["heat"];
        flashpoint = data.floats["flashpoint"];
        onFire = data.bools["onFire"];
        if (data.bools.ContainsKey("fireproof"))
            fireproof = data.bools["fireproof"];
    }
}
