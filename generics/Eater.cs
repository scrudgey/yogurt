using UnityEngine;
using System.Collections.Generic;

public class Eater : Interactive, ISaveable {
    public float nutrition;
    public enum preference { neutral, likes, dislikes }
    enum nauseaStatement { none, warning, imminent }
    nauseaStatement lastStatement;
    public preference vegetablePreference;
    public preference meatPreference;
    public preference immoralPreference;
    public preference offalPreference;
    private AudioSource audioSource;
    private float _nausea;
    public float nausea {
        get {
            return _nausea;
        }
        set {
            _nausea = value;
            CheckNausea();
        }
    }
    private bool poisonNausea;
    private Queue<GameObject> eatenQueue;
    public float vomitCountDown;
    public Dictionary<BuffType, Buff> netIntrinsics;
    private void CheckNausea() {
        //TODO: this is spawning lots of flags
        if (nausea > 15 && nausea < 30 && lastStatement != nauseaStatement.warning) {
            lastStatement = nauseaStatement.warning;
            MessageSpeech message = new MessageSpeech("I don't feel so good!!", eventData: new EventData(chaos: 1, disturbing: 1, positive: -1));
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
        if (nausea > 60 && lastStatement != nauseaStatement.imminent) {
            lastStatement = nauseaStatement.imminent;
            MessageSpeech message = new MessageSpeech("I'm gonna puke!", eventData: new EventData(chaos: 2, disturbing: 1, positive: -2));
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
    }
    void Awake() {
        eatenQueue = new Queue<GameObject>();
        Interaction eatAction = new Interaction(this, "Eat", "Eat");
        eatAction.defaultPriority = 1;
        eatAction.dontWipeInterface = false;
        eatAction.otherOnPlayerConsent = false;
        eatAction.validationFunction = true;
        interactions.Add(eatAction);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsic);
        Toolbox.RegisterMessageCallback<MessageOccurrence>(this, HandleOccurrence);
    }
    public void HandleNetIntrinsic(MessageNetIntrinsic message) {
        if (message.netBuffs[BuffType.poison].boolValue) {
            poisonNausea = true;
        } else {
            poisonNausea = false;
        }
        if (message.netBuffs[BuffType.undead].boolValue) {
            vegetablePreference = preference.likes;
            meatPreference = preference.likes;
            offalPreference = preference.likes;
            immoralPreference = preference.likes;
            nausea = 0;
        }
        netIntrinsics = message.netBuffs;
    }
    public void HandleOccurrence(MessageOccurrence message) {
        foreach (EventData data in message.data.events)
            ReactToOccurrence(data);
    }
    void Update() {
        if (poisonNausea) {
            nausea += Time.deltaTime * 30f;
        }
        if (nausea > 50) {
            // Vomit();
            nausea += Time.deltaTime * 10f;
        }
        if (nausea > 100 && Random.Range(0f, 1f) < 0.01f) {
            Vomit();
            // nausea += Time.deltaTime * 10f;
        }
        if (nutrition > 100) {
            nausea += Time.deltaTime * 20f * Mathf.Min(2f, nutrition / 200);
        }
        if (vomitCountDown > 0) {
            vomitCountDown -= Time.deltaTime;
            if (vomitCountDown <= 0) {
                MessageHead head = new MessageHead();
                head.type = MessageHead.HeadType.vomiting;
                head.value = false;
                Toolbox.Instance.SendMessage(gameObject, this, head);
            }
        }
    }
    public int CheckReaction(Edible food) {
        // TODO: better code here
        int reaction = 0;
        bool[] types = new bool[] { food.vegetable, food.meat, food.immoral, food.offal };
        preference[] prefs = new preference[] { vegetablePreference, meatPreference, immoralPreference, offalPreference };
        for (int i = 0; i < prefs.Length; i++) {
            if (types[i]) {
                switch (prefs[i]) {
                    case preference.dislikes:
                        reaction -= 3;
                        break;
                    case preference.likes:
                        reaction++;
                        break;
                    default:
                        break;
                }
            }
        }
        return reaction;
    }
    public string Eat_desc(Edible food) {
        string foodname = Toolbox.Instance.GetName(food.gameObject);
        return "Eat " + foodname;
    }
    public void Eat(Edible food) {
        nutrition += food.nutrition;
        MessageHead head = new MessageHead();
        head.type = MessageHead.HeadType.eating;
        head.value = true;
        head.crumbColor = food.pureeColor;
        Toolbox.Instance.SendMessage(gameObject, this, head);
        // randomly store a clone of the object for later vomiting
        // GameObject eaten = Instantiate(food.gameObject) as GameObject;
        GameObject eaten = food.gameObject;
        eaten.name = Toolbox.Instance.CloneRemover(eaten.name);
        eatenQueue.Enqueue(eaten);
        eaten.SetActive(false);
        if (eatenQueue.Count > 2) {
            GameObject oldEaten = eatenQueue.Dequeue();
            ClaimsManager.Instance.WasDestroyed(oldEaten);
            Destroy(oldEaten);
        }
        //update our status based on our reaction to the food
        int reaction = CheckReaction(food);
        if (reaction > 0) {
            Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Yummy!", eventData: new EventData(positive: 1)));
        }
        if (reaction < 0) {
            nausea += 30;
            Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Yuck", eventData: new EventData(positive: -1)));
        }
        if (nutrition > 50) {
            Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I'm full!"));
        }
        if (nutrition > 75) {
            Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I can't eat another bite!"));
        }
        Toolbox.Instance.AddLiveBuffs(gameObject, food.gameObject);
        // set up an occurrence flag for this eating!
        OccurrenceEat eatData = new OccurrenceEat();
        eatData.eater = gameObject;
        eatData.edible = food;
        MonoLiquid mliquid = food.GetComponent<MonoLiquid>();
        if (mliquid) {
            eatData.liquid = mliquid.liquid;
            if (mliquid.liquid != null) {
                if (mliquid.liquid.name == "yogurt") {
                    GameManager.Instance.IncrementStat(StatType.yogurtEaten, 1);
                }
            }
        }
        LiquidContainer container = food.GetComponent<LiquidContainer>();
        if (container) {
            if (container.amount > 0) {
                GameObject sip = Instantiate(Resources.Load("prefabs/droplet"), transform.position, Quaternion.identity) as GameObject;
                Liquid.MonoLiquidify(sip, container.liquid);
                Toolbox.Instance.AddLiveBuffs(gameObject, sip);
                eatenQueue.Enqueue(sip);
                sip.SetActive(false);
            }
        }
        LiquidResevoir reservoir = food.GetComponent<LiquidResevoir>();
        if (reservoir) {
            // GameObject sip = new GameObject();
            GameObject sip = Instantiate(Resources.Load("prefabs/droplet"), transform.position, Quaternion.identity) as GameObject;
            Liquid.MonoLiquidify(sip, reservoir.liquid);
            Toolbox.Instance.AddLiveBuffs(gameObject, sip);
            // Destroy(sip);
            eatenQueue.Enqueue(sip);
            sip.SetActive(false);
            MySaver.disabledPersistents.Add(sip);
        }
        if (Toolbox.Instance.CloneRemover(food.name) == "sword") {
            GameManager.Instance.IncrementStat(StatType.swordsEaten, 1);
        }
        if (Toolbox.Instance.CloneRemover(food.name) == "heart") {
            GameManager.Instance.IncrementStat(StatType.heartsEaten, 1);
        }
        if (food.GetComponent<Hat>() != null) {
            GameManager.Instance.IncrementStat(StatType.hatsEaten, 1);
        }
        if (food.human) {
            GameManager.Instance.IncrementStat(StatType.actsOfCannibalism, 1);
        }
        Toolbox.Instance.OccurenceFlag(gameObject, eatData);
        // play eat sound
        if (food.eatSound != null) {
            Toolbox.Instance.AudioSpeaker(food.eatSound, transform.position);
        }
        GameManager.Instance.CheckItemCollection(food.gameObject, gameObject);
        food.BeEaten();
    }
    public bool Eat_Validation(Edible food) {
        if (GameManager.Instance.data == null)
            return false;
        if (food.inedible) {
            if (GameManager.Instance.data.perks["eat_all"]) {
                return true;
            } else {
                return false;
            }
        }
        return true;
    }
    public void Vomit() {
        vomitCountDown = 1.5f;

        GameManager.Instance.IncrementStat(StatType.vomit, 1);

        nausea = 0;
        nutrition = 0;

        OccurrenceVomit data = new OccurrenceVomit();
        data.vomiter = gameObject;
        if (eatenQueue.Count > 0) {
            GameObject eaten = eatenQueue.Dequeue();
            data.vomit = eaten.gameObject;
            eaten.SetActive(true);
            eaten.transform.position = transform.position;
            PhysicalBootstrapper phys = eaten.GetComponent<PhysicalBootstrapper>();
            if (phys) {
                phys.doInit = true;
                phys.InitPhysical(0.13f, Vector3.zero);
            }
            MonoLiquid mono = eaten.GetComponent<MonoLiquid>();
            if (mono) {
                GameObject droplet = Toolbox.Instance.SpawnDroplet(mono.liquid, 0f, gameObject, 0.15f);
                mono.liquid.vomit = true;
                mono.edible.vomit = true;
                if (mono.liquid.name == "yogurt") {
                    GameManager.Instance.IncrementStat(StatType.yogurtVomit, 1);
                }
                CircleCollider2D dropCollider = droplet.GetComponent<CircleCollider2D>();
                foreach (Collider2D collider in GetComponentsInChildren<Collider2D>()) {
                    Physics2D.IgnoreCollision(dropCollider, collider, true);
                }
            }
            Edible edible = eaten.GetComponent<Edible>();
            if (edible) {
                edible.vomit = true;
            }
            eaten = null;
        }
        Toolbox.Instance.OccurenceFlag(gameObject, data);
        MessageHead head = new MessageHead();
        head.type = MessageHead.HeadType.vomiting;
        head.value = true;
        Toolbox.Instance.SendMessage(gameObject, this, head);
        Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Blaaaaargh!"));
        if (audioSource) {
            audioSource.PlayOneShot(Resources.Load("sounds/vomit", typeof(AudioClip)) as AudioClip);
        }
        Liquid vomitLiquid = Liquid.LoadLiquid("vomit");
        vomitLiquid.vomit = true;
        Toolbox.Instance.SpawnDroplet(vomitLiquid, 0f, gameObject, 0.05f);
    }

    void ReactToOccurrence(EventData od) {
        if (netIntrinsics != null && netIntrinsics[BuffType.undead].boolValue)
            return;
        // Debug.Log(od.whatHappened);
        // foreach (KeyValuePair<Rating, float> kvp in od.ratings) {
        //     Debug.Log(kvp.Key.ToString() + ": " + kvp.Value.ToString());
        // }
        if (od.ratings[Rating.disgusting] > 1)
            nausea += 10f;
        if (od.ratings[Rating.disgusting] > 2)
            nausea += 10f;
    }
    public void SaveData(PersistentComponent data) {
        data.floats["nutrition"] = nutrition;
        data.floats["nausea"] = nausea;
        data.floats["vomitCountDown"] = vomitCountDown;
        int index = 0;
        while (eatenQueue.Count > 0) {
            GameObject eaten = eatenQueue.Dequeue();
            MySaver.UpdateGameObjectReference(eaten, data, "eaten" + index.ToString());
            MySaver.AddToReferenceTree(gameObject, eaten);
            index++;
        }
    }
    public void LoadData(PersistentComponent data) {
        nutrition = data.floats["nutrition"];
        nausea = data.floats["nausea"];
        vomitCountDown = data.floats["vomitCountDown"];
        if (data.ints.ContainsKey("eaten1")) {
            GameObject eaten = MySaver.IDToGameObject(data.ints["eaten1"]);
            if (eaten != null) {
                eatenQueue.Enqueue(eaten);
                eaten.SetActive(false);
            } else {
                // Debug.Log("eaten1 is null");
            }
        }
        if (data.ints.ContainsKey("eaten0")) {
            GameObject eaten = MySaver.IDToGameObject(data.ints["eaten0"]);
            if (eaten != null) {
                eatenQueue.Enqueue(eaten);
                eaten.SetActive(false);
            } else {
                // Debug.Log("eaten0 is null");
            }

        }
    }
}
