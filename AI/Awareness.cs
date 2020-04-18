using System;
using System.Collections.Generic;
using AI;
using UnityEngine;
using DropOutStack;

[System.Serializable]
public class Knowledge {
    public GameObject obj;
    public Transform transform;
    public Vector3 lastSeenPosition;
    public float lastSeenTime;
    public Flammable flammable;
    public Knowledge() { }
    public Knowledge(GameObject o) {
        this.obj = o;
        transform = o.transform;
        lastSeenPosition = transform.position;
        lastSeenTime = Time.time;
        flammable = o.GetComponentInParent<Flammable>();
    }
    public void UpdateInfo() {
        if (obj) {
            lastSeenPosition = transform.position;
            lastSeenTime = Time.time;
        }
    }
}

[System.Serializable]
public class PersonalAssessment {
    public enum friendStatus { neutral, friend, enemy }
    public friendStatus status;
    public Knowledge knowledge;
    // public bool unconscious;
    public Controllable.HitState hitstate;
    public int numberOfTimesInsulted;
    public bool warned;
    public float timeWarned = -99f;
    public float timeLastSpokenTo;
    public PersonalAssessment(Knowledge k) {
        knowledge = k;
    }
    public PersonalAssessment() { }
}

public class Awareness : MonoBehaviour, ISaveable, IDirectable {
    public List<Ref<GameObject>> newPeopleList = new List<Ref<GameObject>>();
    public float socializationTimer;
    public static Dictionary<Rating, string> reactions = new Dictionary<Rating, string>(){
            {Rating.disgusting, "{grossreact}"},
            {Rating.disturbing, "{disturbreact}"},
            {Rating.chaos, "{chaosreact}"},
            {Rating.offensive, "{offensereact}"},
            {Rating.positive, "{positivereact}"}
        };
    public DecisionMaker decisionMaker;
    public List<GameObject> initialAwareness;
    public DropOutStack.DropOutStack<EventData> shortTermMemory = new DropOutStack.DropOutStack<EventData>(1000);
    public GameObject possession;
    public Collider2D protectZone;
    public Collider2D warnZone;
    public Knowledge possessionDefaultState;
    public GameObject wayWardPossession;
    private GameObject sightCone;
    private Flammable flammable;
    private HashSet<GameObject> seenFlags = new HashSet<GameObject>();
    Transform cachedTransform;
    FixedSizedQueue<string> lastNEvents = new FixedSizedQueue<string>();
    private Dictionary<BuffType, Buff> netBuffs = new Dictionary<BuffType, Buff>();
    public new Transform transform {
        get {
            if (cachedTransform == null) {
                cachedTransform = gameObject.GetComponent<Transform>();
            }
            return cachedTransform;
        }
    }
    public Vector2 direction;
    public Transform sightConeTransform;
    private Vector3 sightConeScale;
    // private Controllable control;
    private float speciousPresent;
    private const float perceptionInterval = 0.25f;
    private List<GameObject> fieldOfView = new List<GameObject>();
    private bool viewed;
    public Controllable.HitState hitState;
    public Ref<GameObject> nearestEnemy = new Ref<GameObject>(null);
    public Ref<GameObject> nearestFire = new Ref<GameObject>(null);
    public bool imOnFire;
    public SerializableDictionary<GameObject, Knowledge> knowledgebase = new SerializableDictionary<GameObject, Knowledge>();
    public SerializableDictionary<GameObject, PersonalAssessment> people = new SerializableDictionary<GameObject, PersonalAssessment>();
    void Start() {
        // Debug.Log(name + "awareness starting");
        // Debug.Log(initialAwareness);
        // Debug.Log(initialAwareness.Count);
        MessageDirectable message = new MessageDirectable();
        message.addDirectable.Add(this);
        Toolbox.Instance.SendMessage(gameObject, this, message);

        sightCone = Instantiate(Resources.Load("sightcone1"), gameObject.transform.position, Quaternion.identity) as GameObject;
        sightConeScale = sightCone.transform.localScale;
        sightConeTransform = sightCone.transform;
        sightConeTransform.parent = transform;
        if (initialAwareness != null) {
            if (initialAwareness.Count > 0) {
                // Debug.Log(initialAwareness[0]);
                fieldOfView = initialAwareness;
                Perceive();
            }
        }
    }
    void Awake() {
        lastNEvents.Limit = 25;
        Toolbox.RegisterMessageCallback<MessageInsult>(this, ProcessInsult);
        Toolbox.RegisterMessageCallback<MessageDamage>(this, AttackedByPerson);
        Toolbox.RegisterMessageCallback<MessageHitstun>(this, ProcessHitStun);
        Toolbox.RegisterMessageCallback<MessageThreaten>(this, ProcessThreat);
        Toolbox.RegisterMessageCallback<MessageInventoryChanged>(this, ProcessInventoryChanged);
        Toolbox.RegisterMessageCallback<MessageSpeech>(this, HandleSpeech);
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsics);
        Toolbox.RegisterMessageCallback<MessageOnCamera>(this, HandleOnCamera);
        GameManager.onRecordingChange += OnRecordingChange;
    }
    void OnRecordingChange(bool value) {
        if (this == null)
            return;
        if (!value) {
            MessageOnCamera message = new MessageOnCamera(value);
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
    }
    void HandleNetIntrinsics(MessageNetIntrinsic message) {
        netBuffs = message.netBuffs;
        if (message.netBuffs[BuffType.enraged].active()) {
            // all people become enemies
            foreach (KeyValuePair<GameObject, PersonalAssessment> kvp in people) {
                Hurtable otherHurtable = kvp.Key.GetComponent<Hurtable>();
                if (otherHurtable != null && !otherHurtable.monster)
                    kvp.Value.status = PersonalAssessment.friendStatus.enemy;
            }
        }
    }
    void HandleOnCamera(MessageOnCamera message) {

    }

    void HandleSpeech(MessageSpeech message) {
        foreach (GameObject party in message.involvedParties) {
            PersonalAssessment assessment = FormPersonalAssessment(party);
            if (assessment != null) {
                assessment.timeLastSpokenTo = Time.time;
            }
        }
    }
    void ProcessHitStun(MessageHitstun message) {
        hitState = message.hitState;
    }
    void ProcessThreat(MessageThreaten message) {
        if (hitState >= Controllable.HitState.unconscious)
            return;
        PersonalAssessment assessment = FormPersonalAssessment(message.messenger.gameObject);
        assessment.status = PersonalAssessment.friendStatus.enemy;
    }
    void ProcessInventoryChanged(MessageInventoryChanged message) {
        GameObject obj = message.holding;
        if (message.dropped != null)
            obj = message.dropped;
        if (obj != null) {
            Knowledge knowledge = null;
            if (knowledgebase.TryGetValue(obj, out knowledge)) {
                knowledge.UpdateInfo();
            } else {
                knowledge = new Knowledge(obj);
                knowledgebase.Add(obj, knowledge);
            }
        }
    }
    public List<GameObject> FindObjectWithName(string targetName) {
        List<GameObject> returnArray = new List<GameObject>();
        List<GameObject> removeArray = new List<GameObject>();
        foreach (Knowledge k in knowledgebase.Values) {
            if (k.obj) {
                if (k.obj.activeInHierarchy == false)
                    continue;
                if (k.obj.name == targetName)
                    returnArray.Add(k.obj);
            } else {
                removeArray.Add(k.obj);
            }
        }
        foreach (GameObject g in removeArray) {
            knowledgebase.Remove(g);
        }
        return returnArray;
    }
    void Update() {
        // update sight cone rotation and scale -- point it in the right direction.
        if (transform.localScale.x < 0 && sightConeTransform.localScale.x > 0) {
            Vector3 tempscale = sightConeScale;
            tempscale.x = -1 * sightConeScale.x;
            sightConeTransform.localScale = tempscale;
        }
        if (transform.localScale.x > 0 && sightConeTransform.localScale.x < 0) {
            sightConeTransform.localScale = sightConeScale;
        }
        float rot_z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        sightConeTransform.rotation = Quaternion.Euler(0f, 0f, rot_z);

        // work the timer for the discrete perception updates
        speciousPresent -= Time.deltaTime;
        if (speciousPresent <= 0) {
            if (fieldOfView.Count > 0 && viewed == true) {
                Perceive();
            }
            SetNearestEnemy();
            SetNearestFire();
            // TODO: check if i am on fire
            if (flammable == null) {
                flammable = GetComponent<Flammable>();
            }
            if (flammable != null) {
                imOnFire = flammable.onFire;
            }
        }

        if (socializationTimer <= 0) {
            if (decisionMaker.personality.social == Personality.Social.chatty) {
                socializationTimer += 2.5f * Time.deltaTime;
            }
            if (decisionMaker.personality.social == Personality.Social.normal) {
                socializationTimer += 0.5f * Time.deltaTime;
            }
        }
    }
    public void SetNearestEnemy() {
        nearestEnemy.val = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        List<GameObject> removeKeys = new List<GameObject>();
        foreach (PersonalAssessment assessment in people.Values) {
            if (assessment.knowledge.obj == null) {
                removeKeys.Add(assessment.knowledge.obj);
                continue;
            }
            if (assessment.status != PersonalAssessment.friendStatus.enemy)
                continue;
            if (assessment.hitstate == Controllable.HitState.unconscious && decisionMaker.personality.battleStyle != Personality.BattleStyle.bloodthirsty)
                continue;
            // Vector3 directionToTarget = assessment.knowledge.lastSeenPosition - currentPosition;
            Vector3 directionToTarget = assessment.knowledge.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr) {
                closestDistanceSqr = dSqrToTarget;
                nearestEnemy.val = assessment.knowledge.obj;
            }
        }
        foreach (GameObject key in removeKeys) {
            people.Remove(key);
        }
    }
    public void SetNearestFire() {
        nearestFire.val = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (Knowledge knowledge in knowledgebase.Values) {
            if (knowledge.flammable == null)
                continue;
            if (knowledge.flammable.onFire) {
                Vector3 directionToTarget = knowledge.lastSeenPosition - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr) {
                    closestDistanceSqr = dSqrToTarget;
                    nearestFire.val = knowledge.obj;
                }
            }
        }
    }
    // if its time to run the perception, add all triggerstay colliders to the list.
    // we don't know when this will be run or how many times, so i need a boolean to track
    // whether it has run this cycle yet or not.
    void OnTriggerStay2D(Collider2D other) {
        if (other.isTrigger)
            return;
        if (other.transform.IsChildOf(transform.root))
            return;
        // if it's background
        if (other.gameObject.layer == 8)
            return;
        if (speciousPresent <= 0) {
            if (viewed == false) {
                fieldOfView = new List<GameObject>();
                viewed = true;
            }
            if (fieldOfView.Contains(other.gameObject))
                return;
            // might be able to have better logic for how to add things to the field of view here. I need 
            // "high level" objects of import.
            // Debug.Log(other.transform.parent.gameObject);
            if (other.tag == "Physical")
                fieldOfView.Add(other.gameObject);
            Controllable baseControllable = other.GetComponentInParent<Controllable>();
            if (baseControllable != null) {
                fieldOfView.Add(baseControllable.gameObject);
            }
            Inventory otherInv = other.GetComponentInParent<Inventory>();
            if (otherInv != null)
                if (otherInv.holding)
                    fieldOfView.Add(otherInv.holding.gameObject);
            Head otherHead = other.GetComponentInChildren<Head>();
            if (otherHead != null)
                if (otherHead.hat != null)
                    fieldOfView.Add(otherHead.hat.gameObject);
        }

    }
    public string RecallMemory() {
        if (shortTermMemory.Count() == 0) {
            return "I am a blank slate.";
        }
        IEnumerator<EventData> enumerator = shortTermMemory.GetEnumerator();
        int i = 0;
        while (i <= UnityEngine.Random.Range(0, shortTermMemory.Count())) {
            i++;
            enumerator.MoveNext();
        }
        EventData memory = enumerator.Current;
        return "I remember when " + memory.whatHappened;
    }
    void ProcessOccurrenceFlag(GameObject flag) {
        if (netBuffs != null && netBuffs.ContainsKey(BuffType.enraged) && netBuffs[BuffType.enraged].active())
            return;
        Occurrence occurrence = flag.GetComponent<Occurrence>();
        if (occurrence == null)
            return;
        if (occurrence.data != null)
            WitnessOccurrence(occurrence.data);
    }
    void OnTriggerEnter2D(Collider2D other) {
        if (hitState >= Controllable.HitState.unconscious)
            return;
        if (seenFlags.Contains(other.gameObject))
            return;
        if (other.tag == "occurrenceFlag") {
            ProcessOccurrenceFlag(other.gameObject);
        }
        if (other.tag == "occurrenceSound") {
            Occurrence noiseOccurrence = other.GetComponent<Occurrence>();
            if (noiseOccurrence.involvedParties().Contains(gameObject)) {
                return;
            }
            MessageNoise message = new MessageNoise(other.gameObject);
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
        Qualities qualities = other.GetComponent<Qualities>();
        if (qualities) {
            foreach (EventData data in qualities.ToDescribable().GetChildren()) {
                ReactToEvent(data, new HashSet<GameObject>());

                OccurrenceEvent oD = new OccurrenceEvent(data);
                // oD.AddChild(data);
                // oD.additionEventData.Add(data);
                // oD.children.Add(data);
                MessageOccurrence message = new MessageOccurrence(oD);
                Toolbox.Instance.SendMessage(gameObject, this, message);
            }
        }
        if (other.name == "CameraRegion") {
            MessageOnCamera message = new MessageOnCamera(true);
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
        seenFlags.Add(other.gameObject);
    }
    void WitnessOccurrence(OccurrenceData od) {
        Toolbox.Instance.SendMessage(gameObject, this, new MessageOccurrence(od));
        if (od is OccurrenceViolence)
            WitnessViolence((OccurrenceViolence)od);
        foreach (EventData e in od.describable.GetChildren()) {
            ReactToEvent(e, od.involvedParties());
        }
    }
    void ReactToEvent(EventData dat, HashSet<GameObject> involvedParties) {
        // Debug.Log(involvedParties);
        // store memory
        EventData memory = new EventData(dat);
        shortTermMemory.Push(memory);

        // store reaction to memory?

        // react to specifics of event
        if (involvedParties.Contains(gameObject))
            return;

        int seenCount = 0;
        foreach (string noun in lastNEvents) {
            if (noun == dat.noun)
                seenCount += 1;
        }
        lastNEvents.Add(dat.noun);
        Rating[] ratings = (Rating[])Rating.GetValues(typeof(Rating));
        Toolbox.ShuffleArray<Rating>(ratings);
        foreach (Rating rating in ratings) {
            float threshhold = Toolbox.Gompertz(dat.quality[rating], 1.26f, -6.9f, 1);
            threshhold *= 1 - (float)seenCount / 5.0f;
            if (UnityEngine.Random.Range(0f, 1f) < threshhold && dat.quality[rating] > 0) {
                MessageSpeech message = new MessageSpeech(reactions[rating]);
                message.nimrod = true;
                message.involvedParties.Add(gameObject);
                message.involvedParties.UnionWith(involvedParties);
                Toolbox.Instance.SendMessage(gameObject, this, message);
                break;
            }
        }
    }
    public void ReactToPerson(GameObject target) {
        PersonalAssessment assessment = FormPersonalAssessment(target);
        if (assessment == null)
            return;
        List<Ref<GameObject>> removeThese = new List<Ref<GameObject>>();
        foreach (Ref<GameObject> np in newPeopleList) {
            if (np.val == target) {
                removeThese.Add(np);
            }
        }
        foreach (Ref<GameObject> np in removeThese) {
            newPeopleList.Remove(np);
        }
        string phrase = "";
        switch (assessment.status) {
            case PersonalAssessment.friendStatus.enemy:
                phrase = "{greet-enemy}";

                break;
            case PersonalAssessment.friendStatus.friend:
                phrase = "{greet-friend}";
                break;
            default:
            case PersonalAssessment.friendStatus.neutral:
                phrase = "{greet-neutral}";
                break;
        }
        MessageSpeech message = new MessageSpeech(phrase);
        message.nimrod = true;
        message.involvedParties.Add(target);
        message.involvedParties.Add(gameObject);
        Toolbox.Instance.SendMessage(gameObject, this, message);
        MessageNoise noise = new MessageNoise(gameObject);
        Toolbox.Instance.SendMessage(target, this, noise);
    }
    void WitnessViolence(OccurrenceViolence dat) {
        if (dat.victim == null || dat.attacker == null)
            return;
        GameObject victimObject = Controller.Instance.GetBaseInteractive(dat.victim.transform);
        GameObject attackerObject = Controller.Instance.GetBaseInteractive(dat.attacker.transform);
        PersonalAssessment attacker = FormPersonalAssessment(attackerObject);
        PersonalAssessment victim = FormPersonalAssessment(victimObject);
        if (possession != null && possession == victimObject) {
            Debug.Log(dat.attacker.name + " attacked my possession!");
            if (attacker != null) {
                attacker.status = PersonalAssessment.friendStatus.enemy;
            }
        }
        if (dat.victim == gameObject) {
            // A. Getting hit
            // B. Getting hit, but not enough to hurt
            // MessageSpeech message = new MessageSpeech("How dare you!");
            // Debug.Log(dat.amount);
            // Toolbox.Instance.SendMessage(gameObject, this, message);
        } else if (dat.attacker != gameObject) {
            // F. Witnessing violence to other
            MessageSpeech message = new MessageSpeech("Whoah! Yikes!");
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
        if (attacker == null || victim == null) {
            return;
        }
        if (gameObject == attacker.knowledge.obj)
            return;
        switch (attacker.status) {
            case PersonalAssessment.friendStatus.friend:
                victim.status = PersonalAssessment.friendStatus.enemy;
                // Debug.Log(name + " friend attacked friend");
                break;
            case PersonalAssessment.friendStatus.neutral:
                if (victim.status == PersonalAssessment.friendStatus.friend) {
                    attacker.status = PersonalAssessment.friendStatus.enemy;
                    // Debug.Log(name + " neutral attacked friend");
                }
                if (victim.status == PersonalAssessment.friendStatus.neutral) {
                    attacker.status = PersonalAssessment.friendStatus.enemy;
                    // Debug.Log(name + " neutral attacked neutral");
                }
                if (victim.status == PersonalAssessment.friendStatus.enemy) {
                    attacker.status = PersonalAssessment.friendStatus.friend;
                    // Debug.Log(name + " neutral attacked enemy");
                }
                break;
            default:
                break;
        }
    }
    // process the list of objects in the field of view.
    void Perceive() {
        viewed = false;
        speciousPresent = perceptionInterval;
        foreach (GameObject g in fieldOfView) {
            if (g == null)
                continue;
            Knowledge knowledge = null;
            if (g == possession) {
                if (!knowledgebase.ContainsKey(g)) {
                    possessionDefaultState = new Knowledge(g);
                    knowledge = new Knowledge(g);
                    knowledgebase.Add(g, knowledge);
                }
            }
            if (knowledgebase.TryGetValue(g, out knowledge)) {
                knowledge.UpdateInfo();
            } else {
                knowledge = new Knowledge(g);
                knowledgebase.Add(g, knowledge);
            }
            PersonalAssessment assessment = FormPersonalAssessment(g);
            Humanoid human = g.GetComponentInParent<Humanoid>();
            if (human) {
                assessment.hitstate = human.hitState;
                if (protectZone != null) {
                    if (warnZone.bounds.Contains(knowledge.transform.position) &&
                        GameManager.Instance.sceneTime - assessment.timeWarned > 7f &&
                        assessment.status != PersonalAssessment.friendStatus.enemy) {
                        assessment.timeWarned = GameManager.Instance.sceneTime;
                        string phrase = "";
                        if (!assessment.warned) {
                            phrase = "Halt! Come no closer!";
                        } else {
                            phrase = "I am sworn to protect this bridge";
                        }
                        MessageSpeech message = new MessageSpeech(phrase);
                        Toolbox.Instance.SendMessage(gameObject, this, message);
                        assessment.warned = true;
                    }
                    if (protectZone.bounds.Contains(knowledge.transform.position)) {
                        assessment.status = PersonalAssessment.friendStatus.enemy;
                        foreach (Priority priority in decisionMaker.priorities) {
                            priority.ReceiveMessage(new MessageThreaten());
                        }
                    }
                }
            }
        }
    }
    public bool PossessionsAreOkay() {
        if (possession == null) {
            return true;
        }
        Knowledge knowledge = null;
        if (!knowledgebase.TryGetValue(possession, out knowledge)) {
            return false;
        }
        if (possessionDefaultState == null) {
            return false;
        }
        if (Time.time - knowledge.lastSeenTime > 0.5f) {
            return false;
        }
        if (Vector2.Distance(knowledge.lastSeenPosition, possessionDefaultState.lastSeenPosition) > 0.1) {
            return false;
        } else {
            return true;
        }
    }
    public PersonalAssessment FormPersonalAssessment(GameObject g, bool debug = false) {
        if (g == null)
            return null;
        if (g == gameObject)
            return null;
        if (debug)
            Debug.Log("assess " + g.name + ":");
        Controllable rootControllable = g.GetComponentInParent<Controllable>();
        if (rootControllable == null) {
            if (debug)
                Debug.Log("no root controllable. quitting...");
            return null;
        }
        GameObject rootObject = rootControllable.gameObject;
        if (debug)
            Debug.Log("root object: " + rootObject.name);
        if (!knowledgebase.ContainsKey(rootObject))
            knowledgebase.Add(rootObject, new Knowledge(rootObject));
        PersonalAssessment storedAssessment;
        if (people.TryGetValue(rootObject, out storedAssessment)) {
            float interval = Time.time - knowledgebase[rootObject].lastSeenTime;
            if (interval > 10f) {
                AddSocializationTarget(rootObject);
            }
            interval = Time.time - storedAssessment.timeLastSpokenTo;
            if ((interval > 10f && decisionMaker.personality.social == Personality.Social.chatty) || (interval > 25f && decisionMaker.personality.social == Personality.Social.normal)) {
                AddSocializationTarget(rootObject);
            }
            if (netBuffs != null && netBuffs.ContainsKey(BuffType.enraged) && netBuffs[BuffType.enraged].active()) {
                Hurtable otherHurtable = rootControllable.GetComponent<Hurtable>();
                if (otherHurtable != null && !otherHurtable.monster)
                    storedAssessment.status = PersonalAssessment.friendStatus.enemy;
            }
            return storedAssessment;
        }
        PersonalAssessment assessment = new PersonalAssessment(knowledgebase[rootObject]);
        if (netBuffs != null && netBuffs.ContainsKey(BuffType.enraged) && netBuffs[BuffType.enraged].active()) {
            Hurtable otherHurtable = rootControllable.GetComponent<Hurtable>();
            if (otherHurtable != null && !otherHurtable.monster)
                assessment.status = PersonalAssessment.friendStatus.enemy;
        }
        AddSocializationTarget(rootObject);
        people.Add(rootObject, assessment);
        return assessment;
    }
    void AddSocializationTarget(GameObject target) {
        foreach (Ref<GameObject> np in newPeopleList) {
            if (np.val == target)
                return;
        }
        Ref<GameObject> newPerson = new Ref<GameObject>(target);
        // newPerson.countDownTimer = 10f;
        newPeopleList.Add(newPerson);
    }
    void AttackedByPerson(MessageDamage message) {
        if (hitState >= Controllable.HitState.unconscious)
            return;
        if (message.impersonal)
            return;
        if (netBuffs != null) {
            if (netBuffs.ContainsKey(BuffType.enraged) && netBuffs[BuffType.enraged].active())
                return;
            if (netBuffs.ContainsKey(BuffType.invulnerable) && netBuffs[BuffType.invulnerable].active() && message.type == damageType.fire)
                return;
        }
        // adjust reaction depending on magnitude
        if (message.amount <= 15) {
            // minor nusiance
            MessageSpeech speech = new MessageSpeech("{nuisance}");
            speech.nimrod = true;
            Toolbox.Instance.SendMessage(gameObject, this, speech);
            return;
        }

        GameObject g = message.responsibleParty;
        PersonalAssessment assessment = FormPersonalAssessment(g);
        if (assessment != null) {
            if (assessment.status != PersonalAssessment.friendStatus.friend) {
                if (assessment.status != PersonalAssessment.friendStatus.enemy) {
                    MessageSpeech speech = new MessageSpeech("{newenemy}");
                    speech.nimrod = true;
                    Toolbox.Instance.SendMessage(gameObject, this, speech);
                }
                assessment.status = PersonalAssessment.friendStatus.enemy;
            }
            assessment.knowledge.lastSeenPosition = g.transform.position;
        }
    }

    public void ProcessInsult(MessageInsult message) {
        if (hitState >= Controllable.HitState.unconscious)
            return;
        // TODO: make the reaction to insults more sophisticated
        PersonalAssessment assessment = FormPersonalAssessment(message.messenger.gameObject);
        Speech mySpeech = GetComponent<Speech>();
        assessment.numberOfTimesInsulted += 1;
        // TODO: make the insult trigger personality-dependent
        if (assessment.numberOfTimesInsulted >= 2) {
            assessment.status = PersonalAssessment.friendStatus.enemy;
        }
        // process hurt feelings
        if (mySpeech != null) {
            switch (assessment.status) {
                case PersonalAssessment.friendStatus.friend:
                    break;
                case PersonalAssessment.friendStatus.neutral:
                    break;
                case PersonalAssessment.friendStatus.enemy:
                    break;
            }
        }
    }
    public void SaveData(PersistentComponent data) {
        // TODO: don't overrwrite existing saved knowledges
        // TODO: check that it is working as intended, index knowledge and P.A. with id numbers
        data.ints["hitstate"] = (int)hitState;
        data.knowledgeBase = new List<SerializedKnowledge>();
        data.people = new List<SerializedPersonalAssessment>();
        if (possession != null) {
            MySaver.UpdateGameObjectReference(possession, data, "possession", overWriteWithNull: false);
            // MySaver.AddToReferenceTree(data.id, possession);
        }
        foreach (KeyValuePair<GameObject, Knowledge> keyVal in knowledgebase) {
            SerializedKnowledge knowledge = SaveKnowledge(keyVal.Value);
            if (knowledge.gameObjectID == System.Guid.Empty)
                continue;
            data.knowledgeBase.Add(knowledge);
        }
        if (possessionDefaultState != null) {
            SerializedKnowledge knowledge = SaveKnowledge(possessionDefaultState);
            if (knowledge.gameObjectID != System.Guid.Empty)
                data.knowledges["defaultState"] = knowledge;
        }
        foreach (KeyValuePair<GameObject, PersonalAssessment> keyVal in people) {
            data.people.Add(SavePerson(keyVal.Value));
        }
    }
    public void LoadData(PersistentComponent data) {
        if (data.GUIDs.ContainsKey("possession")) {
            possession = MySaver.IDToGameObject(data.GUIDs["possession"]);
        }
        hitState = (Controllable.HitState)data.ints["hitstate"];
        foreach (SerializedKnowledge knowledge in data.knowledgeBase) {
            Knowledge newKnowledge = LoadKnowledge(knowledge);
            if (newKnowledge.obj != null) {
                knowledgebase[newKnowledge.obj] = newKnowledge;
            }
        }
        foreach (SerializedPersonalAssessment pa in data.people) {
            GameObject go = MySaver.IDToGameObject(pa.gameObjectID);
            if (go != null) {
                PersonalAssessment assessment = LoadPerson(pa);
                assessment.knowledge = knowledgebase[go];
                people[go] = assessment;
            }
        }
        if (data.knowledges.ContainsKey("defaultState")) {
            possessionDefaultState = LoadKnowledge(data.knowledges["defaultState"]);
        }
        SetNearestEnemy();
        SetNearestFire();
    }
    SerializedKnowledge SaveKnowledge(Knowledge input) {
        SerializedKnowledge data = new SerializedKnowledge();
        data.lastSeenPosition = input.lastSeenPosition;
        data.lastSeenTime = input.lastSeenTime;
        data.gameObjectID = System.Guid.Empty;
        if (input.obj != null) {
            MySaver.savedObjects.TryGetValue(input.obj, out data.gameObjectID);
        }
        return data;
    }
    Knowledge LoadKnowledge(SerializedKnowledge input) {
        Knowledge knowledge = new Knowledge();
        knowledge.lastSeenPosition = input.lastSeenPosition;
        knowledge.lastSeenTime = input.lastSeenTime;
        knowledge.obj = MySaver.IDToGameObject(input.gameObjectID);
        if (knowledge.obj != null) {
            knowledge.transform = knowledge.obj.transform;
            knowledge.flammable = knowledge.obj.GetComponent<Flammable>();
        }
        return knowledge;
    }
    SerializedPersonalAssessment SavePerson(PersonalAssessment input) {
        SerializedPersonalAssessment data = new SerializedPersonalAssessment();
        data.status = input.status;
        data.HitState = input.hitstate;
        if (!MySaver.savedObjects.TryGetValue(input.knowledge.obj, out data.gameObjectID)) {
            data.gameObjectID = System.Guid.Empty;
        }
        return data;
    }
    PersonalAssessment LoadPerson(SerializedPersonalAssessment input) {
        PersonalAssessment assessment = new PersonalAssessment();
        assessment.status = input.status;
        assessment.hitstate = input.HitState;
        return assessment;
    }
    public void DirectionChange(Vector2 newDirection) {
        direction = newDirection;
    }
}