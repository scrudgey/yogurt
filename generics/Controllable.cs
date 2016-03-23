using UnityEngine;
// using System.Collections;

public class Controllable : MonoBehaviour {

//	[HideInInspector] 
	public bool upFlag;
//	[HideInInspector] 
	public bool downFlag;
//	[HideInInspector] 
	public bool rightFlag;
//	[HideInInspector] 
	public bool leftFlag;
	[HideInInspector] 
	public bool shootPressedFlag;
	[HideInInspector] 
	public bool shootHeldFlag;
	[HideInInspector] 
	public string lastPressed = "right";
	public Vector2 direction = Vector2.right;
	public float directionAngle = 0;
	public delegate void ClickAction();
	public event ClickAction OnLastRightClickedChange;
	public event ClickAction OnMouseUpEvent;
	public event ClickAction OnLastLeftClickedChange;
	public IDirectable directable;
	private GameObject _lastLeftClicked;
	public GameObject lastLeftClicked{
		get {return _lastLeftClicked;}
		set{
			_lastLeftClicked = value;
			if (OnLastLeftClickedChange != null)
				OnLastLeftClickedChange();
		}
	}
	private GameObject _lastRightClicked;
	public GameObject lastRightClicked{
		get{return _lastRightClicked;}
		set{
			_lastRightClicked = value;
			if (OnLastRightClickedChange != null)
				OnLastRightClickedChange();
		}
	}
	
	public void MouseUp(){
		if (OnMouseUpEvent != null)
			OnMouseUpEvent();
	}
	void Update(){
		if (rightFlag || leftFlag)
			lastPressed = "right";
		if (downFlag)
			lastPressed = "down";
		if (upFlag)
			lastPressed = "up";

		// update direction vector if speed is above a certain value
		if(GetComponent<Rigidbody2D>().velocity.normalized.magnitude > 0.1 && (upFlag || downFlag || leftFlag || rightFlag) ){
			// directionAngle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
			direction = GetComponent<Rigidbody2D>().velocity.normalized;
			directionAngle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
			if (directable != null)
				directable.DirectionChange(direction);
		}
	}
	
	public void SetDirection(Vector2 d){
		direction = d;
		UpdateDirection();
		Debug.Log(lastPressed);
		if (directable != null)
			directable.DirectionChange(d);
	}
	
	public void UpdateDirection(){
		float angle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
		Vector2 scaleVector = Vector2.one;
		if (angle > 315 || angle < 45){
			lastPressed = "right";
		} else if (angle >= 45 && angle <= 135) {
			lastPressed = "up";
		} else if (angle >= 135 && angle < 225) {
			lastPressed = "right";
			scaleVector.x = -1;
		} else if (angle >= 225 && angle < 315) {
			lastPressed = "down";
		}
		transform.localScale = scaleVector;
	}

}
