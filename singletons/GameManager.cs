using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager> {

	protected GameManager(){}

	public string message ="smoke weed every day";	
	private CameraControl cameraControl;
	private Camera cam;
	public GameObject playerObject;
	public string lastSavedPlayerPath;
	private int entryID;

	private bool _telepathyOn;
	public bool telepathyOn{
		get {return _telepathyOn;}
		set {
			_telepathyOn = value;
			TelepathyMask();
		}
	}

	public void TelepathyMask(){
		if (telepathyOn){
			playerObject.SendMessage("Say","I can hear thoughts!",SendMessageOptions.DontRequireReceiver);
			cam.cullingMask |= 1 << LayerMask.NameToLayer("thoughts");
		} else {
			try {
				cam.cullingMask &=  ~(1 << LayerMask.NameToLayer("thoughts"));
			} catch {
				Debug.Log(cam);
				Debug.Log("Weird telepathy culling mask issue");
			}
		}
	}

	public void SetFocus(GameObject target){
		playerObject = target;

		Controller.Instance.focus = target.GetComponent<Controllable>();
		cameraControl = FindObjectOfType<CameraControl>();
		if (cameraControl)
			cameraControl.focus = target;
		UISystem.Instance.inventory = target.GetComponent<Inventory>();
	}

	void Start(){
		// this right here is the kludge
		playerObject = GameObject.Find ("Tom");		
		cam = GameObject.FindObjectOfType<Camera>();
		SetFocus(playerObject);

		lastSavedPlayerPath = Application.persistentDataPath+"/player_"+GameManager.Instance.playerObject.name+"_state.xml";

		MySaver.CleanupSaves();
		MySaver.OnPostLoad += PostLoad;
	}

	void PostLoad(){
		// this is more of a kludge
		playerObject = GameObject.Find ("Tom(Clone)");	
		cam = GameObject.FindObjectOfType<Camera>();
		if (playerObject){
			SetFocus(playerObject);
		}
		
	}

	public void LeaveScene(string toSceneName,int toEntryNumber){
		// call mysaver, tell it to save scene and player separately
		MySaver.Save();
		// load new scene
		entryID = toEntryNumber;
		Application.LoadLevel(toSceneName);
	}
	void OnLevelWasLoaded(int level) {
		// call scene load routine & load player
		MySaver.LoadScene();
		PlayerEnter();
	}
	void PlayerEnter(){
		if (playerObject){
			List<Doorway> doorways = new List<Doorway>( GameObject.FindObjectsOfType<Doorway>() );
			foreach (Doorway doorway in doorways){
				if (doorway.entryID == entryID){
					Vector3 tempPos = doorway.transform.position;
					tempPos.y = tempPos.y - 0.05f;
					playerObject.transform.position = tempPos;
				}
			}
		}
	}

}

