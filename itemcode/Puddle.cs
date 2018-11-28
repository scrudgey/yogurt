using UnityEngine;
using System.Collections.Generic;

public class Puddle : MonoBehaviour {
    public enum moveState { unmoved, set }
    public float amount;
    private SpriteRenderer spriteRenderer;
    private moveState state = moveState.unmoved;
    private List<GameObject> ignoredObjects = new List<GameObject>();
    void Start() {
        amount = 1;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "background";
        spriteRenderer.sortingOrder = 100;
        // set random small puddle sprite
        Sprite[] sprites = Resources.LoadAll<Sprite>("sprites/smallpuddle");
        spriteRenderer.sprite = sprites[Random.Range(0, 4)];
        MonoLiquid monoliquid = GetComponent<MonoLiquid>();
        if (monoliquid) {
            Item item = GetComponent<Item>();
            item.itemName = "puddle of " + monoliquid.liquid.name;
        }
    }

    void OnTriggerStay2D(Collider2D coll) {
        if (coll.gameObject.tag == "fire")
            return;
        if (ignoredObjects.Contains(coll.gameObject))
            return;
        Puddle otherPuddle = coll.gameObject.GetComponent<Puddle>();
        if (otherPuddle) {
            if (state == moveState.unmoved) {
                if (Random.Range(0, 1) < 0.5) {
                    state = moveState.set;
                    Vector2 position = transform.position;
                    Vector2 randomWalk = Random.insideUnitCircle;
                    randomWalk = randomWalk.normalized * 0.02f;
                    position = position + randomWalk;
                    transform.position = position;
                } else {
                    ClaimsManager.Instance.WasDestroyed(coll.gameObject);
                    Destroy(coll.gameObject);
                    Sprite[] sprites = Resources.LoadAll<Sprite>("sprites/mediumpuddle");
                    spriteRenderer.sprite = sprites[Random.Range(0, 4)];
                    amount += otherPuddle.amount;
                    state = moveState.set;
                }
            }
        }
        ignoredObjects.Add(coll.gameObject);
    }
}
