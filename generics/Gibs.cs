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
    public void Emit(MessageDamage message) {
        if (!DamageTypeMatch(damageCondition, message.type))
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
            // TODO: this is to be fixed
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
            // TODO: allow strong impacts to affect
            // emit in the direction between point of impact and center of mass
            // if we both have physical, calculate z direction as well

            // float magnitude = Random.Range(initVelocity.low, initVelocity.high);
            // 
            // float magnitude = Random.Range(baseDir.magnitude * initVelocity.low, baseDir.magnitude * initVelocity.high);
            // Vector3 force = magnitude * Toolbox.Instance.RandomVector(baseDir.normalized, 45f);
            // Debug.Log(message.force);
            Vector3 force = message.force * Random.Range(initVelocity.low, initVelocity.high) / 12f;
            if (message.strength)
                force *= 2f;
            if (message.angleAboveHorizontal != 0) {
                initAngleFromHorizontal.low = message.angleAboveHorizontal;
                initAngleFromHorizontal.high = message.angleAboveHorizontal;
            }
            float phi = Random.Range(initAngleFromHorizontal.low, initAngleFromHorizontal.high);
            force.z = force.magnitude * Mathf.Sin(phi);
            force.x = force.x * Mathf.Cos(phi);
            force.y = force.y * Mathf.Cos(phi);
            bitPhys.Set3Motion(force);
            // Debug.Log(force.x);
            // Debug.Log(force.y);
            // Debug.Log(force.z);
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
        applyAnimationSkinColor = other.applyAnimationSkinColor;
        damageCondition = other.damageCondition;
        number = other.number;
        particle = other.particle;
        color = other.color;
        notPhysical = other.notPhysical;
        initHeight.low = other.initHeight.low;
        initHeight.high = other.initHeight.high;
        initVelocity.low = other.initVelocity.low;
        initVelocity.high = other.initVelocity.high;
        initAngleFromHorizontal.low = other.initAngleFromHorizontal.low;
        initAngleFromHorizontal.high = other.initAngleFromHorizontal.high;
    }
}
