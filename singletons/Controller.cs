using UnityEngine;
// using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Controller : Singleton<Controller> {

	public Controllable focus;
    private bool _suspendInput = false;
    public bool suspendInput{
        get{ 
            return _suspendInput; 
            }
        set{ 
            _suspendInput = value;
            if (focus)
                ResetInput(focus);
        }
    }
	private GameObject lastLeftClicked;
	private List<string> forbiddenColliders = new List<string> {"fire", "sightcone", "table", "background"};
	public string message ="smoke weed every day";
    public enum SelectType{
        none, swearAt
    }
    public SelectType currentSelect = SelectType.none;
    
	static public void ResetInput(Controllable control){
		control.upFlag = false;
		control.downFlag = false;
		control.leftFlag = false;
		control.rightFlag = false;
		control.shootPressedFlag = false;
		control.shootHeldFlag = false;
	}

	// Update is called once per frame
	void Update () {
		if (focus != null & !suspendInput){
			ResetInput(focus);
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
				UINew.Instance.ShootPressed();
			}
			if(Input.GetButton("Fire1")){
				focus.shootHeldFlag = true;
				UINew.Instance.ShootHeld();
			}
		}
		
		if (Input.GetButtonDown("Cancel")){
			UINew.Instance.PauseMenu();
		}
	}

	void OnGUI(){
		// the mousedown click events need to be a bit better- referencing fire in particular is clunky
		Event e = Event.current;
		if (e.isMouse){
			// right click
			if (Input.GetMouseButtonDown(1)){
				RightClick();
			}
			// left click
			if (Input.GetMouseButtonDown(0)){
				LeftClick();
			}
			// mouse up event
			if (Input.GetMouseButtonUp(1)){
				focus.MouseUp();
			}
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
    void LeftClick(){
        //detect if we clicked anything
        if (EventSystem.current.IsPointerOverGameObject())
            return;
            
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (currentSelect == SelectType.none){
            foreach (RaycastHit2D hit in hits){
                if (hit.collider != null && !forbiddenColliders.Contains(hit.collider.tag)){
                    focus.lastLeftClicked = hit.collider.gameObject;
                    lastLeftClicked = hit.collider.gameObject;
                    UINew.Instance.Clicked(lastLeftClicked);
                }
            }
        }
        if (currentSelect == SelectType.swearAt){
            currentSelect = SelectType.none;
            foreach (RaycastHit2D hit in hits){
                if (hit.collider != null && !forbiddenColliders.Contains(hit.collider.tag)){
                    Swear(hit.collider.gameObject);
                }
            }
        }
    }

	public bool InteractionIsWithinRange(Interaction i){
		if (i == null || lastLeftClicked == null){
			return false;
		}

		if (i.limitless)
			return true;

		float dist = Vector3.Distance(lastLeftClicked.transform.position, focus.transform.position);

		if (dist < i.range){
			return true;
		} else {
			return false;
		}
	}
    
    public void Swear(GameObject target=null){
        Speech speech = focus.GetComponent<Speech>();
        if (speech){
            speech.Swear(target);
        }
    }
    
    public void SayRandom(){
        Speech speech = focus.GetComponent<Speech>();
        if (speech){
            speech.SayRandom();
        }
    }
    
    public void SayLine(){
        Speech speech = focus.GetComponent<Speech>();
        if (!speech)
            return;
            
        ScriptDirector director = FindObjectOfType<ScriptDirector>();
        if (!director){
            speech.Say("What's my line?"); 
        } else {
            speech.Say(director.NextTomLine());
        }
    }

}
