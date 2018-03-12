using UnityEngine;
public class LiquidResevoir : Interactive {
    public Liquid liquid;
    public AudioClip fillSound;
    public string initLiquid;
    public string genericName;
    void Awake() {
        liquid = Liquid.LoadLiquid(initLiquid);
    }
}
