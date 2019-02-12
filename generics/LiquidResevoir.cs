using UnityEngine;
public class LiquidResevoir : Interactive {
    public Liquid liquid;
    public AudioClip fillSound;
    public string initLiquid;
    public string genericName;
    void Awake() {
        liquid = Liquid.LoadLiquid(initLiquid);
        if (liquid.buffs.Count > 0) {
            Intrinsics intrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(gameObject);
            intrinsics.buffs.AddRange(liquid.buffs);
        }
    }
}
