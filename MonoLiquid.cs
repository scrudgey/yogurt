﻿using UnityEngine;
public class MonoLiquid : MonoBehaviour, ISaveable {
    public Liquid liquid;
    public Edible edible;
    public PhysicalBootstrapper physB;
    public bool ignoreCollisions;
    static GameObject CreateStain(GameObject target, Vector3 location) {
        GameObject stainObject = Instantiate(Resources.Load("prefabs/stain")) as GameObject;
        stainObject.transform.position = location;
        Stain stain = stainObject.GetComponent<Stain>();
        stain.ConfigureParentObject(target);
        return stainObject;
    }
    void Awake() {
        edible = GetComponent<Edible>();
        if (!edible)
            edible = gameObject.AddComponent<Edible>();
        physB = GetComponent<PhysicalBootstrapper>();
        liquid = Liquid.LoadLiquid("water");
    }
    void Start() {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer && liquid != null) {
            spriteRenderer.color = liquid.color;
        }
    }
    void OnCollisionEnter2D(Collision2D coll) {
        if (ignoreCollisions)
            return;
        if (physB == null)
            return;
        if (physB.physical == null)
            return;
        if (coll.collider.gameObject.layer == LayerMask.NameToLayer("main"))
            if (physB.physical.currentMode == Physical.mode.fly) {
                GameObject splash = Instantiate(Resources.Load("prefabs/splash"), transform.position, Quaternion.identity) as GameObject;
                SpriteRenderer splashSprite = splash.GetComponent<SpriteRenderer>();
                splashSprite.color = liquid.color;
                Transform rectTransform = splash.GetComponent<Transform>();
                rectTransform.Rotate(new Vector3(0, 0, Random.Range(0f, 360f)));

                int numberStains = 0;
                foreach (Transform child in coll.transform) {
                    if (child.name == "stain(Clone)") {
                        numberStains += 1;
                        if (numberStains >= 3)
                            break;
                    }
                }
                if (numberStains < 3) {
                    GameObject stain = CreateStain(coll.gameObject, transform.position);
                    Toolbox.Instance.AddLiveBuffs(coll.gameObject, gameObject);
                    SpriteRenderer stainRenderer = stain.GetComponent<SpriteRenderer>();
                    stainRenderer.color = liquid.color;
                    Liquid.MonoLiquidify(stain, liquid);
                    Flammable stainFlammable = stain.GetComponentInParent<Flammable>();
                    Flammable myFlammable = GetComponent<Flammable>();
                    if (stainFlammable != null && myFlammable != null && myFlammable.onFire) {
                        stainFlammable.SpontaneouslyCombust();
                        stainFlammable.responsibleParty = myFlammable.responsibleParty;
                    }

                    EventData data = Toolbox.Instance.DataFlag(
                        coll.gameObject,
                        "staining",
                        liquid.name + " stained " + Toolbox.Instance.GetName(coll.gameObject),
                        chaos: 1,
                        disgusting: 1);
                    // data.noun = "staining";
                    // data.whatHappened = liquid.name + " stained " + Toolbox.Instance.GetName(coll.gameObject);
                    if (liquid.immoral > 0) {
                        data.quality[Rating.chaos] += 1;
                        data.quality[Rating.disturbing] += 1;
                    }
                    if (liquid.vomit) {
                        data.quality[Rating.chaos] += 1;
                        data.quality[Rating.disgusting] += 1;
                    }
                    if (liquid.offal > 0) {
                        data.quality[Rating.chaos] += 1;
                        data.quality[Rating.disgusting] += 1;
                    }
                }
                ClaimsManager.Instance.WasDestroyed(gameObject);
                Destroy(gameObject);
            }
    }
    public void LoadLiquid(string type) {
        // TODO: check for null return
        liquid = Liquid.LoadLiquid(type);
    }
    void OnGroundImpact(Physical phys) {
        EventData data = Toolbox.Instance.DataFlag(
            gameObject,
            $"{liquid.name} spilling",
            $"{liquid.name} was spilled",
            chaos: 1,
            disgusting: 1);
        // data.whatHappened = liquid.name + " was spilled";
        // data.noun = "spilling";
        GameObject puddle = Instantiate(Resources.Load("prefabs/Puddle"), transform.position, Quaternion.identity) as GameObject;
        PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
        if (pb != null)
            pb.DestroyPhysical();
        Liquid.MonoLiquidify(puddle, liquid);
        Edible puddleEdible = puddle.GetComponent<Edible>();
        Edible edible = GetComponent<Edible>();
        if (edible) {
            puddleEdible.human = edible.human;
            puddleEdible.offal = true;
        }
        Destroy(gameObject);
        ClaimsManager.Instance.WasDestroyed(gameObject);
        Flammable puddleFlammable = puddle.GetComponent<Flammable>();
        Flammable myFlammable = GetComponent<Flammable>();
        if (puddleFlammable != null && myFlammable != null && myFlammable.onFire) {
            puddleFlammable.SpontaneouslyCombust();
            puddleFlammable.responsibleParty = myFlammable.responsibleParty;
        }
    }
    public void SaveData(PersistentComponent data) {
        // data
        data.liquids["liquid"] = liquid;
    }
    public void LoadData(PersistentComponent data) {
        Liquid.MonoLiquidify(gameObject, data.liquids["liquid"]);
    }
}
