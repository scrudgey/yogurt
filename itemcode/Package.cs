using UnityEngine;

public class Package : Interactive, ISaveable {
    public string contents;
    void Awake() {
        if (contents == "none")
            return;
        Interaction openAct = new Interaction(this, "Open", "Open");
        interactions.Add(openAct);
    }
    void Start() {
        Gibs gibs = gameObject.AddComponent<Gibs>();
        GameObject content = Resources.Load("prefabs/" + contents) as GameObject;
        if (content == null) {
            content = Resources.Load("prefabs/eggplant") as GameObject;
            Debug.Log($"package could not find {contents}");
        }
        gibs.particle = content;
        gibs.number = 1;
        gibs.damageCondition = damageType.any;
        Bones bones = GetComponent<Bones>();
        if (bones != null) {
            // set sprite
            SpriteRenderer objRenderer = content.GetComponent<SpriteRenderer>();
            bones.boneSprite = objRenderer.sprite;
            bones.Start();
        }
    }
    public void Open() {
        Destructible destructo = GetComponent<Destructible>();
        destructo.Die();
        if (!GameManager.Instance.data.collectedObjects.Contains(contents)) {
            MusicController.Instance.EnqueueMusic(new MusicItem());
        }
    }
    public string Open_desc() {
        return "Open package";
    }
    public void SaveData(PersistentComponent data) {
        data.strings["contents"] = contents;
    }
    public void LoadData(PersistentComponent data) {
        contents = data.strings["contents"];
    }
}
