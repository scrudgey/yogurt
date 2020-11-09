using UnityEngine;

public class SimpleControl : Controllable, ISaveable {
    protected float baseSpeed;
    public float maxSpeed;
    public float panicSpeed;
    public float sprintSpeed;
    public float maxAcceleration;
    public float friction;
    private Vector3 _scaleVector;
    private Vector3 scaleVector {
        get {
            return _scaleVector;
        }
        set {
            if (value != _scaleVector) {
                transform.localScale = scaleFactor * value;
            }
            _scaleVector = value;
        }
    }
    public float scaleFactor = 1f;
    public override void Awake() {
        base.Awake();
        baseSpeed = maxSpeed;
        // myRigidBody = GetComponent<Rigidbody2D>();
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsic);
        Toolbox.RegisterMessageCallback<MessageAnimation>(this, HandleAnimation);
    }
    void HandleNetIntrinsic(MessageNetIntrinsic message) {
        maxSpeed = baseSpeed + message.netBuffs[BuffType.speed].floatValue;
    }
    void HandleAnimation(MessageAnimation animationMessage) {
        if (animationMessage.type == MessageAnimation.AnimType.panic) {
            if (animationMessage.value) {
                panicSpeed = 0.2f;
            } else {
                panicSpeed = 0;
            }
        } else {

        }
    }
    public virtual void FixedUpdate() {
        Vector2 acceleration = Vector2.zero;
        Vector2 deceleration = Vector2.zero;
        if (hitState > Controllable.HitState.none) {
            myRigidBody.drag = 10f;
            ResetInput();
            return;
        } else {
            myRigidBody.drag = 1f;
        }
        if (running) {
            sprintSpeed = 0.2f;
        } else {
            sprintSpeed = 0f;
        }
        // Do the normal controls stuff
        // set vertical force or damp if neither up nor down is held
        if (upFlag) {
            if (running) {
                acceleration.y = maxAcceleration * 2f;
            } else {
                acceleration.y = maxAcceleration;
            }
        }
        if (downFlag) {
            if (running) {
                acceleration.y = -2 * maxAcceleration;
            } else {
                acceleration.y = -1 * maxAcceleration;
            }
        }
        if (!upFlag && !downFlag) {
            deceleration.y = -1 * friction * GetComponent<Rigidbody2D>().velocity.y;
        }
        // set horizontal force, or damp is neither left nor right held
        if (leftFlag) {
            if (running) {
                acceleration.x = -2 * maxAcceleration;
            } else {
                acceleration.x = -1 * maxAcceleration;
            }
        }
        if (rightFlag) {
            if (running) {
                acceleration.x = 2f * maxAcceleration;
            } else {
                acceleration.x = maxAcceleration;
            }
        }
        if (!rightFlag && !leftFlag) {
            deceleration.x = -1 * friction * GetComponent<Rigidbody2D>().velocity.x;
        }
        // apply force
        myRigidBody.AddForce(acceleration + deceleration);
        // clamp velocity to maximum
        // there's probably a more efficient way to do this calculation but whatevs
        float bonusSpeed = Mathf.Max(panicSpeed, sprintSpeed);
        if (myRigidBody.velocity.magnitude > (maxSpeed + bonusSpeed))
            myRigidBody.velocity = Vector2.ClampMagnitude(myRigidBody.velocity, (maxSpeed + bonusSpeed));
        // use the scale x trick for left-facing animations
        Vector2 vel = myRigidBody.velocity;
        if (vel.x < -0.1) {
            Vector3 tempVector = Vector3.one;
            tempVector.x = -1;
            scaleVector = tempVector;
        }
        if (vel.x > 0.1) {
            Vector3 tempVector = Vector3.one;
            tempVector.x = 1;
            scaleVector = tempVector;
        }
    }

    public void SaveData(PersistentComponent data) {
        data.strings["lastPressed"] = lastPressed;
        data.vectors["direction"] = direction;
        data.bools["fightMode"] = fightMode;
        data.bools["disabled"] = disabled;
        data.ints["hitstate"] = (int)hitState;
    }
    public void LoadData(PersistentComponent data) {
        lastPressed = data.strings["lastPressed"];
        SetDirection(data.vectors["direction"]);
        disabled = data.bools["disabled"];
        hitState = (Controllable.HitState)data.ints["hitstate"];
        if (data.bools["fightMode"] && !fightMode) {
            ToggleFightMode();
        }
    }
    public void UpdateDirection() {
        float angle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
        // change lastpressed because this is relevant to animation
        if (angle > 315 || angle < 45) {
            lastPressed = "right";
            Vector3 tempVector = Vector3.one;
            tempVector.x = 1;
            scaleVector = tempVector;
        } else if (angle >= 45 && angle <= 135) {
            lastPressed = "up";
        } else if (angle >= 135 && angle < 225) {
            lastPressed = "right";
            Vector3 tempVector = Vector3.one;
            tempVector.x = -1;
            scaleVector = tempVector;
        } else if (angle >= 225 && angle < 315) {
            lastPressed = "down";
        }
    }
    protected override void SetDirection(Vector2 d) {
        base.SetDirection(d);
        UpdateDirection();
    }
}
