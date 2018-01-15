using UnityEngine;
using UnityEngine.UI;

public class SceneButton : MonoBehaviour {
	public TeleportMenu menu;
	public string scene_name;
	public void Clicked(){
		menu.SceneButtonCallback(this);
	}
	public void SetValues(TeleportMenu menu, string scene_name){
		this.menu = menu;
		this.scene_name = scene_name;
		Text text = transform.Find("Text").GetComponent<Text>();
		text.text = scene_name;
	}
}
