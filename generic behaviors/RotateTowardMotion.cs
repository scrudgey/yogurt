using UnityEngine;

public class RotateTowardMotion : MonoBehaviour {
    Vector3 tempRotate = Vector3.zero;
    public float angleOffset = 90f;
    public Rigidbody2D body;
    public bool disableOnGroundMode;
    void Awake() {
        body = GetComponent<Rigidbody2D>();
    }
    void Update() {
        if (body == null)
            return;
        if (body.velocity.magnitude < 0.1f)
            return;
        tempRotate.z = Toolbox.Instance.ProperAngle(body.velocity.x, body.velocity.y) + angleOffset;
        transform.rotation = Quaternion.identity;
        transform.Rotate(tempRotate);
    }

    public void GroundModeStart() {
        if (disableOnGroundMode)
            this.enabled = true;
    }
    public void FlyModeStart() {
        if (disableOnGroundMode)
            this.enabled = false;
    }
}
