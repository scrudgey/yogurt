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
	public string lastPlayerName;
	private int entryID;
	public float gravity = 1.6f;

	public void FocusIntrinsicsChanged(Intrinsic intrinsic){
		if (intrinsic.telepathy.boolValue){
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
		// TODO: add a default player condition here
		playerObject = GameObject.Find ("Tom");		
		if (!playerObject){
			playerObject = GameObject.Instantiate(Resources.Load("prefabs/Tom")) as GameObject;
		}
		cam = GameObject.FindObjectOfType<Camera>();
		SetFocus(playerObject);
		MySaver.CleanupSaves();
	}

	void PostLoad(GameObject playerObject){
//		playerObject = GameObject.Find(lastPlayerName);	
		cam = GameObject.FindObjectOfType<Camera>();
		if (playerObject){
			SetFocus(playerObject);
			Intrinsics intrinsics = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(playerObject);
			FocusIntrinsicsChanged(intrinsics.NetIntrinsic());
		}
		
	}

	public void LeaveScene(string toSceneName,int toEntryNumber){
		// call mysaver, tell it to save scene and player separately
		MySaver.Save();
		// unity load saved editor scene file
		entryID = toEntryNumber;
		Application.LoadLevel(toSceneName);
	}
	void OnLevelWasLoaded(int level) {
		// call scene load routine & load player
		GameObject player = MySaver.LoadScene();
		// initialize values re: player object focus
		PostLoad(player);
		// initialize UI system references
//		UISystem.Instance.PostLoadInit();
		// place player at appropriate entrance
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

