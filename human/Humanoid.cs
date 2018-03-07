using UnityEngine;

public class Humanoid : SimpleControl, IMessagable {
    Transform cachedTransform;
    public new Transform transform {
        get {
            if (cachedTransform == null) {
                cachedTransform = gameObject.GetComponent<Transform>();
            }
            return cachedTransform;
        }
    }
    public float runTilt;
    private Quaternion rightTilt;
    private Quaternion leftTilt;
    private Quaternion forward;
    public override void Awake() {
        base.Awake();
        // this messy nonsense is just to fix the quaternions that indicate
        // direction tilt when running. there's probably a nicer way to do
        // this but I was learning unity's vectors / rotations / quaternions / 2D
        // at this time.
        Vector2 leftTiltVector = Vector2.zero;
        Vector2 rightTiltVector = Vector2.zero;

        leftTiltVector.x = Mathf.Cos(1.57f - (runTilt * 6.28f / 360f));
        leftTiltVector.y = Mathf.Sin(1.57f - (runTilt * 6.28f / 360f));

        rightTiltVector.x = Mathf.Cos((runTilt * 6.28f / 360f) + 1.57f);
        rightTiltVector.y = Mathf.Sin((runTilt * 6.28f / 360f) + 1.57f);

        rightTilt = Quaternion.LookRotation(Vector3.forward, rightTiltVector);
        leftTilt = Quaternion.LookRotation(Vector3.forward, leftTiltVector);
        forward = Quaternion.LookRotation(Vector3.forward, -1 * Vector3.forward);
    }
    public override void FixedUpdate() {
        if (hitState > Controllable.HitState.none) {
            rigidBody2D.drag = 10f;
            ResetInput();
            return;
        } else {
            rigidBody2D.drag = 1f;
        }
        base.FixedUpdate();
        if (leftFlag) {
            transform.rotation = Quaternion.Lerp(transform.rotation, leftTilt, 0.1f);
        }
        if (rightFlag) {
            transform.rotation = Quaternion.Lerp(transform.rotation, rightTilt, 0.1f);
        }
        if (!rightFlag && !leftFlag) {
            transform.rotation = Quaternion.Lerp(transform.rotation, forward, 0.1f);
        }
    }
    public override void ReceiveMessage(Message message) {
        base.ReceiveMessage(message);
        if (message is MessageInventoryChanged) {
            Inventory inv = (Inventory)message.messenger;
            MessageInventoryChanged invMessage = (MessageInventoryChanged)message;
            if (inv.holding) {
                if (fightMode)
                    ToggleFightMode();
                MonoBehaviour[] list = inv.holding.gameObject.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour mb in list) {
                    if (mb is IDirectable) {
                        IDirectable idir = (IDirectable)mb;
                        directables.Add(idir);
                        idir.DirectionChange(direction);
                    }
                }
            }
            if (invMessage.dropped) {
                MonoBehaviour[] list = invMessage.dropped.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour mb in list) {
                    if (mb is IDirectable) {
                        directables.Remove((IDirectable)mb);
                    }
                }
            }
        }
    }
    public override void ToggleFightMode() {
        base.ToggleFightMode();
        if (fightMode) {
            MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.fighting, true);
            Toolbox.Instance.SendMessage(gameObject, this, anim);
            if (GameManager.Instance.playerObject == gameObject)
                UINew.Instance.ShowPunchButton();
        } else {
            MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.fighting, false);
            Toolbox.Instance.SendMessage(gameObject, this, anim);
            if (GameManager.Instance.playerObject == gameObject)
                UINew.Instance.HidePunchButton();
        }
    }
}
