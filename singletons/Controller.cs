using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class Controller : Singleton<Controller> {
	private Controllable _focus;
	public Controllable focus{
		get{
			return _focus;
		}
		set{
			_focus = value;
			focusHurtable = _focus.GetComponent<Hurtable>();
		}
	}
	public Hurtable focusHurtable;
    private bool _suspendInput = false;
    public bool suspendInput{
        get{ 
            return _suspendInput; 
            }
        set{ 
            _suspendInput = value;
            if (focus)
				focus.ResetInput();
        }
    }
	private GameObject lastLeftClicked;
	public List<string> forbiddenColliders = new List<string> {"fire", "sightcone", "table", "background", "occurrenceFlag"};
	public string message = "smoke weed every day";
    public enum SelectType{
        none, swearAt
    }
    public SelectType currentSelect = SelectType.none;
	void Update () {
		if (focus != null & !suspendInput){
			focus.ResetInput();
			if( Input.GetAxis("Vertical") > 0 )
				focus.upFlag = true;
			if(Input.GetAxis("Vertical") < 0)
				focus.downFlag = true;
			if(Input.GetAxis("Horizontal") < 0)
				focus.leftFlag = true;
			if(Input.GetAxis("Horizontal") > 0)
				focus.rightFlag = true;
			//Fire key 
			if(Input.GetButtonDown("Fire1")){
				focus.shootPressedFlag = true;
			}
			if(Input.GetButton("Fire1")){
				focus.shootHeldFlag = true;
			}
		}
		if (Input.GetButtonDown("Cancel")){
			if (!GameManager.Instance.InCutscene()){
				UINew.Instance.ShowMenu(UINew.MenuType.escape);
			} else {
				CutsceneManager.Instance.EscapePressed();
			}
		}
	}
	void OnGUI(){
		if (Event.current.isMouse){
			// right click
			if (Input.GetMouseButtonDown(1)){
				RightClick();
			}
			// left click
			if (Input.GetMouseButtonDown(0)){
				LeftClick();
			}
			// mouse up event
			// if (Input.GetMouseButtonUp(1)){
			// 	focus.MouseUp();
			// }
		}
	}
    void RightClick(){
        //detect if we clicked anything
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        foreach (RaycastHit2D hit in hits){
            if (hit.collider != null && !forbiddenColliders.Contains(hit.collider.tag)){
                focus.lastRightClicked = hit.collider.gameObject;
            }
        }
    }
	GameObject GetBaseInteractive(Transform target){
		Transform currentChild = target;
		while (true){
			if (currentChild.name.Substring(Mathf.Max(0, currentChild.name.Length-6)) == "Ground"){
				// Debug.Log("found ground, returning "+currentChild.gameObject.name);
				return currentChild.gameObject;
			}
			if (currentChild.parent == null){
				// Debug.Log("no parent, returning "+currentChild.gameObject.name);
				return currentChild.gameObject;
			}
			currentChild = currentChild.parent;
		}
	}
    void LeftClick(){
		if (focus.hitState >= Controllable.HitState.stun)
			return;
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero).OrderBy(h=>h.collider.gameObject.name).ToArray();
        // IsPointerOverGameObject is required here to exclude clicks if we are hovering over a UI element.
        // this may or may not cause problems down the road, but I'm unsure how else to do this.
        // TODO: currently unresolved. UI overlapping objects in world creates problem clicks.
        
        if (currentSelect == SelectType.none && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()){
        // if (currentSelect == SelectType.none){
            foreach (RaycastHit2D hit in hits){
                if (hit.collider != null && !forbiddenColliders.Contains(hit.collider.tag)){
                    focus.lastLeftClicked = hit.collider.gameObject;
					// we need to check the object from the root for interactions, but place buttons at the child clicked on
					Clicked(GetBaseInteractive(hit.collider.transform), hit.collider.transform.gameObject);
					break;
                }
            }
        }
        if (currentSelect == SelectType.swearAt){
            currentSelect = SelectType.none;
            foreach (RaycastHit2D hit in hits){
                if (hit.collider != null && !forbiddenColliders.Contains(hit.collider.tag)){
					MessageSpeech message = new MessageSpeech();
					message.swearTarget = hit.collider.gameObject;
					Toolbox.Instance.SendMessage(focus.gameObject, this, message);
                }
            }
        }
    }

    public void Clicked(GameObject clicked, GameObject clickSite){
		if (lastLeftClicked == clicked){
			// TODO: fix this conditional!
			UINew.Instance.ClearWorldButtons();
			lastLeftClicked = null;
		} else {
			lastLeftClicked = clicked;
			Inventory inventory = focus.GetComponent<Inventory>();
			if (clicked.transform.IsChildOf(focus.transform) || clicked == focus.gameObject){
				if (inventory)
					if (inventory.holding)
						UINew.Instance.DisplayHandActions(inventory);
			} else {
				UINew.Instance.SetClickedActions(lastLeftClicked, clickSite);
			}
		}
	}

	public void ResetLastLeftClicked(){
		lastLeftClicked = null;
	}

	public bool InteractionIsWithinRange(Interaction i){
		// TODO: this all should use something other than lastleftclicked, for persistent buttons of sorts.
		// using i.action.parent doesn't work, because some actions are sourced from player gameobject, not "target", whatever it is
		if (i == null || lastLeftClicked == null)
			return false;
		if (i.limitless)
			return true;
		float dist = Vector3.SqrMagnitude(lastLeftClicked.transform.position - focus.transform.position);
		if (dist < i.range){
			return true;
		} else {
			return false;
		}
	}
}
