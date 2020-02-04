using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AI;
using System;

[System.Serializable]
public class Personality {
    public enum Bravery { neutral, cowardly, brave };
    public enum Stoicism { neutral, fragile, noble };
    public enum BattleStyle { normal, bloodthirsty };
    public enum Suggestible { normal, suggestible, stubborn };
    public enum Social { normal, chatty, reserved };
    public enum CombatProfficiency { normal, poor, expert };

    public enum CameraPreference { none, actor, avoidant, ambivalent, excited, eater };
    public enum PizzaDeliverer { no, yes };
    public Bravery bravery;
    public Stoicism stoicism;
    public BattleStyle battleStyle;
    public Suggestible suggestible;
    public Social social;
    public CombatProfficiency combatProficiency;
    public CameraPreference camPref;
    public PizzaDeliverer pizzaDeliverer;
    public Personality(Bravery bravery, CameraPreference camPref, Stoicism stoicism, BattleStyle battleStyle, Suggestible suggestible, Social social, CombatProfficiency combatProficiency, PizzaDeliverer pizzaDeliverer) {
        this.bravery = bravery;
        this.camPref = camPref;
        this.stoicism = stoicism;
        this.battleStyle = battleStyle;
        this.suggestible = suggestible;
        this.social = social;
        this.combatProficiency = combatProficiency;
        this.pizzaDeliverer = pizzaDeliverer;
    }
}

public class DecisionMaker : MonoBehaviour, ISaveable {
    public enum PriorityType {
        Attack, FightFire, ProtectPossessions,
        ReadScript, RunAway, Wander, ProtectZone,
        MakeBalloonAnimals, InvestigateNoise, PrioritySocialize,
        Panic, Sentry, Trapdoor, DeliverPizza
    }
    // the ONLY reason we need this redundant structure is so that I can expose
    // a selectable default priority type in unity editor.
    static Dictionary<Type, PriorityType> priorityTypes = new Dictionary<Type, PriorityType>(){
        {typeof(PriorityAttack), PriorityType.Attack},
        {typeof(PriorityFightFire), PriorityType.FightFire},
        {typeof(PriorityProtectPossessions), PriorityType.ProtectPossessions},
        {typeof(PriorityReactToCamera), PriorityType.ReadScript},
        {typeof(PriorityRunAway), PriorityType.RunAway},
        {typeof(PriorityWander), PriorityType.Wander},
        {typeof(PriorityProtectZone), PriorityType.ProtectZone},
        {typeof(PriorityMakeBalloonAnimals), PriorityType.MakeBalloonAnimals},
        {typeof(PriorityInvestigateNoise), PriorityType.InvestigateNoise},
        {typeof(PrioritySocialize), PriorityType.PrioritySocialize},
        {typeof(PriorityPanic), PriorityType.Panic},
        {typeof(PrioritySentry), PriorityType.Sentry},
        {typeof(PriorityTrapdoor), PriorityType.Trapdoor},
        {typeof(PriorityDeliverPizza), PriorityType.DeliverPizza},
    };
    public string activePriorityName;
    public PriorityType defaultPriorityType;
    public Controllable control;
    public GameObject thought;
    public Text thoughtText;
    public List<Priority> priorities;
    public Personality personality;
    public Priority defaultPriority;
    public Priority activePriority = null;
    public List<GameObject> initialAwareness;
    public GameObject possession;
    public Controllable.HitState hitState;
    public Awareness awareness;
    public BoxCollider2D protectionZone = null;
    public Collider2D warnZone = null;
    public Vector3 guardPoint;
    public bool initialized = false;
    // public priorityType
    void Awake() {
        if (!initialized)
            Initialize();
    }
    public void Initialize() {

        // Debug.Log("decision maker initialize");
        initialized = true;
        // make sure there's Awareness
        awareness = Toolbox.GetOrCreateComponent<Awareness>(gameObject);
        awareness.decisionMaker = this;
        awareness.enabled = this.enabled;
        control = GetComponent<Controllable>();

        // start awareness with knowledge of possessions
        if (possession != null) {
            initialAwareness.Add(possession);
            awareness.possession = possession;
        }
        awareness.initialAwareness = initialAwareness;
        awareness.protectZone = protectionZone;
        awareness.warnZone = warnZone;

        // initialize thought bubble
        thought = Instantiate(Resources.Load("UI/thoughtbubble"), gameObject.transform.position, Quaternion.identity) as GameObject;
        DistanceJoint2D dj = thought.GetComponent<DistanceJoint2D>();
        dj.connectedBody = GetComponent<Rigidbody2D>();
        dj.distance = 0.3f;
        thoughtText = thought.GetComponentInChildren<Text>();
        thoughtText.text = "";

        // create priorities
        priorities = new List<Priority>();
        if (personality.social != Personality.Social.reserved)
            InitializePriority(new PrioritySocialize(gameObject, control), typeof(PrioritySocialize));
        InitializePriority(new PriorityFightFire(gameObject, control), typeof(PriorityFightFire));
        InitializePriority(new PriorityRunAway(gameObject, control), typeof(PriorityRunAway));
        InitializePriority(new PriorityAttack(gameObject, control), typeof(PriorityAttack));
        InitializePriority(new PriorityProtectPossessions(gameObject, control), typeof(PriorityProtectPossessions));
        InitializePriority(new PriorityWander(gameObject, control), typeof(PriorityWander));
        InitializePriority(new PriorityInvestigateNoise(gameObject, control), typeof(PriorityInvestigateNoise));
        InitializePriority(new PriorityPanic(gameObject, control), typeof(PriorityPanic));
        if (personality.camPref != Personality.CameraPreference.none) {
            InitializePriority(new PriorityReactToCamera(gameObject, control, personality.camPref), typeof(PriorityReactToCamera));
        }
        if (protectionZone != null) {
            InitializePriority(new PriorityProtectZone(gameObject, control, protectionZone, guardPoint), typeof(PriorityProtectZone));
        }
        if (defaultPriorityType == PriorityType.MakeBalloonAnimals) {
            InitializePriority(new PriorityMakeBalloonAnimals(gameObject, control), typeof(PriorityMakeBalloonAnimals));
        }
        if (defaultPriorityType == PriorityType.Sentry) {
            InitializePriority(new PrioritySentry(gameObject, control), typeof(PrioritySentry));
        }
        if (defaultPriorityType == PriorityType.Trapdoor) {
            InitializePriority(new PriorityTrapdoor(gameObject, control, transform.position), typeof(PriorityTrapdoor));
        }
        if (personality.pizzaDeliverer == Personality.PizzaDeliverer.yes) {
            InitializePriority(new PriorityDeliverPizza(gameObject, control), typeof(PriorityDeliverPizza));
        }

        Toolbox.RegisterMessageCallback<MessageHitstun>(this, HandleHitStun);
        Toolbox.RegisterMessageCallback<Message>(this, ReceiveMessage);
    }
    public void InitializePriority(Priority priority, Type type) {
        priorities.Add(priority);
        if (defaultPriorityType == priorityTypes[type]) {
            defaultPriority = priority;
        }
    }
    public void HandleHitStun(MessageHitstun message) {
        hitState = message.hitState;
    }
    public void ReceiveMessage(Message message) {
        if (hitState >= Controllable.HitState.unconscious)
            return;
        foreach (Priority priority in priorities) {
            priority.ReceiveMessage(message);
        }
    }
    public void Update() {
        if (hitState >= Controllable.HitState.stun)
            return;
        Priority oldActivePriority = activePriority;
        activePriority = defaultPriority;
        foreach (Priority priority in priorities) {
            priority.Update();
            if (activePriority == null)
                activePriority = priority;

            if (priority.urgency > priority.minimumUrgency)
                priority.urgency -= Time.deltaTime / 10f;
            if (priority.urgency < priority.minimumUrgency)
                priority.urgency += Time.deltaTime / 10f;
            priority.urgency = Mathf.Min(priority.urgency, Priority.urgencyMaximum);
            // if (priority.Urgency(personality) <= Priority.urgencyMinor)
            //     continue;
            if (priority.Urgency(personality) > activePriority.Urgency(personality))
                activePriority = priority;
        }
        activePriorityName = activePriority.priorityName;

        if (activePriority != oldActivePriority) {
            control.ResetInput();
            if (oldActivePriority != null)
                oldActivePriority.ExitPriority();
            activePriority.EnterPriority();
        }
        if (activePriority != null) {
            // Debug.Log(activePriority.ToString() + " " + activePriority.Urgency(personality).ToString());
            activePriority.DoAct();
            // thoughtText = activePriority.goal.
            // Debug.Log(gameObject.name + " " + activePriority.GetType().ToString());
        }
    }
    void OnDestroy() {
        if (thought) {
            Destroy(thought);
        }
    }
    public void SaveData(PersistentComponent data) {
        data.ints["hitstate"] = (int)hitState;
        foreach (Type priorityType in priorityTypes.Keys) {
            foreach (Priority priority in priorities) {
                if (priority.GetType() == priorityType) {
                    data.floats[priorityType.ToString()] = priority.urgency;
                }
            }
        }
        if (protectionZone != null)
            MySaver.UpdateGameObjectReference(protectionZone.gameObject, data, "protectID", overWriteWithNull: false);
        if (warnZone != null)
            MySaver.UpdateGameObjectReference(warnZone.gameObject, data, "warnID", overWriteWithNull: false);
        data.vectors["guardPoint"] = guardPoint;
    }
    public void LoadData(PersistentComponent data) {
        hitState = (Controllable.HitState)data.ints["hitstate"];
        initialAwareness = new List<GameObject>();
        guardPoint = data.vectors["guardPoint"];
        if (data.GUIDs.ContainsKey("protectID")) {
            GameObject protectObject = MySaver.IDToGameObject(data.GUIDs["protectID"]);
            if (protectObject != null) {
                awareness.protectZone = protectObject.GetComponent<BoxCollider2D>();
                protectionZone = (BoxCollider2D)awareness.protectZone;
            }
        }
        if (data.GUIDs.ContainsKey("warnID")) {
            GameObject protectObject = MySaver.IDToGameObject(data.GUIDs["protectID"]);
            GameObject warnObject = MySaver.IDToGameObject(data.GUIDs["warnID"]);
            if (warnObject != null) {
                awareness.warnZone = warnObject.GetComponent<Collider2D>();
                warnZone = awareness.warnZone;
            }
        }
        if (protectionZone != null) {
            priorities.Add(new PriorityProtectZone(gameObject, control, protectionZone, guardPoint));
        }

        foreach (Priority priority in priorities) {
            foreach (Type priorityType in priorityTypes.Keys) {
                if (priority.GetType() == priorityType) {
                    string priorityName = priorityType.ToString();
                    if (data.floats.ContainsKey(priorityName)) {
                        priority.urgency = data.floats[priorityName];
                    }
                }
            }
        }
    }
}
