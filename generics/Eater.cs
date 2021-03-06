﻿using UnityEngine;
using System.Collections.Generic;

public class Eater : Interactive, ISaveable {
    public bool strongStomach; // if true, does not get nauseated from disgusting / disturbing occurrences
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
    public LinkedList<GameObject> eatenQueue;
    public float vomitCountDown;
    public Dictionary<BuffType, Buff> netIntrinsics;
    bool starting = true;
    private void CheckNausea() {
        if (starting) {
            return;
        }
        if (nausea > 15 && nausea < 30 && lastStatement != nauseaStatement.warning) {
            lastStatement = nauseaStatement.warning;
            MessageSpeech message = new MessageSpeech("I don't feel so good!!", data: new EventData(chaos: 1, disturbing: 1, positive: -1));
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
        if (nausea > 60 && lastStatement != nauseaStatement.imminent) {
            lastStatement = nauseaStatement.imminent;
            MessageSpeech message = new MessageSpeech("I'm gonna puke!", data: new EventData(chaos: 2, disturbing: 1, positive: -2));
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
    }
    void Awake() {
        starting = true;
        eatenQueue = new LinkedList<GameObject>();
        Interaction eatAction = new Interaction(this, "Eat", "Eat");
        // eatAction.defaultPriority = 1;
        eatAction.dontWipeInterface = false;
        eatAction.otherOnSelfConsent = false;
        eatAction.holdingOnOtherConsent = false;
        eatAction.validationFunction = true;
        interactions.Add(eatAction);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsic);
        Toolbox.RegisterMessageCallback<MessageOccurrence>(this, HandleOccurrence);
    }
    void Start() {
        starting = false;
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
        foreach (EventData data in message.data.describable.GetChildren())
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
    void EnqueueEatenObject(GameObject eaten) {
        // add to first:        eaten -> {secondEaten, firstEaten}
        eatenQueue.AddFirst(eaten);

        eaten.SetActive(false);
        while (eatenQueue.Count > 2) {// {eaten, secondEaten, firstEaten}
            // remove first-in-first-out
            // {eaten, secondEaten} -> firstEaten
            GameObject oldEaten = eatenQueue.Last.Value;
            eatenQueue.RemoveLast();

            ClaimsManager.Instance.WasDestroyed(oldEaten);
            MySaver.RemoveObject(oldEaten);
            Destroy(oldEaten);
        }
    }
    public void Eat(Edible food) {
        nutrition += food.nutrition;
        MessageHead head = new MessageHead();
        head.type = MessageHead.HeadType.eating;
        head.value = true;
        head.crumbColor = food.pureeColor;
        Toolbox.Instance.SendMessage(gameObject, this, head);

        GameObject eaten = food.gameObject;
        LiquidContainer container = food.GetComponent<LiquidContainer>();
        LiquidResevoir reservoir = food.GetComponent<LiquidResevoir>();

        eaten.name = Toolbox.Instance.CloneRemover(eaten.name);

        //update our status based on our reaction to the food
        int reaction = CheckReaction(food);
        if (netIntrinsics[BuffType.clearHeaded].active()) {
            if (reaction > 0) {
                Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Good food.", data: new EventData(positive: 1)));
            }
            if (reaction < 0) {
                Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I did not enjoy that.", data: new EventData(positive: -1)));
            }
        } else {
            if (reaction > 0) {
                Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Yummy!", data: new EventData(positive: 1)));
            }
            if (reaction < 0) {
                nausea += 30;
                Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Yuck", data: new EventData(positive: -1)));
            }
            if (reaction == 0) {
                Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Refreshing!", data: new EventData(positive: 1)));
            }
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
            GameManager.Instance.CheckLiquidCollection(mliquid.liquid, gameObject);
            if (mliquid.liquid != null) {
                if (mliquid.liquid.name == "yogurt") {
                    GameManager.Instance.IncrementStat(StatType.yogurtEaten, 1);
                }
                if (mliquid.liquid.name == "yogurt") {
                    GameManager.Instance.IncrementStat(StatType.yogurtEaten, 1);
                }
            }
        }
        if (container) {
            if (container.amount > 0) {
                GameObject sip = Instantiate(Resources.Load("prefabs/droplet"), transform.position, Quaternion.identity) as GameObject;
                Liquid.MonoLiquidify(sip, container.liquid);
                Toolbox.Instance.AddLiveBuffs(gameObject, sip);
                Destroy(sip);
                // EnqueueEatenObject(sip);
            }
        }
        if (reservoir) {
            GameObject sip = Instantiate(Resources.Load("prefabs/droplet"), transform.position, Quaternion.identity) as GameObject;
            Liquid.MonoLiquidify(sip, reservoir.liquid);
            Toolbox.Instance.AddLiveBuffs(gameObject, sip);
            // EnqueueEatenObject(sip);
            // MySaver.disabledPersistents.Add(sip);
            Destroy(sip);
        }
        EnqueueEatenObject(eaten);
        eaten.SetActive(false);

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

        eaten.transform.position = transform.position;
        eaten.transform.SetParent(transform, false);
        eaten.transform.localPosition = Random.insideUnitCircle * (3f / 100);

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
            // pop from the stack: last-in-first-out
            // {eaten, secondEaten} 
            // eaten <- {secondEaten} 
            GameObject eaten = eatenQueue.First.Value;
            eatenQueue.RemoveFirst();

            // GameObject eaten = eatenQueue.Dequeue();
            // string eatenName = Toolbox.Instance.GetName(eaten);
            // data.strings[$"eaten{index}"] = eatenName;
            // Debug.Log($"{this} adding eaten to reference tree: {eatenName}");
            // MySaver.UpdateGameObjectReference(eaten, data, $"eaten{index}");
            // MySaver.AddToReferenceTree(gameObject, eaten);
            // index++;

            data.vomit = eaten.gameObject;
            eaten.SetActive(true);
            eaten.transform.position = transform.position;
            eaten.transform.SetParent(null);
            PhysicalBootstrapper phys = eaten.GetComponent<PhysicalBootstrapper>();
            if (phys) {
                phys.doInit = true;
                phys.InitPhysical(0.13f, Vector3.zero);
            }
            MonoLiquid mono = eaten.GetComponent<MonoLiquid>();
            if (mono) {
                Destroy(eaten);
                GameObject droplet = Toolbox.Instance.SpawnDroplet(mono.liquid, 0f, gameObject, 0.15f);
                // mono.liquid.vomit = true;
                droplet.GetComponent<MonoLiquid>().liquid.vomit = true;
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
            // eaten = null;
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
        vomitLiquid.atomicLiquids = new HashSet<Liquid>();
        vomitLiquid.atomicLiquids.Add(new Liquid(vomitLiquid));
        Toolbox.Instance.SpawnDroplet(vomitLiquid, 0f, gameObject, 0.05f);
        if (gameObject == GameManager.Instance.playerObject) {
            UINew.Instance.RefreshUI(active: true);
        }
    }

    void ReactToOccurrence(EventData od) {
        if (strongStomach)
            return;
        if (netIntrinsics != null && netIntrinsics[BuffType.undead].active())
            return;
        // do not react to the event if i am mindful
        if (netIntrinsics != null && netIntrinsics[BuffType.clearHeaded].active()) {
            return;
        }
        // Debug.Log(od.whatHappened);
        // foreach (KeyValuePair<Rating, float> kvp in od.ratings) {
        //     Debug.Log(kvp.Key.ToString() + ": " + kvp.Value.ToString());
        // }

        if (od.quality[Rating.disgusting] > 1)
            nausea += 10f;
        if (od.quality[Rating.disgusting] > 2)
            nausea += 10f;
    }
    public void SaveData(PersistentComponent data) {
        data.floats["nutrition"] = nutrition;
        data.floats["nausea"] = nausea;
        data.floats["vomitCountDown"] = vomitCountDown;
        data.ints["lastStatement"] = (int)lastStatement;
        int index = 0;
        if (data.GUIDs.ContainsKey("eaten1"))
            data.GUIDs.Remove("eaten1");
        if (data.GUIDs.ContainsKey("eaten0"))
            data.GUIDs.Remove("eaten0");

        LinkedList<GameObject> newEatenQueue = new LinkedList<GameObject>();
        while (eatenQueue.Count > 0 && index < 2) { // do NOT save anything more than two items!!! seriously!
                                                    // remove first-in-first-out
                                                    // GameObject eaten = eatenStack.Pop();
            GameObject eaten = eatenQueue.First.Value;
            eatenQueue.RemoveFirst();

            string eatenName = Toolbox.Instance.GetName(eaten);
            data.strings[$"eaten{index}"] = eatenName;
            // Debug.Log($"{this} adding eaten to reference tree: {eatenName}");
            // Debug.Log($"saving eaten {eaten}");
            MySaver.UpdateGameObjectReference(eaten, data, $"eaten{index}");
            MySaver.AddToReferenceTree(gameObject, eaten);
            index++;
            newEatenQueue.AddLast(eaten);
        }
        eatenQueue = newEatenQueue;
    }
    public void LoadData(PersistentComponent data) {
        eatenQueue = new LinkedList<GameObject>();
        nutrition = data.floats["nutrition"];
        nausea = data.floats["nausea"];
        vomitCountDown = data.floats["vomitCountDown"];
        lastStatement = (nauseaStatement)data.ints["lastStatement"];
        if (data.GUIDs.ContainsKey("eaten1")) {
            GameObject eaten = MySaver.IDToGameObject(data.GUIDs["eaten1"]);
            if (eaten != null) {
                EnqueueEatenObject(eaten);
                eaten.SetActive(false);
                eaten.transform.position = transform.position;
                eaten.transform.SetParent(transform, false);
                eaten.transform.localPosition = Random.insideUnitCircle * (3f / 100);
                Bones itemBones = eaten.GetComponent<Bones>();
                if (itemBones != null) {
                    if (itemBones.follower == null)
                        itemBones.Start();
                }
            } else {
                Debug.LogError($"{this} could not locate eaten1 object {data.strings["eaten1"]}. Possible lost saved object on the loose!");
            }
        }
        if (data.GUIDs.ContainsKey("eaten0")) {
            GameObject eaten = MySaver.IDToGameObject(data.GUIDs["eaten0"]);
            if (eaten != null) {
                EnqueueEatenObject(eaten);
                eaten.SetActive(false);
                eaten.transform.position = transform.position;
                eaten.transform.SetParent(transform, false);
                eaten.transform.localPosition = Random.insideUnitCircle * (3f / 100);
                Bones itemBones = eaten.GetComponent<Bones>();
                if (itemBones != null) {
                    if (itemBones.follower == null)
                        itemBones.Start();
                }
            } else {
                // Debug.Log("eaten0 is null");
                Debug.LogError($"{this} could not locate eaten0 object {data.strings["eaten0"]}. Possible lost saved object on the loose!");
            }

        }
    }
}
