using UnityEngine;
// using System.Collections;

public class SingletonInitializer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		string tempstring;
		tempstring = Toolbox.Instance.message;
		RobustLoad();
		tempstring = GameManager.Instance.message;
		tempstring = Messenger.Instance.MOTD;
		tempstring = UINew.Instance.MOTD;
		Debug.Log(tempstring);
	}

	public void RobustLoad(){
		// this routine checks to make sure all the necessary pieces are instantiated in the scene.
		// if they aren't, it will spawn them.
		string[] requirements = new string[] {"singleton initializer", "Main Camera", "EventSystem", "NeoUICanvas", "InventoryScreen"};
		foreach(string requirement in requirements){
			GameObject go = GameObject.Find(requirement);
			if (!go){
				string path = @"required/"+requirement;
				GameObject newgo = GameObject.Instantiate(Resources.Load(path)) as GameObject;
				newgo.name = Toolbox.Instance.ScrubText(newgo.name);
			}
		}
		
		GameObject mainCamera = GameObject.Find("Main Camera");
		Camera cam = mainCamera.GetComponent<Camera>();
		if (mainCamera){
			CameraControl cameraControl = mainCamera.GetComponent<CameraControl>();
			if (!cameraControl)
				cameraControl = mainCamera.AddComponent<CameraControl>();
			Canvas uiCanvas = GameObject.Find("NeoUICanvas").GetComponent<Canvas>();
			uiCanvas.worldCamera = cam;
			Canvas inventoryCanvas = GameObject.Find("InventoryScreen").GetComponent<Canvas>();
			inventoryCanvas.worldCamera = cam;
		}

		GameObject ui = GameObject.Find("UI");
		if (ui){
			Canvas canvas = ui.GetComponent<Canvas>();
			canvas.worldCamera = cam;
		}
		UINew.Instance.PostLoadInit();
	}
}
