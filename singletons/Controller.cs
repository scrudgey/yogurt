using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Controller : Singleton<Controller> {

	public Controllable focus;
	private GameObject lastLeftClicked;

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
			if(Input.GetButtonDown("Fire1"))
				focus.shootPressedFlag = true;

			if(Input.GetButton("Fire1"))
				focus.shootHeldFlag = true;

		}
	}

	void OnGUI(){

		// the mousedown click events need to be a bit better- referencing fire in particular is clunky
		Event e = Event.current;
		if (e.isMouse){
			
			// right click
			if (Input.GetMouseButtonDown(1)){
				//detect if we clicked anything
				RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition),Vector2.zero);
				foreach (RaycastHit2D hit in hits){
					if (hit.collider != null && hit.transform.tag != "fire" && hit.collider.tag != "sightcone" ){
						focus.lastRightClicked = hit.collider.gameObject;
					}

				}
			}

			// left click
			if (Input.GetMouseButtonDown(0)){
				//detect if we clicked anything
				if (!EventSystem.current.IsPointerOverGameObject())
				{
					RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition),Vector2.zero);
					foreach (RaycastHit2D hit in hits){
						if (hit.collider != null && hit.collider.tag != "fire" && hit.collider.tag != "sightcone" ){
							focus.lastLeftClicked = hit.collider.gameObject;
							lastLeftClicked = hit.collider.gameObject;
							UISystem.Instance.actions = Interactor.Instance.GetInteractions(focus.gameObject,lastLeftClicked);
							UISystem.Instance.targetName = lastLeftClicked.name;
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
