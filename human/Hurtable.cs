using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class Hurtable : Damageable, ISaveable {
    public enum BloodType { blood, oil }
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
    public bool ethereal;
    private float hitStunCounter;
    private bool doubledOver;
    public float impulse;
    public float downedTimer;
    private float ouchFrequency = 0.1f;
    public GameObject dizzyEffect;
    public float timeSinceLastCough;
    bool vibrate;
    public BloodType bloodType;
    public bool bleeds = true; // if true, bleed on cutting / piercing damage
    public bool monster = false; // if true, death of this hurtable is not shocking or disturbing
    public bool ghostly = false; // if true, does not leave a skeleton behind when vaporized
    public Dictionary<damageType, float> totalDamage = new Dictionary<damageType, float>();
    public Collider2D myCollider;
    public bool unknockdownable;
    public bool headExploded;
    private List<AudioClip> knockdownSounds;
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
        myCollider = GetComponent<Collider2D>();
        knockdownSounds = new List<AudioClip>();
        knockdownSounds.Add(Resources.Load("sounds/8bit_impact1") as AudioClip);
        knockdownSounds.Add(Resources.Load("sounds/8bit_impact2") as AudioClip);
        knockdownSounds.Add(Resources.Load("sounds/8bit_impact3") as AudioClip);
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
        if (intrins.netBuffs[BuffType.bonusHealth].floatValue > netBuffs[BuffType.bonusHealth].floatValue) {
            health += intrins.netBuffs[BuffType.bonusHealth].floatValue;
        }
        if (intrins.netBuffs[BuffType.death].active()) {
            health = float.MinValue;
            if (gameObject == GameManager.Instance.playerObject)
                GameManager.Instance.IncrementStat(StatType.deathByPotion, 1);
            // Die(lastMessage, damageType.physical);
            GameManager.Instance.ExplodeHead(gameObject);
        }
        if (intrins.netBuffs[BuffType.undead].active()) {
            if (gibsContainerPrefab != null && gibsContainerPrefab.name == "vampireCorpseGibsContainer") {
                base.NetIntrinsicsChanged(intrins);
                return;
            }
            // switch gibs to undead gibs
            // delete all gibs
            foreach (Gibs gibs in GetComponents<Gibs>()) {
                Destroy(gibs);
            }
            // set new gibs prefab
            gibsContainerPrefab = Resources.Load("prefabs/gibs/undeadGibsContainer") as GameObject;
            // load undead gibs
            LoadGibsPrefab();
        }
        base.NetIntrinsicsChanged(intrins);
    }
    public override void CalculateDamage(MessageDamage message) {
        float damage = message.amount;

        // if armor subtracted from the damage and i am dead, add it back
        if (hitState == Controllable.HitState.dead && !message.strength && netBuffs[BuffType.armor].floatValue > 0)
            message.amount += netBuffs[BuffType.armor].floatValue;

        switch (message.type) {
            case damageType.piercing:
            case damageType.cutting:
                if (message.amount > 0) {
                    Bleed(transform.position, message.force.normalized);
                }
                goto case damageType.physical;
            default:
            case damageType.physical:
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
                impulse += damage;
                damage = message.amount;
                break;
            case damageType.asphyxiation:
                oxygen -= message.amount;
                if (oxygen <= 0) {
                    Die(message, damageType.asphyxiation);
                }
                damage = 0;
                break;
        }

        // side effect
        if (message.type != damageType.fire && message.type != damageType.asphyxiation && message.type != damageType.acid) {
            hitState = Controllable.AddHitState(hitState, Controllable.HitState.stun);
            if (gameObject != GameManager.Instance.playerObject) {
                hitStunCounter = Random.Range(0.2f, 0.25f);
            } else {
                hitStunCounter = Random.Range(0.1f, 0.2f);
            }
        }

        // special effect
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

        // player adjustment
        if (gameObject == GameManager.Instance.playerObject) {
            damage *= 0.75f; // fudge factor
        }

        /**
        **  health adjustment
        **/
        health -= damage;

        if (!totalDamage.ContainsKey(message.type)) {
            totalDamage[message.type] = 0;
        }
        totalDamage[message.type] += damage;

        // if the damage is fire or cutting, we die at health 0
        if (health <= 0 && (message.type == damageType.fire || message.type == damageType.cutting || message.type == damageType.acid)) {
            Die(message, message.type);
        }
        // otherwise, we die at health -50%
        if (health <= -0.4 * maxHealth) {
            damageType maxDamageType = totalDamage.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            Die(message, maxDamageType);
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
    }

    public void Die(MessageDamage message, damageType type /* the overall type of damage TODO: rename */) {
        if (hitState == Controllable.HitState.dead)
            return;
        Inventory inv = GetComponent<Inventory>();
        if (inv) {
            inv.DropItem();
        }
        if (type == damageType.cosmic || type == damageType.fire || type == damageType.acid) {
            if (!ghostly) {
                Instantiate(Resources.Load("prefabs/skeleton"), transform.position, transform.rotation);
                Toolbox.Instance.AudioSpeaker("Flash Fire Ignite 01", transform.position);
            }
            ClaimsManager.Instance.WasDestroyed(gameObject);
            Destruct();
        } else if (type == damageType.explosion) {
            Destruct();
        } else {
            KnockDown();
        }
        hitState = Controllable.AddHitState(hitState, Controllable.HitState.dead);

        LogTypeOfDeath(message);
        if (dizzyEffect != null) {
            ClaimsManager.Instance.WasDestroyed(dizzyEffect);
            Destroy(dizzyEffect);
        }

        gameObject.SendMessage("OnDie", SendMessageOptions.DontRequireReceiver);
    }
    public void OnDestruct() {
        Head head = GetComponentInChildren<Head>();
        if (head != null && head.hat != null) {
            head.RemoveHat();
        }
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
    public void LogTypeOfDeath(MessageDamage message) {
        bool suicide = lastAttacker == gameObject;
        bool assailant = (lastAttacker != null) && (lastAttacker != gameObject) && !impersonalAttacker;

        // TODO: could this logic belong to eventdata / occurrence ?
        if (GameManager.Instance.playerObject == gameObject) {
            if (message != null) {
                if (message.type == damageType.fire) {
                    if (suicide) {
                        GameManager.Instance.IncrementStat(StatType.selfImmolations, 1);
                    }
                }
                if (message.type == damageType.asphyxiation) {
                    GameManager.Instance.IncrementStat(StatType.deathByAsphyxiation, 1);
                }
                if (message.type == damageType.explosion) {
                    GameManager.Instance.IncrementStat(StatType.deathByExplosion, 1);
                }
                if (message.type == damageType.acid) {
                    GameManager.Instance.IncrementStat(StatType.deathByAcid, 1);
                }
                if (message.type == damageType.fire) {
                    GameManager.Instance.IncrementStat(StatType.deathByFire, 1);
                }
            }
            if (impersonalAttacker) {
                GameManager.Instance.IncrementStat(StatType.deathByMisadventure, 1);
            }
            if (assailant) {
                GameManager.Instance.IncrementStat(StatType.deathByCombat, 1);
            }
            GameManager.Instance.PlayerDeath();
        } else {
            if (message != null && message.type == damageType.fire) {
                // TODO: this is wrong?
                GameManager.Instance.IncrementStat(StatType.immolations, 1);
            }
        }

        OccurrenceDeath occurrenceData = new OccurrenceDeath(monster);
        occurrenceData.dead = gameObject;
        occurrenceData.suicide = suicide;
        occurrenceData.damageZone = impersonalAttacker;
        occurrenceData.assailant = assailant;
        occurrenceData.lastAttacker = lastAttacker;
        if (message != null) {
            occurrenceData.lastDamage = message;
        }
        Toolbox.Instance.OccurenceFlag(gameObject, occurrenceData);

        void IncrementStat() {
            GameManager.Instance.data.baddiesDefeated += 1;
            UINew.Instance.UpdateObjectives();
        }

        if (GameManager.Instance.data.activeCommercial != null) {
            Commercial commercial = GameManager.Instance.data.activeCommercial;
            if (commercial.name == "1950s Greaser Beatdown" && gameObject.name.StartsWith("greaser")) {
                IncrementStat();
            }
            if (commercial.name == "Combat II" && gameObject.name.StartsWith("Bruiser")) {
                IncrementStat();
            }
            if (commercial.name == "Combat III" && gameObject.name.StartsWith("BillGhost")) {
                IncrementStat();
            }
            if (commercial.name == "Combat IV" && gameObject.name.StartsWith("Tharr")) {
                IncrementStat();
            }
        }

    }

    override protected void Update() {
        base.Update();
        if (impulse > 0) {
            impulse -= Time.deltaTime * 40f;

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
        if (health < 0.75 * maxHealth) {
            if (gameObject == GameManager.Instance.playerObject) {
                health += Time.deltaTime * 10f;
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
            // Debug.Log($"{gameObject} doubling over {impulse} {doubledOver} {hitState}");
            DoubleOver(true);
        }
        if (impulse <= 0f && doubledOver && hitState < Controllable.HitState.unconscious) {
            DoubleOver(false);
        }
        if (netBuffs[BuffType.coughing].active()) {
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
        if (unknockdownable)
            return;
        if (hitState >= Controllable.HitState.unconscious)
            return;
        hitState = Controllable.AddHitState(hitState, Controllable.HitState.unconscious);
        doubledOver = false;
        if (gameObject == GameManager.Instance.playerObject) {
            downedTimer = 5f;
        } else {
            downedTimer = 10f;
        }
        if (myCollider != null) {
            myCollider.gameObject.layer = LayerMask.NameToLayer("knockdown");
        }
        if (knockdownSounds.Count > 0) {
            AudioClip clip = knockdownSounds[Random.Range(0, knockdownSounds.Count)];
            Toolbox.Instance.AudioSpeaker(clip, transform.position);
        }
        Vector3 pivot = transform.position;
        pivot.y -= 0.15f;
        transform.RotateAround(pivot, new Vector3(0, 0, 1), -90);
        MessageHitstun message = new MessageHitstun();
        message.doubledOver = false;
        message.knockedDown = true;
        message.hitState = hitState;
        Toolbox.Instance.SendMessage(gameObject, this, message);

        dizzyEffect = Instantiate(Resources.Load("particles/dizzy"), transform.position + new Vector3(0.1f, 0.1f, 0), Quaternion.identity) as GameObject;
        FollowGameObject dizzyFollower = dizzyEffect.GetComponent<FollowGameObject>();
        dizzyFollower.target = gameObject;
        dizzyFollower.Init();
        gameObject.SendMessage("OnKnockDown", SendMessageOptions.DontRequireReceiver);
    }
    public void GetUp() {
        // Debug.Log(health);
        if (health < 0.25f * maxHealth)
            if (gameObject == GameManager.Instance.playerObject) {
                health += 0.3f * maxHealth;
            } else {
                health += 0.25f * maxHealth;
            }
        if (myCollider != null) {
            myCollider.gameObject.layer = LayerMask.NameToLayer("feet");
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
        gameObject.SendMessage("OnGetUp", SendMessageOptions.DontRequireReceiver);
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
        if (bleeds) {
            Liquid blood = null;
            switch (bloodType) {
                default:
                case BloodType.blood:
                    blood = Liquid.LoadLiquid("blood");
                    break;
                case BloodType.oil:
                    blood = Liquid.LoadLiquid("oil");
                    break;
            }
            float initHeight = 0;
            if (hitState < Controllable.HitState.unconscious) {
                initHeight = (position.y - transform.position.y) + 0.15f;
            }
            GameObject drop = Toolbox.Instance.SpawnDroplet(blood, 0.3f, gameObject, initHeight, direction.normalized);
            Edible edible = drop.GetComponent<Edible>();
            edible.human = true;
        }
    }
    public void ExplodeHead() {
        if (headExploded)
            return;
        headExploded = true;
        Head head = GetComponentInChildren<Head>();
        MessageDamage message = new MessageDamage(100f, damageType.physical);
        if (head != null) {
            head.RemoveHat();
            Liquid blood = null;
            switch (bloodType) {
                default:
                case BloodType.blood:
                    blood = Liquid.LoadLiquid("blood");
                    break;
                case BloodType.oil:
                    blood = Liquid.LoadLiquid("oil");
                    break;
            }

            GameObject headGibs = Resources.Load("prefabs/gibs/headGibsContainer") as GameObject;
            foreach (Gibs gib in headGibs.GetComponents<Gibs>()) {
                Gibs newGib = head.gameObject.AddComponent<Gibs>();
                newGib.CopyFrom(gib);
                newGib.initHeight = new LoHi(0.05f, 0.2f);
                newGib.initAngleFromHorizontal = new LoHi(0.7f, 0.9f);
                newGib.initVelocity = new LoHi(1f, 2f);
                Vector2 rand = UnityEngine.Random.insideUnitCircle;
                message.force = new Vector3(rand.x * 5f, rand.y * 5f, UnityEngine.Random.Range(5f, 30f));
                List<GameObject> gibs = newGib.Emit(message);
                // apply my blood to the drop dripper
                foreach (GameObject g in gibs) {
                    DropDripper dripper = g.GetComponent<DropDripper>();
                    if (dripper != null) {
                        dripper.liquid = blood;
                        switch (bloodType) {
                            default:
                            case BloodType.blood:
                                dripper.liquidType = "blood";
                                break;
                            case BloodType.oil:
                                dripper.liquidType = "oil";
                                break;
                        }
                    }
                }
            }
            AudioClip boom = Resources.Load("sounds/explosion/cannon") as AudioClip;
            // PlayPublicSound(boom);
            Toolbox.Instance.AudioSpeaker(boom, transform.position);
            Destroy(head.gameObject);
            GameManager.Instance.IncrementStat(StatType.headsExploded, 1);
            EventData headExpldeData = EventData.HeadExplosion(gameObject);
            Toolbox.Instance.OccurenceFlag(head.gameObject, headExpldeData);

            for (int i = 0; i < 10; i++) {
                Vector2 rand = UnityEngine.Random.insideUnitCircle;
                Vector3 velocity = new Vector3(rand.x * UnityEngine.Random.Range(0.1f, 5f), rand.y * UnityEngine.Random.Range(0.1f, 5f), UnityEngine.Random.Range(1f, 5f));
                Vector3 pos = head.transform.position;
                pos.z = 0.18f;
                Toolbox.Instance.SpawnDroplet(pos, blood, velocity);
            }
        }
        Die(message, damageType.physical);
        if (gameObject == GameManager.Instance.playerObject) {
            GameManager.Instance.IncrementStat(StatType.deathByExplodingHead, 1);
        }
    }
    public void SaveData(PersistentComponent data) {
        data.floats["health"] = health;
        data.floats["maxHealth"] = maxHealth;
        data.floats["impulse"] = impulse;
        data.floats["downed_timer"] = downedTimer;
        data.ints["hitstate"] = (int)hitState;
        data.floats["oxygen"] = oxygen;
        data.floats["hitstuncounter"] = hitStunCounter;
        data.bools["headExploded"] = headExploded;
    }
    public void LoadData(PersistentComponent data) {
        health = data.floats["health"];
        maxHealth = data.floats["maxHealth"];
        impulse = data.floats["impulse"];
        downedTimer = data.floats["downed_timer"];
        hitState = (Controllable.HitState)data.ints["hitstate"];
        oxygen = data.floats["oxygen"];
        hitStunCounter = data.floats["hitstuncounter"];
        headExploded = data.bools["headExploded"];
        if (headExploded) {
            Head head = GetComponentInChildren<Head>();
            if (head != null) {
                Destroy(head.gameObject);
            }
        }
    }
}
