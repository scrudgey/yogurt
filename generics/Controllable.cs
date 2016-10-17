using UnityEngine;
using System.Collections.Generic;

public class Controllable : MonoBehaviour, IMessagable {
	public bool upFlag;
	public bool downFlag;
	public bool rightFlag;
	public bool leftFlag;
	[HideInInspector] 
	public bool shootPressedFlag;
	private bool shootPressedDone;
	public bool shootHeldFlag;
	public string lastPressed = "right";
	private Vector2 _direction = Vector2.right;
	public Vector2 direction{
		get {return _direction;}
		set {
			_direction = value;
			foreach (IDirectable directable in directables){
				directable.DirectionChange(value);
			}
		}
	}
	public float directionAngle = 0;
	public delegate void ClickAction();
	public event ClickAction OnLastRightClickedChange;
	public event ClickAction OnMouseUpEvent;
	public event ClickAction OnLastLeftClickedChange;
	public List<IDirectable> directables = new List<IDirectable>();
	private GameObject _lastLeftClicked;
	public Interaction defaultInteraction;
	public bool fightMode;
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
	public virtual void Start(){
		foreach(Component component in gameObject.GetComponents<Component>())
		{
			if (component is IDirectable){
				directables.Add((IDirectable)component);
			}
		}
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
			// SetDirection(-1f * dam.force);
			direction = -1f * message.force;
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
						directables.Add((IDirectable)mb);
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
	}

	public void UpdateActions(Inventory inv){
		if (inv.holding){
			List<Interaction> manualActions = Interactor.ReportManualActions(inv.holding.gameObject, gameObject);
			foreach (Interaction inter in Interactor.ReportRightClickActions(gameObject, inv.holding.gameObject))
				if (!manualActions.Contains(inter))   // inverse double-count diode
					manualActions.Add(inter);
			foreach (Interaction inter in Interactor.ReportFreeActions(inv.holding.gameObject))
				if (!manualActions.Contains(inter))
					manualActions.Add(inter);
			defaultInteraction = Interactor.GetDefaultAction(manualActions);
			if (Controller.Instance.focus == this)
				UINew.Instance.CreateActionButtons(manualActions, defaultInteraction);
		} else {
			defaultInteraction = null;
			if (Controller.Instance.focus == this)
				UINew.Instance.ClearActionButtons();
		}
	}
}
