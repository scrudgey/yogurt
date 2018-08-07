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
        foreach (Component component in o.GetComponents<Component>()) {
            if (component is Flammable) {
                flammable = (Flammable)component;
            }
        }
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
    public bool unconscious;
    public int numberOfTimesInsulted;
    public bool warned;
    public float timeWarned = -99f;
    public PersonalAssessment(Knowledge k) {
        knowledge = k;
    }
    public PersonalAssessment() { }
}

public class Awareness : MonoBehaviour, ISaveable {
    public DecisionMaker decisionMaker;
    public List<GameObject> initialAwareness;
    public DropOutStack.DropOutStack<EventData> shortTermMemory = new DropOutStack.DropOutStack<EventData>(1000);
    public GameObject possession;
    public Collider2D protectZone;
    public Collider2D warnZone;
    public Knowledge possessionDefaultState;
    public GameObject wayWardPossession;
    private GameObject sightCone;
    private List<GameObject> seenFlags = new List<GameObject>();
    Transform cachedTransform;
    public new Transform transform {
        get {
            if (cachedTransform == null) {
                cachedTransform = gameObject.GetComponent<Transform>();
            }
            return cachedTransform;
        }
    }
    public Transform sightConeTransform;
    private Vector3 sightConeScale;
    private Controllable control;
    private float speciousPresent;
    private const float perceptionInterval = 0.25f;
    private List<GameObject> fieldOfView = new List<GameObject>();
    private bool viewed;
    public Controllable.HitState hitState;
    public Ref<GameObject> nearestEnemy = new Ref<GameObject>(null);
    public Ref<GameObject> nearestFire = new Ref<GameObject>(null);
    public SerializableDictionary<GameObject, Knowledge> knowledgebase = new SerializableDictionary<GameObject, Knowledge>();
    public SerializableDictionary<GameObject, PersonalAssessment> people = new SerializableDictionary<GameObject, PersonalAssessment>();
    void Start() {
        control = gameObject.GetComponent<Controllable>();
        sightCone = Instantiate(Resources.Load("sightcone1"), gameObject.transform.position, Quaternion.identity) as GameObject;
        sightConeScale = sightCone.transform.localScale;
        sightConeTransform = sightCone.transform;
        sightConeTransform.parent = transform;
        if (initialAwareness != null) {
            if (initialAwareness.Count > 0) {
                fieldOfView = initialAwareness;
                Perceive();
            }
        }
    }
    void Awake(){
        Toolbox.RegisterMessageCallback<MessageInsult>(this, ProcessInsult);
        Toolbox.RegisterMessageCallback<MessageDamage>(this, AttackedByPerson);
        Toolbox.RegisterMessageCallback<MessageHitstun>(this, ProcessHitStun);
        Toolbox.RegisterMessageCallback<MessageThreaten>(this, ProcessThreat);
        Toolbox.RegisterMessageCallback<MessageInventoryChanged>(this, ProcessInventoryChanged);
    }
    void ProcessHitStun(MessageHitstun message){
        hitState = message.hitState;
    }
    void ProcessThreat(MessageThreaten message){
        if (hitState >= Controllable.HitState.unconscious)
            return;
        PersonalAssessment assessment = FormPersonalAssessment(message.messenger.gameObject);
        assessment.status = PersonalAssessment.friendStatus.enemy;
    }
    void ProcessInventoryChanged(MessageInventoryChanged message) {
        if (message.holding != null) {
            Knowledge knowledge = null;
            if (knowledgebase.TryGetValue(message.holding, out knowledge)) {
                knowledge.UpdateInfo();
            } else {
                knowledge = new Knowledge(message.holding);
                knowledgebase.Add(message.holding, knowledge);
            }
        }
        if (message.dropped != null) {
            Knowledge knowledge = null;
            if (knowledgebase.TryGetValue(message.dropped, out knowledge)) {
                knowledge.UpdateInfo();
            } else {
                knowledge = new Knowledge(message.dropped);
                knowledgebase.Add(message.dropped, knowledge);
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
        float rot_z = Mathf.Atan2(control.direction.y, control.direction.x) * Mathf.Rad2Deg;
        sightConeTransform.rotation = Quaternion.Euler(0f, 0f, rot_z);
        
        // work the timer for the discrete perception updates
        speciousPresent -= Time.deltaTime;
        if (speciousPresent <= 0) {
            if (fieldOfView.Count > 0 && viewed == true) {
                Perceive();
            }
            SetNearestEnemy();
            SetNearestFire();
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
            if (assessment.unconscious && decisionMaker.personality.battleStyle != Personality.BattleStyle.bloodthirsty)
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
    void OnTriggerEnter2D(Collider2D other) {
        if (hitState >= Controllable.HitState.unconscious)
            return;
        if (seenFlags.Contains(other.gameObject))
            return;
        if (other.tag == "occurrenceFlag") {
            ProcessOccurrence(other.gameObject);
            seenFlags.Add(other.gameObject);
        }
        if (other.tag == "occurrenceSound"){
            // do something
            Occurrence flag = other.GetComponent<Occurrence>();
            if (!flag.involvedParties.Contains(gameObject)){
                // react to noise
                ProcessNoise(other.gameObject);
            }
        }
    }
    void ProcessNoise(GameObject flag){
        MessageNoise message = new MessageNoise(flag);
        Toolbox.Instance.SendMessage(gameObject, this, message);
    }
    void ProcessOccurrence(GameObject flag) {
        Occurrence occurrence = flag.GetComponent<Occurrence>();
        if (occurrence == null)
            return;
        foreach (OccurrenceData od in occurrence.data) {
            Toolbox.Instance.SendMessage(gameObject, this, new MessageOccurrence(od));
            if (od is OccurrenceViolence) 
                WitnessViolence((OccurrenceViolence)od);
            foreach (EventData e in od.events) {
                WitnessEvent(e);
            }
        }
    }
    void WitnessEvent(EventData dat){
        EventData memory = new EventData(dat);
        shortTermMemory.Push(memory);
    }
    public string RecallMemory(){
        IEnumerator<EventData> enumerator = shortTermMemory.GetEnumerator();
        int i = 0;
        while(i < Random.Range(0, shortTermMemory.Count())){
            i++;
            enumerator.MoveNext();
        }
        EventData memory = enumerator.Current;
        return "I remember when "+memory.whatHappened;
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
        if (attacker == null || victim == null) {
            return;
        }
        if (gameObject == attacker.knowledge.obj || gameObject == victim.knowledge.obj)
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
                assessment.unconscious = human.hitState >= Controllable.HitState.stun;
                if (protectZone != null) {
                    if (warnZone.bounds.Contains(knowledge.transform.position) &&
                        GameManager.Instance.sceneTime - assessment.timeWarned > 4f &&
                        assessment.status != PersonalAssessment.friendStatus.enemy) {
                        assessment.timeWarned = GameManager.Instance.sceneTime;
                        MessageSpeech message = new MessageSpeech();
                        if (!assessment.warned) {
                            message.phrase = "Halt! Come no closer!";
                        } else {
                            message.phrase = "I am sworn to protect this bridge";
                        }
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
            return storedAssessment;
        }
        PersonalAssessment assessment = new PersonalAssessment(knowledgebase[rootObject]);
        people.Add(rootObject, assessment);
        return assessment;
    }
    void AttackedByPerson(MessageDamage message) {
        if (hitState >= Controllable.HitState.unconscious)
            return;
        // adjust reaction depending on magnitude?
        GameObject g = message.responsibleParty;
        PersonalAssessment assessment = FormPersonalAssessment(g);
        if (assessment != null) {
            if (assessment.status != PersonalAssessment.friendStatus.friend) {
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
        if (assessment.numberOfTimesInsulted >= 2){
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
            MySaver.AddToReferenceTree(data.id, possession);
        }
        foreach (KeyValuePair<GameObject, Knowledge> keyVal in knowledgebase) {
            SerializedKnowledge knowledge = SaveKnowledge(keyVal.Value);
            if (knowledge.gameObjectID == -1)
                continue;
            data.knowledgeBase.Add(knowledge);
        }
        if (possessionDefaultState != null) {
            SerializedKnowledge knowledge = SaveKnowledge(possessionDefaultState);
            if (knowledge.gameObjectID != -1)
                data.knowledges["defaultState"] = knowledge;
        }
        foreach (KeyValuePair<GameObject, PersonalAssessment> keyVal in people) {
            data.people.Add(SavePerson(keyVal.Value));
        }
    }
    public void LoadData(PersistentComponent data) {
        if (data.ints.ContainsKey("possession")) {
            possession = MySaver.IDToGameObject(data.ints["possession"]);
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
        data.gameObjectID = -1;
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
        data.unconscious = input.unconscious;
        if (!MySaver.savedObjects.TryGetValue(input.knowledge.obj, out data.gameObjectID)) {
            data.gameObjectID = -1;
        }
        return data;
    }
    PersonalAssessment LoadPerson(SerializedPersonalAssessment input) {
        PersonalAssessment assessment = new PersonalAssessment();
        assessment.status = input.status;
        assessment.unconscious = input.unconscious;
        return assessment;
    }
}