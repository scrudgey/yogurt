using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Controller : Singleton<Controller> {

	public Controllable focus;
	private GameObject lastLeftClicked;
	private List<string> forbiddenColliders = new List<string> {"fire", "sightcone", "table", "background"};

	public string message ="smoke weed every day";
	
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
		if (focus != null){
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
	}

	void OnGUI(){

		// the mousedown click events need to be a bit better- referencing fire in particular is clunky
		Event e = Event.current;
		if (e.isMouse){
			
			// right click
			if (Input.GetMouseButtonDown(1)){
				//detect if we clicked anything
				RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
				foreach (RaycastHit2D hit in hits){
					if (hit.collider != null && !forbiddenColliders.Contains(hit.collider.tag)){
						focus.lastRightClicked = hit.collider.gameObject;
					}
				}
			}
			// left click
			if (Input.GetMouseButtonDown(0)){
				//detect if we clicked anything
				if (!EventSystem.current.IsPointerOverGameObject())
				{
					RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
					foreach (RaycastHit2D hit in hits){
						if (hit.collider != null && !forbiddenColliders.Contains(hit.collider.tag)){
							focus.lastLeftClicked = hit.collider.gameObject;
							lastLeftClicked = hit.collider.gameObject;
							UINew.Instance.Clicked(lastLeftClicked);
						}
					}
				}
			}
			
			// mouse up event
			if (Input.GetMouseButtonUp(1)){
				focus.MouseUp();
			}
		}
		
	}

	public bool InteractionIsWithinRange(Interaction i){

		if (i == null || lastLeftClicked == null){
			return false;
		}

		if (i.limitless)
			return true;

		float dist = Vector3.Distance(lastLeftClicked.transform.position,focus.transform.position);

		if (dist < i.range){
			return true;
		} else {
			return false;
		}

	}

}
