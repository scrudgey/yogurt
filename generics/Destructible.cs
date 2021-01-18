using UnityEngine;

public class Destructible : Damageable, ISaveable {
    public float maxHealth;
    public float health;
    public AudioClip[] hitSound;
    public AudioClip[] destroySound;
    public float physicalMultiplier = 1f;
    public float fireMultiplier = 1f;
    public float cosmicMultiplier = 1f;
    public float explosionMultiplier = 2f;
    public AudioSource audioSource;
    public override void Awake() {
        base.Awake();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public override void NetIntrinsicsChanged(MessageNetIntrinsic intrins) {
        if (intrins.netBuffs[BuffType.bonusHealth].floatValue > netBuffs[BuffType.bonusHealth].floatValue) {
            health += intrins.netBuffs[BuffType.bonusHealth].floatValue;
        }
        base.NetIntrinsicsChanged(intrins);
    }

    override protected void Update() {
        base.Update();
        if (health < 0) {
            Die();
        }
        if (health > maxHealth + netBuffs[BuffType.bonusHealth].floatValue) {
            health = maxHealth + netBuffs[BuffType.bonusHealth].floatValue;
        }
    }
    public override void CalculateDamage(MessageDamage message) {
        float damage = message.amount;
        switch (message.type) {
            case damageType.acid:
            case damageType.piercing:
            case damageType.cutting:
            case damageType.physical:
                damage = message.amount * physicalMultiplier;
                break;
            case damageType.fire:
                damage = message.amount * fireMultiplier;
                break;
            case damageType.asphyxiation:
                damage = 0;
                break;
            case damageType.cosmic:
                damage = message.amount * cosmicMultiplier;
                break;
            case damageType.explosion:
                damage = message.amount * explosionMultiplier;
                break;
            default:
                break;
        }
        // Debug.Log($"{gameObject.name} > {health} taking damage: {damage} {message.type}");
        health -= damage;
    }
    //TODO: make destruction chaos somehow proportional to object
    public void Die() {
        Destruct();
        if (destroySound.Length > 0) {
            Toolbox.Instance.AudioSpeaker(destroySound[Random.Range(0, destroySound.Length)], transform.position);
        }
        LiquidContainer container = GetComponent<LiquidContainer>();
        if (container && container.amount > 0) {
            Toolbox.Instance.SpawnDroplet(transform.position, container.liquid);
            Toolbox.Instance.SpawnDroplet(transform.position, container.liquid);
            Toolbox.Instance.SpawnDroplet(transform.position, container.liquid);
        }

        Toolbox.Instance.OccurenceFlag(
            gameObject,
            EventData.Destruction(gameObject, lastAttacker, lastMessage)
            );

        if (lastMessage.type == damageType.fire) {
            if (Toolbox.Instance.CloneRemover(name) == "dollar") {
                GameManager.Instance.IncrementStat(StatType.dollarsBurned, 1);
            }
        }
        if (lastMessage.type == damageType.fire) {
            if (Toolbox.Instance.CloneRemover(name) == "book") {
                GameManager.Instance.IncrementStat(StatType.booksBurned, 1);
            }
        }
    }
    void OnCollisionEnter2D(Collision2D col) {
        // Debug.Log(coll.gameObject.name);
        if (col.gameObject.name.ToLower().Contains("chemical spray"))
            return;
        float vel = col.relativeVelocity.magnitude;
        if (vel > 0.5f) {
            //if we were hit hard, take a splatter damage
            //TODO: i should fix this to be more physical
            if (col.rigidbody) {
                float damage = col.rigidbody.mass * vel / 5.0f;
                MessageDamage message = new MessageDamage();
                message.amount = damage;
                message.type = damageType.physical;
                message.force = col.relativeVelocity;
                TakeDamage(message);
                // Debug.Log("Collision damage on " + gameObject.name + " to the tune of " + damage.ToString());
            } else {
                Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
                float damage = rigidbody.mass * vel / 5.0f;
                MessageDamage message = new MessageDamage();
                message.amount = damage;
                message.type = damageType.physical;
                message.force = col.relativeVelocity;
                TakeDamage(message);
                // Debug.Log("Collision damage on " + gameObject.name + " to the tune of " + damage.ToString());
            }
        }

        if (vel > 0.1f && hitSound.Length > 0) {
            // GetComponent<AudioSource>().PlayOneShot(hitSound[Random.Range(0, hitSound.Length)]);
            AudioClip clip = hitSound[Random.Range(0, hitSound.Length)];
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }
    }

    public void SaveData(PersistentComponent data) {
        data.floats["health"] = health;
        // data.ints["lastDamage"] = (int)lastMessage;
    }
    public void LoadData(PersistentComponent data) {
        health = data.floats["health"];
        // lastDamage = (damageType)data.ints["lastDamage"];
    }
}
