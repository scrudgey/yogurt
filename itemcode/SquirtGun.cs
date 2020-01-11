using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquirtGun : LiquidContainer, IDirectable {
    public Transform right;
    public Transform down;
    public Transform up;
    public float height;
    public string lastPressed;
    public float velocity;
    public AudioClip squirtSound;
    private AudioSource audioSource;
    override public void Awake() {
        if (!configured) {
            configured = true;
            Interaction fillReservoir = new Interaction(this, "Fill", "FillFromReservoir");
            Interaction fillContainer = new Interaction(this, "Fill", "FillFromContainer");
            fillContainer.validationFunction = true;
            interactions.Add(fillContainer);
            interactions.Add(fillReservoir);
            Interaction squirtAction = new Interaction(this, "Shoot", "Squirt", false, true);
            squirtAction.defaultPriority = 6;
            squirtAction.validationFunction = true;
            interactions.Add(squirtAction);
            Interaction spray2 = new Interaction(this, "Shoot", "SprayObject", true, false);
            spray2.unlimitedRange = true;
            spray2.dontWipeInterface = true;
            spray2.validationFunction = true;
            interactions.Add(spray2);

            empty = true;
            if (liquidSprite)
                liquidSprite.enabled = false;
            if (initLiquid != "") {
                FillByLoad(initLiquid);
            }
            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        }
    }
    public void Squirt() {
        Transform direction;
        switch (lastPressed) {
            case "down":
                direction = down;
                break;
            case "up":
                direction = up;
                break;
            default:
                direction = right;
                break;
        }
        doSquirt(direction.up, direction.position);
    }
    public bool Squirt_Validation() {
        return amount > 0;
    }
    public void SprayObject(GameObject item) {
        Vector3 direction = (item.transform.position - transform.position).normalized;
        doSquirt(direction, transform.position);
    }

    public string SprayObject_desc(GameObject item) {
        string itemname = Toolbox.Instance.GetName(item.gameObject);
        return "Shoot " + liquid.name + " at " + itemname;
    }
    public bool SprayObject_Validation(GameObject item) {
        if (amount <= 0)
            return false;
        if (item != gameObject) {
            return true;
        } else {
            return false;
        }
    }
    public void doSquirt(Vector3 direction, Vector3 position) {
        GameObject droplet = Toolbox.Instance.SpawnDroplet(liquid, 0, gameObject, 0.05f, velocity * direction, noCollision: false);
        droplet.transform.position = position;
        audioSource.PlayOneShot(squirtSound);
        foreach (Collider2D myCollider in transform.root.GetComponentsInChildren<Collider2D>()) {
            foreach (Collider2D dropCollider in droplet.transform.root.GetComponentsInChildren<Collider2D>()) {
                Physics2D.IgnoreCollision(myCollider, dropCollider, true);
            }
        }
        amount -= 1f;
    }
    public void DirectionChange(Vector2 newdir) {
        lastPressed = Toolbox.Instance.DirectionToString(newdir);
    }
}
