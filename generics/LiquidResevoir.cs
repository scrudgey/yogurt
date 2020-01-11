﻿using UnityEngine;
public class LiquidResevoir : Interactive {
    public Liquid liquid;
    public AudioClip fillSound;
    public string initLiquid;
    public string genericName;
    public bool drinkable;
    public AudioClip[] drinkSounds;
    void Awake() {
        liquid = Liquid.LoadLiquid(initLiquid);
        if (liquid.buffs.Count > 0) {
            Intrinsics intrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(gameObject);
            intrinsics.buffs.AddRange(liquid.buffs);
        }
        if (drinkable) {
            Interaction drinker = new Interaction(this, "Drink", "Drink");
            drinker.playerOnOtherConsent = true;
            interactions.Add(drinker);
        }
    }
    public void Drink(Eater eater) {
        if (eater) {
            GameObject sip = Instantiate(Resources.Load("prefabs/droplet"), transform.position, Quaternion.identity) as GameObject;
            // sip.AddComponent<MonoLiquid>();
            Liquid.MonoLiquidify(sip, liquid);
            eater.Eat(sip.GetComponent<Edible>());

            if (drinkSounds.Length > 0) {
                Toolbox.Instance.AudioSpeaker(drinkSounds[Random.Range(0, drinkSounds.Length - 1)], transform.position);
            }
            GameManager.Instance.CheckItemCollection(gameObject, eater.gameObject);
        }
    }
    public string Drink_desc(Eater eater) {
        string myname = Toolbox.Instance.GetName(gameObject);
        return "Drink " + liquid.name + " from " + myname;
    }
}
