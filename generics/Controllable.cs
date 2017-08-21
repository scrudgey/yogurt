using UnityEngine;
using System.Collections.Generic;

public class Controllable : MonoBehaviour, IMessagable {
	public enum HitState{none, stun, unconscious, dead};
	static public HitState AddHitState(HitState orig, HitState argument){
		if (argument > orig){
			return argument;
		}
		return orig;
	}
	static public HitState RemoveHitState(HitState orig, HitState argument){
		if (argument >= orig){
			return HitState.none;
		}
		return orig;
	}
	private bool _upFlag;
	private bool _downFlag;
	private bool _leftFlag;
	private bool _rightFlag;
	private bool _shootPressedFlag;
	public bool upFlag{
		get {if (disabled) return false; else return _upFlag;}
		set {_upFlag = value;}
	}
	public bool downFlag{
		get {if (disabled) return false; else return _downFlag;}
		set {_downFlag = value;}
	}
	public bool leftFlag{
		get {if (disabled) return false; else return _leftFlag;}
		set {_leftFlag = value;}
	}
	public bool rightFlag{
		get {if (disabled) return false; else return _rightFlag;}
		set {_rightFlag = value;}
	}
	public bool shootPressedFlag{
		get {if (disabled) return false; else return _shootPressedFlag;}
		set {_shootPressedFlag = value;}
	}
	private bool shootPressedDone;
	public bool shootHeldFlag;
	public string lastPressed = "right";
	private Vector2 _direction = Vector2.right;
	public Vector2 direction{
		get {return _direction;}
		set {
			_direction = value;
			directionAngle = Toolbox.Instance.ProperAngle(_direction.x, _direction.y);
			foreach (IDirectable directable in directables){
				directable.DirectionChange(value);
			}
		}
	}
	public float directionAngle = 0;
	public List<IDirectable> directables = new List<IDirectable>();
	public GameObject lastLeftClicked;
	public Interaction defaultInteraction;
	public bool fightMode;
	public bool disabled = false;
	public HitState hitState;
	public void ResetInput(){
		upFlag = false;
		downFlag = false;
		leftFlag = false;
		rightFlag = false;
		shootPressedFlag = false;
		shootHeldFlag = false;
	}
	public GameObject lastRightClicked;
	public virtual void Start(){
		foreach(Component component in gameObject.GetComponentsInChildren<Component>())
		{
			if (component is IDirectable){
				directables.Add((IDirectable)component);
			}
		}
	}
	void Update(){
		if (hitState > 0){
			ResetInput();
		}
		if (hitState > Controllable.HitState.none){
			ResetInput();
			// return;
		}
		if (rightFlag || leftFlag)
			lastPressed = "right";
		if (downFlag)
			lastPressed = "down";
		if (upFlag)
			lastPressed = "up";
		// update direction vector if speed is above a certain value
		if(GetComponent<Rigidbody2D>().velocity.normalized.magnitude > 0.1){// && (upFlag || downFlag || leftFlag || rightFlag) ){
			SetDirection(GetComponent<Rigidbody2D>().velocity.normalized);
			directionAngle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
		}
		if (!shootPressedFlag && shootPressedDone)
			shootPressedDone = false;
		if (shootPressedFlag){
			ShootPressed();
			shootPressedDone = true;
		}
		if (shootHeldFlag){
			ShootHeld();
		}
	}
	public virtual void SetDirection(Vector2 d){
		direction = d;
	}
	public void ShootPressed(){
		if (fightMode){
			Toolbox.Instance.SendMessage(gameObject, this, new MessagePunch());
		}
		if (defaultInteraction != null)
			defaultInteraction.DoAction();
	}
	public void ShootHeld(){
		if (defaultInteraction != null){
			if (defaultInteraction.continuous)	
				defaultInteraction.DoAction();
		}
	}
	public void ToggleFightMode(){
		fightMode = !fightMode;
		if (fightMode){
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
	public virtual void ReceiveMessage(Message incoming){
		if (incoming is MessageDamage){
			MessageDamage message = (MessageDamage)incoming;
			direction = -1f * message.force;
		}
		if (incoming is MessageHitstun){
			MessageHitstun hits = (MessageHitstun)incoming;
			hitState = hits.hitState;
		}
		if (incoming is MessageInventoryChanged){
			Inventory inv = (Inventory)incoming.messenger;
			MessageInventoryChanged invMessage = (MessageInventoryChanged)incoming;
			if (inv.holding){
				if (fightMode)
					ToggleFightMode();
				MonoBehaviour[] list = inv.holding.gameObject.GetComponents<MonoBehaviour>();
				foreach(MonoBehaviour mb in list)
				{
					if (mb is IDirectable){
						IDirectable idir = (IDirectable)mb;
						directables.Add(idir);
						idir.DirectionChange(direction);
					}
				}
			}
			if (invMessage.dropped){
				MonoBehaviour[] list = invMessage.dropped.GetComponents<MonoBehaviour>();
				foreach(MonoBehaviour mb in list)
				{
					if (mb is IDirectable){
						directables.Remove((IDirectable)mb);
					}
				}
			}
			UpdateActions(inv);
		}
		if (incoming is MessageDirectable){
			MessageDirectable message = (MessageDirectable)incoming;
			if (message.addDirectable != null)
				directables.Add(message.addDirectable);
			if (message.removeDirectable != null)
				directables.Remove(message.removeDirectable);
		}
	}
	public void UpdateActions(){
		Inventory inv = GetComponent<Inventory>();
		if (inv != null)
			UpdateActions(inv);
	}
	public void UpdateActions(Inventory inv){
		if (inv == null)
			return;
		if (inv.holding){
			List<Interaction> manualActions = Interactor.ReportManualActions(inv.holding.gameObject, gameObject);
			defaultInteraction = Interactor.GetDefaultAction(manualActions);
			if (Controller.Instance.focus == this)
				UINew.Instance.CreateActionButtons(manualActions, defaultInteraction);
		} else {
			defaultInteraction = null;
			if (Controller.Instance.focus == this){
				UINew.Instance.ClearActionButtons();
				UINew.Instance.ClearWorldButtons();
			}
		}
	}
}
