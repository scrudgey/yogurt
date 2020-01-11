using UnityEngine;

public class Destructible : Damageable, ISaveable {
    public float maxHealth;
    public float health;
    public float bonusHealth;
    public AudioClip[] hitSound;
    public AudioClip[] destroySound;
    public float physicalMultiplier = 1f;
    public float fireMultiplier = 1f;
    public AudioSource audioSource;
    public void Start() {
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public override void NetIntrinsicsChanged(MessageNetIntrinsic intrins) {
        if (intrins.netBuffs[BuffType.bonusHealth].floatValue > bonusHealth) {
            health += intrins.netBuffs[BuffType.bonusHealth].floatValue;
        }
        bonusHealth = intrins.netBuffs[BuffType.bonusHealth].floatValue;
    }

    override protected void Update() {
        base.Update();
        if (health < 0) {
            Die();
        }
        if (health > maxHealth + bonusHealth) {
            health = maxHealth + bonusHealth;
        }
    }
    public override float CalculateDamage(MessageDamage message) {
        float damage = 0f;
        switch (message.type) {
            case damageType.piercing:
            case damageType.cutting:
            case damageType.physical:
                damage = message.amount * physicalMultiplier;
                break;
            case damageType.fire:
                damage = message.amount * fireMultiplier;
                break;
            case damageType.explosion:
            case damageType.cosmic:
                damage = message.amount;
                break;
            case damageType.asphyxiation:
            default:
                break;
        }
        // Debug.Log(gameObject.name+ "> " + health.ToString()+ " taking damage: "+damage.ToString());
        health -= damage;
        return damage;
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
        EventData data = Toolbox.Instance.DataFlag(gameObject, chaos: Random.Range(1, 2));
        data.noun = "destruction";
        data.whatHappened = Toolbox.Instance.CloneRemover(gameObject.name) + " was destroyed";
        if (lastMessage.type == damageType.fire) {
            if (Toolbox.Instance.CloneRemover(name) == "dollar") {
                GameManager.Instance.IncrementStat(StatType.dollarsBurned, 1);
            }
        }
    }
    void OnCollisionEnter2D(Collision2D col) {
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
            audioSource.PlayOneShot(hitSound[Random.Range(0, hitSound.Length)]);
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
