using UnityEngine;

public class Humanoid : Controllable, IMessagable {
	Transform cachedTransform;
	public new Transform transform
		{
			get
			{
				if( cachedTransform == null )
				{
					cachedTransform = gameObject.GetComponent<Transform>();
				}
				return cachedTransform;
			}
	}
	private float baseSpeed;
	public float maxSpeed;
	public float maxAcceleration;
	public float friction;
	public float runTilt;
	private Quaternion rightTilt;
	private Quaternion leftTilt;
	private Quaternion forward;
	// private float initFriction;
	private Rigidbody2D rigidBody2D;
	private Vector3 scaleVector{
		get {
			return _scaleVector;
		}
		set {
			if (value != _scaleVector){
				transform.localScale = value;
			}
			_scaleVector = value;
		}
	}
	private Vector3 _scaleVector;
	private bool LoadInitialized = false;
	public override void Start () {
		base.Start();
		if (!LoadInitialized)
			LoadInit();
		// this messy nonsense is just to fix the quaternions that indicate
		// direction tilt when running. there's probably a nicer way to do
		// this but I was learning unity's vectors / rotations / quaternions / 2D
		// at this time.
		Vector2 leftTiltVector = Vector2.zero;
		Vector2 rightTiltVector = Vector2.zero;
		
		leftTiltVector.x = Mathf.Cos( 1.57f - (runTilt * 6.28f / 360f));
		leftTiltVector.y = Mathf.Sin( 1.57f - (runTilt * 6.28f / 360f));
		
		rightTiltVector.x = Mathf.Cos((runTilt * 6.28f / 360f) + 1.57f);
		rightTiltVector.y = Mathf.Sin((runTilt * 6.28f / 360f) + 1.57f);
		
		rightTilt = Quaternion.LookRotation(Vector3.forward, rightTiltVector);
		leftTilt = Quaternion.LookRotation(Vector3.forward, leftTiltVector);
		forward = Quaternion.LookRotation(Vector3.forward, -1 * Vector3.forward);
	}
	void LoadInit(){
		baseSpeed = maxSpeed;
		rigidBody2D = GetComponent<Rigidbody2D>();
	}
	void FixedUpdate(){
		Vector2 acceleration = Vector2.zero;
		Vector2 deceleration = Vector2.zero;
		if (hitState > Controllable.HitState.none){
			rigidBody2D.drag = 10f;
			ResetInput();
			return;
		}
		// rigidBody2D.drag = initFriction;
		// Do the normal controls stuff
		// set vertical force or damp if neither up nor down is held
		if (upFlag)
			acceleration.y = maxAcceleration;
		if (downFlag)
			acceleration.y = -1 * maxAcceleration;
		if (!upFlag && !downFlag){
			deceleration.y = -1 * friction * GetComponent<Rigidbody2D>().velocity.y;
		}
		// set horizontal force, or damp is neither left nor right held
		if (leftFlag){
			acceleration.x = -1 * maxAcceleration;
			transform.rotation = Quaternion.Lerp (transform.rotation, leftTilt, 0.1f);
		}
		if (rightFlag){
			acceleration.x = maxAcceleration;
			transform.rotation = Quaternion.Lerp (transform.rotation, rightTilt, 0.1f);
		}
		if (!rightFlag && !leftFlag){
			deceleration.x = -1 * friction * GetComponent<Rigidbody2D>().velocity.x;
			transform.rotation = Quaternion.Lerp(transform.rotation, forward, 0.1f);
		}
		// apply force
		rigidBody2D.AddForce(acceleration+deceleration);

		// clamp velocity to maximum
		// there's probably a more efficient way to do this calculation but whatevs
		if (rigidBody2D.velocity.magnitude > maxSpeed)
			rigidBody2D.velocity = Vector2.ClampMagnitude(rigidBody2D.velocity, maxSpeed);

		// use the scale x trick for left-facing animations
		Vector2 vel = rigidBody2D.velocity;
		if (vel.x < -0.1){
			Vector3 tempVector = Vector3.one;
			tempVector.x = -1;
			scaleVector = tempVector;
		}
		if (vel.x > 0.1){
			Vector3 tempVector = Vector3.one;
			tempVector.x = 1;
			scaleVector = tempVector;
		}
	}
	public override void ReceiveMessage(Message message){
		if (!LoadInitialized)
			LoadInit();
		base.ReceiveMessage(message);
		if (message is MessageNetIntrinsic){
			MessageNetIntrinsic intrins = (MessageNetIntrinsic)message;
			if (intrins.netIntrinsic.buffs.ContainsKey(BuffType.speed)){
				maxSpeed = baseSpeed + intrins.netIntrinsic.buffs[BuffType.speed].floatValue;
			}
		}
	}
	public void UpdateDirection(){
		float angle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
		// change lastpressed because this is relevant to animation
		if (angle > 315 || angle < 45){
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
	public override void SetDirection(Vector2 d){
		base.SetDirection(d);
		UpdateDirection();
	}
}
