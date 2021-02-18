using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;
using System.Linq;

public class Gremlin : MonoBehaviour {
    class Module {
        float timer;
        public bool complete;
        public Gremlin gremlin;
        virtual public void Update() {
            timer += Time.deltaTime;
            if (timer > 5f) {
                complete = true;
            }
        }
        public Module(Gremlin gremlin) {
            this.gremlin = gremlin;
        }
    }
    abstract class WalkToModule<T> : Module where T : Component {
        RoutineWalkToGameobject walkToGameobject;
        protected T target;
        public WalkToModule(Gremlin gremlin) : base(gremlin) {
            target = SelectTarget();
            if (target == null) {
                complete = true;
            } else {
                walkToGameobject = new RoutineWalkToGameobject(gremlin.gameObject, gremlin.controller, new Ref<GameObject>(target.gameObject));
            }
        }
        virtual public T SelectTarget() {
            T[] pickups = GameObject.FindObjectsOfType<T>();
            if (pickups.Length > 0) {
                return pickups[Random.Range(0, pickups.Length)];
            } else {
                return null;
            }
        }
        public override void Update() {
            base.Update();
            if (target == null) {
                complete = true;
                return;
            } else {
                status stat = walkToGameobject.Update();
                if (stat == status.success) {
                    DoAction();
                    complete = true;
                }
            }
        }
        public abstract void DoAction();
    }
    class GrabSomethingModule : WalkToModule<Pickup> {
        public GrabSomethingModule(Gremlin gremlin) : base(gremlin) { }

        override public void DoAction() {
            if (gremlin.inventory.holding != null) {
                if (Random.Range(0, 1f) < 0.5) {
                    gremlin.inventory.DropItem();
                } else {
                    gremlin.inventory.StashItem(gremlin.inventory.holding.gameObject);
                }
            }
            gremlin.inventory.GetItem(target);
        }
    }
    class EatSomethingModule : WalkToModule<Edible> {
        public EatSomethingModule(Gremlin gremlin) : base(gremlin) { }
        override public Edible SelectTarget() {
            Edible[] pickups = GameObject.FindObjectsOfType<Edible>().Where(x => !x.inedible).ToArray();
            if (pickups.Length > 0) {
                return pickups[Random.Range(0, pickups.Length)];
            } else {
                return null;
            }
        }
        override public void DoAction() {
            if (!target.inedible)
                gremlin.eater.Eat(target);
        }
    }
    class ThrowModule : Module {
        public ThrowModule(Gremlin gremlin) : base(gremlin) {
            if (gremlin.inventory.holding) {
                gremlin.inventory.ActivateThrow();
            }
            complete = true;
        }
    }
    class DropModule : Module {
        public DropModule(Gremlin gremlin) : base(gremlin) {
            if (gremlin.inventory.holding) {
                gremlin.inventory.DropItem();
            }
            complete = true;
        }
    }
    class SwearModule : Module {
        public SwearModule(Gremlin gremlin) : base(gremlin) {
            GameObject target = null;
            if (Random.Range(0, 1) < 0.5f) {
                Item[] items = GameObject.FindObjectsOfType<Item>();
                if (items.Length > 0)
                    target = items[Random.Range(0, items.Length)].gameObject;
            }
            if (target != null) {
                if (Random.Range(0, 1f) < 0.5f) {
                    gremlin.speech.InsultMonologue(target);
                } else {
                    gremlin.speech.Swear(target: target);
                }
            }
            gremlin.speech.Swear(target: target);
        }
    }
    class DrinkContainerModule : WalkToModule<LiquidContainer> {
        public DrinkContainerModule(Gremlin gremlin) : base(gremlin) { }
        public override void DoAction() {
            if (target.amount > 0) {
                target.Drink(gremlin.eater);
            }
        }
    }
    class DrinkReservoirModule : WalkToModule<LiquidResevoir> {
        public DrinkReservoirModule(Gremlin gremlin) : base(gremlin) { }
        public override void DoAction() {
            target.Drink(gremlin.eater);
        }
    }
    class DivideModule : Module {
        public DivideModule(Gremlin gremlin) : base(gremlin) {
            if (GameObject.FindObjectsOfType<Gremlin>().Length < 5) {
                // play sfx
                GameObject newGrem = GameObject.Instantiate(Resources.Load("prefabs/gremlin"), gremlin.transform.position, Quaternion.identity) as GameObject;
                gremlin.transform.localScale = gremlin.transform.localScale * 0.75f;
                newGrem.transform.localScale = gremlin.transform.localScale.x * Vector3.one;
            }
        }
    }
    Module module;
    public Controller controller;
    public SimpleControl controllable;
    public Inventory inventory;
    public Speech speech;
    public Eater eater;
    RoutineWander wanderRoutine;
    float baseTimer;
    public void Start() {
        controllable = GetComponent<SimpleControl>();
        controller = new Controller(controllable);
        inventory = GetComponent<Inventory>();
        eater = GetComponent<Eater>();
        speech = GetComponent<Speech>();
        wanderRoutine = new RoutineWander(gameObject, controller);
        baseTimer = Random.Range(0, 3f);
    }
    void Update() {
        if (module == null) {
            BaseUpdate();
        } else {
            module.Update();
            if (module.complete) {
                module = null;
            }
        }
    }
    void BaseUpdate() {
        wanderRoutine.Update();
        baseTimer -= Time.deltaTime;
        if (baseTimer <= 0) {
            baseTimer = Random.Range(0, 2f);

            // decide on a random module here
            if (Random.Range(0, 1f) < 0.5f) {
                SetRandomModule();
            }

            if (inventory.holding) {
                controller.ShootPressed();
            }
        }
    }

    void SetRandomModule() {
        while (module == null) {
            float randomValue = Random.Range(0, 7f);
            if (randomValue < 1f) {
                module = new GrabSomethingModule(this);
            } else if (randomValue < 2f) {
                module = new EatSomethingModule(this);
            } else if (randomValue < 3f) {
                if (inventory.holding)
                    module = new ThrowModule(this);
            } else if (randomValue < 4f) {
                if (inventory.holding)
                    module = new DropModule(this);
            } else if (randomValue < 5f) {
                module = new SwearModule(this);
            } else if (randomValue < 6f) {
                module = new DrinkContainerModule(this);
                if (module.complete) {
                    module = new DrinkReservoirModule(this);
                }
            } else if (randomValue < 7f) {
                module = new DivideModule(this);
            }
        }

    }
}


// update module until complete, then spawn new module

// basic:
// * run around at random
// * if holding, press F randomly

// then every few seconds, choose at random from the list:
// run to random item
//      use them
// * drop item
// * eat items
// * throw item
// * grab
// disable / enable z-locking
// * swear / insult
// * drink something
// * scale flipper
// * voice
// * throw
// * divide into two
// put on hats?

// * should be hurtable / killable / gibbable
//      batteries drop out
// *should be collectible
