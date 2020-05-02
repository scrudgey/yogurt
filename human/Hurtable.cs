using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class Hurtable : Damageable, ISaveable {
    private Controllable.HitState _hitState;
    public Controllable.HitState hitState {
        get { return _hitState; }
        set {
            // if value has changed, send a message:
            if (value != _hitState) {
                _hitState = value;
                MessageHitstun message = new MessageHitstun();
                message.hitState = value;
                Toolbox.Instance.SendMessage(gameObject, this, message);
            }
        }
    }
    public float health;
    public float maxHealth;
    public float oxygen;
    public float maxOxygen = 100f;
    public float bonusHealth;
    public float armor;
    public bool coughing;
    public bool ethereal;
    private float hitStunCounter;
    private bool doubledOver;
    public float impulse;
    public float downedTimer;
    private float ouchFrequency = 0.1f;
    public GameObject dizzyEffect;
    public float timeSinceLastCough;
    bool vibrate;
    public bool bleeds = true; // if true, bleed on cutting / piercing damage
    public bool monster = false; // if true, death of this hurtable is not shocking or disturbing
    public Dictionary<damageType, float> totalDamage = new Dictionary<damageType, float>();
    public void Reset() {
        health = maxHealth;
        oxygen = maxOxygen;
        impulse = 0;
        downedTimer = 0;
        hitState = Controllable.HitState.none;
        hitStunCounter = 0;
    }
    public override void Awake() {
        base.Awake();
        oxygen = maxOxygen;
        Toolbox.RegisterMessageCallback<MessageHead>(this, HandleHead);
        Toolbox.RegisterMessageCallback<MessageStun>(this, HandleStun);
    }
    void HandleHead(MessageHead head) {
        if (head.type == MessageHead.HeadType.vomiting) {
            DoubleOver(head.value);
            if (head.value) {
                impulse = 35f;
            }
        }
    }
    void HandleStun(MessageStun message) {
        hitState = Controllable.AddHitState(hitState, Controllable.HitState.stun);
        hitStunCounter = message.timeout;
    }
    public override void NetIntrinsicsChanged(MessageNetIntrinsic intrins) {
        if (intrins.netBuffs[BuffType.bonusHealth].floatValue > bonusHealth) {
            health += intrins.netBuffs[BuffType.bonusHealth].floatValue;
        }
        bonusHealth = intrins.netBuffs[BuffType.bonusHealth].floatValue;
        armor = intrins.netBuffs[BuffType.armor].floatValue;
        coughing = intrins.netBuffs[BuffType.coughing].boolValue;
        if (intrins.netBuffs[BuffType.death].active()) {
            health = float.MinValue;
            Die(damageType.physical);
        }
    }
    public override float CalculateDamage(MessageDamage message) {
        if (message.type == damageType.asphyxiation) {
            oxygen -= message.amount;
            if (oxygen <= 0) {
                Die(damageType.asphyxiation);
            }
            return 0;
        }
        float damage = 0;
        float armor = this.armor;
        if (message.strength || hitState == Controllable.HitState.dead)
            armor = 0;
        switch (message.type) {
            case damageType.piercing:
            case damageType.cutting:
                if (Mathf.Max(message.amount - armor, 0) > 0) {
                    Bleed(transform.position, message.force.normalized);
                }
                goto case damageType.physical;
            default:
            case damageType.physical:
                damage = Mathf.Max(message.amount - armor, 0);
                if (message.strength) {
                    impulse += damage * 2;
                } else {
                    impulse += damage;
                }
                break;
            case damageType.fire:
                damage = message.amount * 10;
                break;
            case damageType.cosmic:
                damage = message.amount;
                break;
        }
        health -= damage;

        if (!totalDamage.ContainsKey(message.type)) {
            totalDamage[message.type] = 0;
        }
        totalDamage[message.type] += damage;

        // if the damage is fire or cutting, we die at health 0
        if (health <= 0 && (message.type == damageType.fire || message.type == damageType.cutting)) {
            Die(message.type);
        }
        // otherwise, we die at health -50%
        if (health <= -0.4 * maxHealth) {
            damageType maxDamageType = totalDamage.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            Die(maxDamageType);
        }
        // the corpse can still be destroyed after that
        if (health <= -0.6 * maxHealth && message.type == damageType.cosmic) {
            // TODO: cosmic vaporization effect
            Destruct();
            EventData data = Toolbox.Instance.DataFlag(
                gameObject,
                "vaporization",
                "the corpse of " + Toolbox.Instance.GetName(gameObject) + " was vaporized",
                chaos: 3,
                disturbing: 4,
                disgusting: 4,
                positive: -2,
                offensive: -2);
        }
        if (health <= -0.6 * maxHealth && message.type == damageType.cutting) {
            Destruct();
            if (!monster) {
                EventData data = Toolbox.Instance.DataFlag(
                    gameObject,
                    "corpse desecration",
                    "the corpse of " + Toolbox.Instance.GetName(gameObject) + " was desecrated",
                    chaos: 3,
                    disturbing: 4,
                    disgusting: 4,
                    positive: -2,
                    offensive: -2);
                data.key = "corpse desecration";
                data.val = 1;
                data.popupDesc = "corpses desecrated";
            }
        }
        if (message.type != damageType.fire && message.type != damageType.asphyxiation) {
            hitState = Controllable.AddHitState(hitState, Controllable.HitState.stun);
            hitStunCounter = Random.Range(0.2f, 0.25f);
        }
        if (damage > 0 && Random.Range(0.0f, 1.0f) < ouchFrequency) {
            MessageSpeech speechMessage = new MessageSpeech();
            speechMessage.nimrod = true;
            speechMessage.interrupt = true;
            switch (message.type) {
                case damageType.physical:
                    speechMessage.phrase = "{pain-physical}";
                    break;
                case damageType.fire:
                    speechMessage.phrase = "{pain-fire}";
                    break;
                default:
                    speechMessage.phrase = "{pain-physical}";
                    break;
            }
            Toolbox.Instance.SendMessage(gameObject, this, speechMessage);
        }
        return damage;
    }

    public void Die(damageType type) {
        if (hitState == Controllable.HitState.dead)
            return;

        Inventory inv = GetComponent<Inventory>();
        if (inv) {
            inv.DropItem();
        }
        if (type == damageType.cosmic || type == damageType.fire) {
            Instantiate(Resources.Load("prefabs/skeleton"), transform.position, transform.rotation);
            Toolbox.Instance.AudioSpeaker("Flash Fire Ignite 01", transform.position);
            ClaimsManager.Instance.WasDestroyed(gameObject);
            Destroy(gameObject);
        } else if (type == damageType.explosion) {
            Destruct();
        } else {
            KnockDown();
        }

        LogTypeOfDeath(type);
        if (dizzyEffect != null) {
            ClaimsManager.Instance.WasDestroyed(dizzyEffect);
            Destroy(dizzyEffect);
        }
        hitState = Controllable.AddHitState(hitState, Controllable.HitState.dead);
    }

    public void Resurrect() {
        if (hitState != Controllable.HitState.dead)
            return;
        health = maxHealth;
        Reset();
        GetUp();
        Intrinsics hostIntrins = Toolbox.GetOrCreateComponent<Intrinsics>(gameObject);
        // add mindless?
        hostIntrins.AddNewPromotedLiveBuff(new Buff(BuffType.undead, true, 0, 0));
        hostIntrins.AddNewPromotedLiveBuff(new Buff(BuffType.enraged, true, 0, 0));
        hostIntrins.IntrinsicsChanged();
        monster = true;
        Speech speech = GetComponent<Speech>();
        if (speech) {
            speech.enabled = false;
            speech.disableSpeakWith = true;
        }
    }
    public void LogTypeOfDeath(damageType type) {
        bool suicide = lastAttacker == gameObject;
        bool assailant = (lastAttacker != null) && (lastAttacker != gameObject) && !impersonalAttacker;
        // bool assailant = false;

        // TODO: could this logic belong to eventdata / occurrence ?
        if (GameManager.Instance.playerObject == gameObject) {
            if (type == damageType.fire) {
                if (suicide) {
                    GameManager.Instance.IncrementStat(StatType.selfImmolations, 1);
                }
                GameManager.Instance.IncrementStat(StatType.immolations, 1);
            }
            if (type == damageType.asphyxiation) {
                GameManager.Instance.IncrementStat(StatType.deathByAsphyxiation, 1);
            }
            if (type == damageType.explosion) {
                GameManager.Instance.IncrementStat(StatType.deathByExplosion, 1);
            }
            if (impersonalAttacker) {
                GameManager.Instance.IncrementStat(StatType.deathByMisadventure, 1);
            }
            if (assailant) {
                GameManager.Instance.IncrementStat(StatType.deathByCombat, 1);
            }
            GameManager.Instance.PlayerDeath();
        }

        OccurrenceDeath occurrenceData = new OccurrenceDeath(monster);
        occurrenceData.dead = gameObject;
        occurrenceData.suicide = suicide;
        occurrenceData.damageZone = impersonalAttacker;
        occurrenceData.assailant = assailant;
        occurrenceData.lastAttacker = lastAttacker;
        occurrenceData.lastDamage = type;
        Toolbox.Instance.OccurenceFlag(gameObject, occurrenceData);

        if (gameObject.name.StartsWith("greaser")) {
            GameManager.Instance.data.gangMembersDefeated += 1;
            UINew.Instance.UpdateObjectives();
        }
    }

    override protected void Update() {
        base.Update();
        if (impulse > 0) {
            impulse -= Time.deltaTime * 25f;
        }
        if (downedTimer > 0) {
            downedTimer -= Time.deltaTime;
            // if (gameObject == GameManager.Instance.playerObject) {
            //     downedTimer -= Time.deltaTime * 2f;
            // }
            if (downedTimer < 2 & hitState < Controllable.HitState.dead) {
                Vector3 pos = transform.root.position;
                vibrate = !vibrate;
                if (vibrate) {
                    pos.y = pos.y + 0.01f;
                } else {
                    pos.y = pos.y - 0.01f;
                }
                transform.root.position = pos;
            }
        }
        if (hitStunCounter > 0) {
            hitStunCounter -= Time.deltaTime;
            if (hitStunCounter <= 0 && !doubledOver) {
                hitState = Controllable.RemoveHitState(hitState, Controllable.HitState.stun);
            }
        }
        if (health < 0.5 * maxHealth) {
            if (gameObject == GameManager.Instance.playerObject) {
                health += Time.deltaTime * 6f;
            }
        }
        if (oxygen <= maxOxygen) {
            oxygen += 5f * Time.deltaTime;
        }
        if (health <= 0 && hitState < Controllable.HitState.unconscious) {
            KnockDown();
        }
        if (impulse > 120f && hitState < Controllable.HitState.unconscious) {
            KnockDown();
        }
        if (downedTimer <= 0 && hitState == Controllable.HitState.unconscious) { //&& health > 0) {
            GetUp();
        }
        if (impulse > 75f && !doubledOver && hitState < Controllable.HitState.unconscious) {
            DoubleOver(true);
        }
        if (impulse <= 0f && doubledOver && hitState < Controllable.HitState.unconscious) {
            DoubleOver(false);
        }
        if (coughing) {
            timeSinceLastCough -= Time.deltaTime;
            if (timeSinceLastCough <= 0) {
                MessageSpeech speech = new MessageSpeech("{cough}");
                speech.nimrod = true;
                Toolbox.Instance.SendMessage(gameObject, this, speech);
                timeSinceLastCough = 1f;
                impulse = 50f;
                hitStunCounter = 1f;
                DoubleOver(true);
            }
        }
    }
    public void KnockDown() {
        if (hitState >= Controllable.HitState.unconscious)
            return;
        hitState = Controllable.AddHitState(hitState, Controllable.HitState.unconscious);
        doubledOver = false;
        if (gameObject == GameManager.Instance.playerObject) {
            downedTimer = 5f;
        } else {
            downedTimer = 10f;
        }
        Vector3 pivot = transform.position;
        pivot.y -= 0.15f;
        transform.RotateAround(pivot, new Vector3(0, 0, 1), -90);
        MessageHitstun message = new MessageHitstun();
        message.doubledOver = false;
        message.knockedDown = true;
        message.hitState = hitState;
        Toolbox.Instance.SendMessage(gameObject, this, message);

        dizzyEffect = Instantiate(Resources.Load("prefabs/fx/dizzy"), transform.position + new Vector3(0.1f, 0.1f, 0), Quaternion.identity) as GameObject;
        FollowGameObject dizzyFollower = dizzyEffect.GetComponent<FollowGameObject>();
        dizzyFollower.target = gameObject;
        dizzyFollower.Init();
    }
    public void GetUp() {
        // Debug.Log(health);
        if (health < 0.25f * maxHealth)
            if (gameObject == GameManager.Instance.playerObject) {
                health += 0.3f * maxHealth;
            } else {
                health += 0.25f * maxHealth;
            }
        hitState = Controllable.RemoveHitState(hitState, Controllable.HitState.unconscious);
        doubledOver = false;
        Vector3 pivot = transform.position;
        pivot.x -= 0.15f;
        transform.RotateAround(pivot, new Vector3(0, 0, 1), 90);
        if (dizzyEffect != null) {
            ClaimsManager.Instance.WasDestroyed(dizzyEffect);
            Destroy(dizzyEffect);
        }
        if (gameObject != GameManager.Instance.playerObject) {
            hitState = Controllable.HitState.stun;
            hitStunCounter = 0.5f;
        }
    }
    public void DoubleOver(bool val) {
        if (val) {
            hitState = Controllable.AddHitState(hitState, Controllable.HitState.stun);
            doubledOver = true;
            MessageHitstun message = new MessageHitstun();
            message.doubledOver = true;
            message.hitState = hitState;
            Toolbox.Instance.SendMessage(gameObject, this, message);
        } else {
            hitState = Controllable.RemoveHitState(hitState, Controllable.HitState.stun);
            doubledOver = false;
            MessageHitstun message = new MessageHitstun();
            message.doubledOver = false;
            message.hitState = hitState;
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
    }
    public void Bleed(Vector3 position, Vector3 direction) {
        if (!bleeds)
            return;

        Liquid blood = Liquid.LoadLiquid("blood");
        float initHeight = (position.y - transform.position.y) + 0.15f;
        GameObject drop = Toolbox.Instance.SpawnDroplet(blood, 0.3f, gameObject, initHeight, direction.normalized);
        Edible edible = drop.GetComponent<Edible>();
        edible.human = true;
    }
    public void SaveData(PersistentComponent data) {
        data.floats["health"] = health;
        data.floats["maxHealth"] = maxHealth;
        data.floats["bonusHealth"] = bonusHealth;
        data.floats["impulse"] = impulse;
        data.floats["downed_timer"] = downedTimer;
        data.ints["hitstate"] = (int)hitState;
        data.floats["oxygen"] = oxygen;
        data.floats["hitstuncounter"] = hitStunCounter;
    }
    public void LoadData(PersistentComponent data) {
        health = data.floats["health"];
        maxHealth = data.floats["maxHealth"];
        bonusHealth = data.floats["bonusHealth"];
        impulse = data.floats["impulse"];
        downedTimer = data.floats["downed_timer"];
        hitState = (Controllable.HitState)data.ints["hitstate"];
        oxygen = data.floats["oxygen"];
        hitStunCounter = data.floats["hitstuncounter"];
    }
}
