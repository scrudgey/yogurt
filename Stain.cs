using UnityEngine;

public class Stain : MonoBehaviour, ISaveable {
    public SpriteMask parentMask;
    public SpriteRenderer parentRenderer;
    public GameObject parent;
    void Update() {
        if (parentRenderer != null) {
            parentMask.sprite = parentRenderer.sprite;
        }
    }
    public void ConfigureParentObject(GameObject parent) {
        this.parent = parent;
        transform.position = parent.transform.position;

        parentRenderer = parent.GetComponent<SpriteRenderer>();
        SpriteRenderer stainRenderer = GetComponent<SpriteRenderer>();
        stainRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        parentMask = parent.GetComponent<SpriteMask>();
        if (parentMask == null) {
            int sortingId = Random.Range(500, 32760);
            parentMask = Toolbox.GetOrCreateComponent<SpriteMask>(parent);
            parentMask.isCustomRangeActive = true;
            parentMask.frontSortingLayerID = SortingLayer.NameToID("main");
            parentMask.backSortingLayerID = SortingLayer.NameToID("main");
            parentMask.frontSortingOrder = sortingId + 1;
            parentMask.backSortingOrder = sortingId;

            stainRenderer.sortingOrder = sortingId;
        } else {
            stainRenderer.sortingOrder = parentMask.backSortingOrder;
        }
        transform.SetParent(parent.transform, true);
    }
    public void RemoveStain() {
        Destroy(gameObject);
    }
    public void SaveData(PersistentComponent data) {
        MySaver.UpdateGameObjectReference(parent, data, "parentID");
        // reverse tree add: place me under the tree of my parent
        MySaver.AddToReferenceTree(parent, data.id);
        MonoLiquid stainLiquid = GetComponent<MonoLiquid>();
        if (stainLiquid != null)
            data.liquids["liquid"] = stainLiquid.liquid;
        // data.strings["liquid"] = stainLiquid.liquid.filename;
    }
    public void LoadData(PersistentComponent data) {
        if (data.GUIDs["parentID"] != System.Guid.Empty) {
            ConfigureParentObject(MySaver.IDToGameObject(data.GUIDs["parentID"]));
            if (data.strings.ContainsKey("liquid")) {
                Liquid.MonoLiquidify(gameObject, data.liquids["liquid"]);
                // Liquid.MonoLiquidify(gameObject, Liquid.LoadLiquid(data.strings["liquid"]));
            }
        } else {
            RemoveStain();
        }
    }
}
