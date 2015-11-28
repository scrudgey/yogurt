using UnityEngine;
// using System.Collections;

public class Outfit : Interactive {

	private AdvancedAnimation advancedAnimation;
	private bool LoadInitialized = false;
	public string wornUniformName;

	void Start(){
		advancedAnimation = GetComponent<AdvancedAnimation>();
		if (!LoadInitialized)
			LoadInit();
	}

	public void LoadInit(){
		Interaction wearInteraction = new Interaction(this, "Wear", "DonUniform");
		wearInteraction.dontWipeInterface = false;
		interactions.Add(wearInteraction);
		LoadInitialized = true;
	}

	public void DonUniform(Uniform uniform){
		RemoveUniform();
		PhysicalBootstrapper phys = uniform.GetComponent<PhysicalBootstrapper>();
		if (phys)
			phys.DestroyPhysical();
		advancedAnimation.baseName = uniform.baseName;
		wornUniformName = Toolbox.Instance.CloneRemover(uniform.gameObject.name);
		Destroy(uniform.gameObject);
	}

	void RemoveUniform(){
		// something has to change here if we're going to standardize name usage
		string prefabName = Toolbox.Instance.ReplaceUnderscore(wornUniformName);
		GameObject uniform = Instantiate(Resources.Load("prefabs/"+prefabName)) as GameObject;
		uniform.transform.position = transform.position;
		SpriteRenderer sprite = uniform.GetComponent<SpriteRenderer>();
		sprite.sortingLayerName = "ground";
	}

}
