using UnityEngine;
using System.Collections.Generic;
public class ChemicalSpray : MonoBehaviour {
    public GameObject collisionPlume;
    public GameObject responsibleParty;
    private float height;
    public bool staining;
    void OnCollisionEnter2D(Collision2D coll) {
        Instantiate(collisionPlume, transform.position, Quaternion.identity);
        foreach (Flammable flam in coll.gameObject.GetComponentsInParent<Flammable>()) {
            if (flam.onFire) {
                flam.onFire = false;
                flam.heat = -10f;
                flam.SetBurnTimer();
                OccurrenceFire fireData = new OccurrenceFire();
                fireData.flamingObject = coll.gameObject;
                fireData.extinguished = true;

                Toolbox.Instance.OccurenceFlag(coll.gameObject, fireData);
            }
        }
        Toolbox.Instance.AddLiveBuffs(coll.gameObject, gameObject);
        MessageDamage impact = new MessageDamage();
        impact.amount = 0;
        impact.responsibleParty = responsibleParty;
        impact.suppressImpactSound = true;
        Toolbox.Instance.SendMessage(coll.gameObject, this, impact);
        if (staining && coll.collider.gameObject.layer == LayerMask.NameToLayer("main")) {
            GameObject splash = Instantiate(Resources.Load("prefabs/splash"), transform.position, Quaternion.identity) as GameObject;
            SpriteRenderer splashSprite = splash.GetComponent<SpriteRenderer>();
            splashSprite.color = Color.red;
            Transform rectTransform = splash.GetComponent<Transform>();
            rectTransform.Rotate(new Vector3(0, 0, Random.Range(0f, 360f)));

            int numberStains = 0;
            foreach (Transform child in coll.transform) {
                if (child.name == "stain(Clone)") {
                    numberStains += 1;
                    break;
                }
            }
            if (numberStains == 0) {
                GameObject stainObject = Instantiate(Resources.Load("prefabs/stain")) as GameObject;
                stainObject.transform.position = transform.position;
                Stain stain = stainObject.GetComponent<Stain>();
                stain.ConfigureParentObject(coll.gameObject);
                SpriteRenderer stainRenderer = stain.GetComponent<SpriteRenderer>();
                stainRenderer.color = Color.red;
                Liquid.MonoLiquidify(stain.gameObject, Liquid.LoadLiquid("red_paint"));
                Flammable stainFlammable = stain.GetComponentInParent<Flammable>();
                Flammable myFlammable = GetComponent<Flammable>();
                if (stainFlammable != null && myFlammable != null && myFlammable.onFire) {
                    stainFlammable.SpontaneouslyCombust();
                    stainFlammable.responsibleParty = myFlammable.responsibleParty;
                }
            }
        }
        Destroy(gameObject);
    }
}
