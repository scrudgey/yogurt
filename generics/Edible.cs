using UnityEngine;
using System.Collections.Generic;
public class Edible : Interactive, ISaveable {
    public bool inedible;
    public float nutrition;
    public bool vegetable;
    public bool meat;
    public bool immoral;
    public bool offal;
    public bool vomit;
    public bool blendable;
    public string blend_liquid_name;
    public bool human;
    public Color pureeColor;
    public AudioClip eatSound;
    public GameObject refuse;
    void Start() {
        if (eatSound == null) {
            eatSound = Resources.Load("sounds/eating/bite") as AudioClip;
        }
        if (nutrition == 0) {
            nutrition = 1;
        }
        if (vegetable || meat || immoral || offal || vomit == false) {
            vegetable = true;
        }
    }
    virtual public void BeEaten() {
        PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
        if (pb) {
            pb.DestroyPhysical();
        }
        ClaimsManager.Instance.WasDestroyed(gameObject);
        if (refuse != null) {
            Instantiate(refuse, transform.position, transform.rotation);
        }
    }
    public Liquid Liquify() {
        Liquid returnLiquid = null;
        if (blend_liquid_name != "") {
            returnLiquid = Liquid.LoadLiquid(blend_liquid_name);
        } else {
            returnLiquid = Liquid.LoadLiquid("juice");
            returnLiquid.vegetable = vegetable ? 1 : 0;
            returnLiquid.meat = meat ? 1 : 0;
            returnLiquid.immoral = immoral ? 1 : 0;
            returnLiquid.nutrition = nutrition / 10;
            returnLiquid.color = pureeColor;
            returnLiquid.color.a = 255f;
            returnLiquid.name = Toolbox.Instance.GetName(gameObject) + " juice";

            returnLiquid.atomicLiquids = new HashSet<Liquid>();
            returnLiquid.atomicLiquids.Add(new Liquid(returnLiquid));
        }
        returnLiquid.ingredients.Add(Toolbox.Instance.CloneRemover(gameObject.name));
        return returnLiquid;
    }
    public void SaveData(PersistentComponent data) {
        data.bools["vomit"] = vomit;
    }
    public void LoadData(PersistentComponent data) {
        vomit = data.bools["vomit"];
    }
}
