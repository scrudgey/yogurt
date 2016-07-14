using UnityEngine;
// using System.Collections;

public class Outfit : Interactive {

	private bool LoadInitialized = false;
	public string wornUniformName;

	void Start(){
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
		MessageAnimation anim = new MessageAnimation();
		anim.outfitName = uniform.baseName;
		Toolbox.Instance.SendMessage(gameObject, this, anim);

		wornUniformName = Toolbox.Instance.CloneRemover(uniform.gameObject.name);
		GameManager.Instance.CheckItemCollection(uniform.gameObject, gameObject);
		Destroy(uniform.gameObject);
	}

	public string DonUniform_desc(Uniform uniform){
		string uniformName = Toolbox.Instance.GetName(uniform.gameObject);
		return "Wear "+uniformName;
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
