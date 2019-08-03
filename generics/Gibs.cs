using UnityEngine;

public class Gibs : MonoBehaviour {
    [System.Serializable]
    public class LoHi {
        public float low;
        public float high;
        public LoHi() : this(0.025f, 0.075f) { }
        public LoHi(float low, float high) {
            this.low = low;
            this.high = high;
        }
    }
    public bool applyAnimationSkinColor;
    public damageType damageCondition;
    public int number;
    public GameObject particle;
    public Color color = Color.white;
    public bool notPhysical;
    public LoHi initHeight = new LoHi(0.025f, 0.075f);
    public LoHi initVelocity = new LoHi(0.5f, 0.5f);
    public LoHi initAngleFromHorizontal = new LoHi(0.1f, 0.9f);
    public void Emit(damageType dam, Vector2 baseDir) {
        if (!DamageTypeMatch(damageCondition, dam))
            return;
        for (int i = 0; i < number; i++) {
            GameObject bit = Instantiate(particle, transform.position, Quaternion.identity) as GameObject;
            SpriteRenderer sprite = bit.GetComponent<SpriteRenderer>();
            sprite.color = color;
            if (applyAnimationSkinColor) {
                AdvancedAnimation advancedAnimation = GetComponent<AdvancedAnimation>();
                SkinColorizer colorizer = Toolbox.GetOrCreateComponent<SkinColorizer>(bit);
                if (advancedAnimation) {
                    colorizer.skinColor = advancedAnimation.skinColor;
                }
            }
            Damageable damageable = bit.GetComponent<Damageable>();
            if (damageable) {
                Toolbox.Instance.DisableAndReenable(damageable, 0.2f);
            }
            if (notPhysical)
                return;
            // Rigidbody2D bitBody = Toolbox.GetOrCreateComponent<Rigidbody2D>(bit);
            PhysicalBootstrapper bitPhys = Toolbox.GetOrCreateComponent<PhysicalBootstrapper>(bit);
            PhysicalBootstrapper myBoot = GetComponent<PhysicalBootstrapper>();
            bitPhys.impactsMiss = true;
            bitPhys.noCollisions = true;
            if (bitPhys.size == PhysicalBootstrapper.shadowSize.normal)
                bitPhys.size = PhysicalBootstrapper.shadowSize.medium;
            bitPhys.initHeight = Random.Range(initHeight.low, initHeight.high);
            if (myBoot) {
                if (myBoot.physical != null) {
                    bitPhys.initHeight = myBoot.physical.height;
                }
            }
            Vector3 randomWalk = 0.05f * Random.insideUnitCircle.normalized;
            randomWalk.z = 0;
            bit.transform.position = transform.position + randomWalk;

            // TODO: figure this out
            float magnitude = Random.Range(initVelocity.low, initVelocity.high);
            // Debug.Log(baseDir.magnitude);
            Vector3 force = magnitude * Toolbox.Instance.RandomVector(baseDir.normalized, 45f);
            float phi = Random.Range(initAngleFromHorizontal.low, initAngleFromHorizontal.high);
            force.x = force.x * Mathf.Cos(phi);
            force.y = force.y * Mathf.Cos(phi);
            force.z = magnitude * Mathf.Sin(phi);
            bitPhys.Set3Motion(force);
        }
        // Debug.Break();
    }
    public static bool DamageTypeMatch(damageType dam1, damageType dam2) {
        if (dam1 == dam2)
            return true;
        if (dam1 == damageType.any || dam2 == damageType.any)
            return true;
        if (dam1 == damageType.physical && (dam2 == damageType.cutting || dam2 == damageType.physical || dam2 == damageType.piercing))
            return true;
        if (dam2 == damageType.physical && (dam1 == damageType.cutting || dam1 == damageType.physical || dam1 == damageType.piercing))
            return true;
        return false;
    }
    public void CopyFrom(Gibs other) {
        damageCondition = other.damageCondition;
        number = other.number;
        particle = other.particle;
        // forceMin = other.forceMin;
        // forceMax = other.forceMax;
        color = other.color;
        applyAnimationSkinColor = other.applyAnimationSkinColor;
    }
}
