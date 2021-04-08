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
    public enum CameraPreference { none, actor, avoidant, ambivalent, excited, eater, gravy };
    public enum PizzaDeliverer { no, yes };
    public enum Dancer { no, follower, leader };
    public enum Haunt { no, yes };
    public enum Insulter { no, yes };
    public Bravery bravery;
    public Stoicism stoicism;
    public BattleStyle battleStyle;
    public Suggestible suggestible;
    public Social social;
    public CombatProfficiency combatProficiency;
    public CameraPreference camPref;
    public PizzaDeliverer pizzaDeliverer;
    public Dancer dancer;
    public Haunt haunt;
    public Insulter insulter;
    public Personality(Bravery bravery, CameraPreference camPref, Stoicism stoicism, BattleStyle battleStyle, Suggestible suggestible, Social social, CombatProfficiency combatProficiency, PizzaDeliverer pizzaDeliverer, Dancer dancer, Haunt haunt, Insulter insulter) {
        this.bravery = bravery;
        this.camPref = camPref;
        this.stoicism = stoicism;
        this.battleStyle = battleStyle;
        this.suggestible = suggestible;
        this.social = social;
        this.combatProficiency = combatProficiency;
        this.pizzaDeliverer = pizzaDeliverer;
        this.dancer = dancer;
        this.haunt = haunt;
        this.insulter = insulter;
    }
}

public class DecisionMaker : MonoBehaviour, ISaveable {
    public enum PriorityType {
        Attack, FightFire, ProtectPossessions,
        ReadScript, RunAway, Wander, ProtectZone,
        MakeBalloonAnimals, InvestigateNoise, PrioritySocialize,
        Panic, Sentry, Trapdoor, DeliverPizza, Dance, Haunt, Taunt, SellSmoothies, SingPraise
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
        {typeof(PriorityDance), PriorityType.Dance},
        {typeof(PriorityApproachPlayer), PriorityType.Haunt},
        {typeof(PriorityTaunt), PriorityType.Taunt},
        {typeof(PrioritySellSmoothies), PriorityType.SellSmoothies},
        {typeof(PrioritySingPraise), PriorityType.SingPraise},

    };
    public string activePriorityName;
    public PriorityType defaultPriorityType;
    public Controller control;
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
    public Vector2 guardDirection = Vector2.right;
    public bool initialized = false;
    public bool debug;

    void Awake() {
        if (!initialized)
            Initialize();
    }

    public void Initialize() {
        if (initialized)
            return;
        // Debug.Log("decision maker initialize");
        initialized = true;
        // make sure there's Awareness
        awareness = Toolbox.GetOrCreateComponent<Awareness>(gameObject);
        awareness.decisionMaker = this;
        awareness.enabled = this.enabled;
        control = new Controller(GetComponent<Controllable>());
        control.lostControlDelegate += LostControl;
        control.gainedControlDelegate += GainedControl;
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
            InitializePriority(new PriorityProtectZone(gameObject, control, protectionZone, guardPoint, guardDirection), typeof(PriorityProtectZone));
        }
        if (defaultPriorityType == PriorityType.MakeBalloonAnimals) {
            InitializePriority(new PriorityMakeBalloonAnimals(gameObject, control), typeof(PriorityMakeBalloonAnimals));
        }
        if (defaultPriorityType == PriorityType.Sentry) {
            InitializePriority(new PrioritySentry(gameObject, control, guardPoint, guardDirection), typeof(PrioritySentry));
        }
        if (defaultPriorityType == PriorityType.Trapdoor) {
            InitializePriority(new PriorityTrapdoor(gameObject, control, transform.position), typeof(PriorityTrapdoor));
        }
        if (personality.pizzaDeliverer == Personality.PizzaDeliverer.yes) {
            InitializePriority(new PriorityDeliverPizza(gameObject, control), typeof(PriorityDeliverPizza));
        }
        if (personality.dancer == Personality.Dancer.follower || personality.dancer == Personality.Dancer.leader) {
            InitializePriority(new PriorityDance(gameObject, control), typeof(PriorityDance));
        }
        if (personality.haunt == Personality.Haunt.yes) {
            InitializePriority(new PriorityApproachPlayer(gameObject, control), typeof(PriorityApproachPlayer));
        }
        if (defaultPriorityType == PriorityType.Taunt || personality.insulter == Personality.Insulter.yes) {
            InitializePriority(new PriorityTaunt(gameObject, control), typeof(PriorityTaunt));
        }
        if (defaultPriorityType == PriorityType.SellSmoothies) {
            InitializePriority(new PrioritySellSmoothies(gameObject, control, guardPoint), typeof(PrioritySellSmoothies));
        }
        if (defaultPriorityType == PriorityType.SingPraise) {
            InitializePriority(new PrioritySingPraise(gameObject, control), typeof(PrioritySingPraise));
        }

        Toolbox.RegisterMessageCallback<MessageHitstun>(this, HandleHitStun);
        Toolbox.RegisterMessageCallback<Message>(this, ReceiveMessage);
    }
    public void LostControl() {
        if (activePriority != null)
            activePriority.ExitPriority();
    }
    public void GainedControl() {
        if (activePriority != null)
            activePriority.EnterPriority();
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
            if (debug) {
                Debug.Log($"{priority.priorityName}: {priority.urgency}");
            }
            if (priority.Urgency(personality) > activePriority.Urgency(personality))
                activePriority = priority;
        }
        activePriorityName = activePriority.priorityName;

        if (activePriority != oldActivePriority) {
            if (control.Authenticate(debug: true)) {
                control.ResetInput();
                if (oldActivePriority != null)
                    oldActivePriority.ExitPriority();
                activePriority.EnterPriority();
            }
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
        initialAwareness = new List<GameObject>(); // this probably won't work because load is called after Awake, but it shouldn't matter?
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
            InitializePriority(new PriorityProtectZone(gameObject, control, protectionZone, guardPoint, guardDirection), typeof(PriorityProtectZone));
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

            // this is a hack to prevent the pizza delivery boy from trying to deliver pizzas after save/load.
            // true solution would require making priorities stateful.
            if (defaultPriorityType == PriorityType.DeliverPizza) {
                if (priority.GetType() == typeof(PriorityWander)) {
                    defaultPriority = priority;
                }
                if (priority.GetType() == typeof(PriorityDeliverPizza)) {
                    PriorityDeliverPizza pdp = (PriorityDeliverPizza)priority;
                    pdp.boolSwitch.conditionMet = true;
                }
            }
        }
    }
}
