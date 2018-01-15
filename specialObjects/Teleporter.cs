using UnityEngine;

public class Teleporter : Interactive {
	void Start(){
		Interaction teleport = new Interaction(this, "Teleport", "Teleport");
		interactions.Add(teleport);
	}
	public void Teleport(){
		GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.teleport);
		TeleportMenu menu = menuObject.GetComponent<TeleportMenu>();
		menu.PopulateSceneList();
	}
}
