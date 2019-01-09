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
        parentMask = Toolbox.GetOrCreateComponent<SpriteMask>(parent);
        parentRenderer = parent.GetComponent<SpriteRenderer>();
        SpriteRenderer stainRenderer = GetComponent<SpriteRenderer>();
        stainRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        transform.SetParent(parent.transform, true);

        parentMask.isCustomRangeActive = true;
        parentMask.frontSortingLayerID = SortingLayer.NameToID("air");
        parentMask.backSortingLayerID = SortingLayer.NameToID("air");
        parentMask.frontSortingOrder = 100;
        parentMask.backSortingOrder = 0;
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
            data.strings["liquid"] = stainLiquid.liquid.filename;
    }
    public void LoadData(PersistentComponent data) {
        if (data.ints["parentID"] != -1) {
            ConfigureParentObject(MySaver.IDToGameObject(data.ints["parentID"]));
            if (data.strings.ContainsKey("liquid")) {
                Liquid.MonoLiquidify(gameObject, Liquid.LoadLiquid(data.strings["liquid"]));
            }
        } else {
            RemoveStain();
        }
    }
}
