using UnityEngine;
using System.Collections.Generic;

public class Outfit : Interactive {
	private bool LoadInitialized = false;
	public string wornUniformName;
	public GameObject initUniform;
	void Start(){
		if (!LoadInitialized)
			LoadInit();
		if (initUniform != null){
			GameObject uniObject = Instantiate(initUniform);
			Uniform uniform = uniObject.GetComponent<Uniform>();
			GameObject removedUniform = DonUniform(uniform);
			Destroy(removedUniform);
		}
	}
	public void LoadInit(){
		Interaction wearInteraction = new Interaction(this, "Wear", "DonUniform");
		wearInteraction.dontWipeInterface = false;
		interactions.Add(wearInteraction);
		LoadInitialized = true;
	}
	public GameObject DonUniform(Uniform uniform){
		GameObject removedUniform = RemoveUniform();
		PhysicalBootstrapper phys = uniform.GetComponent<PhysicalBootstrapper>();
		if (phys)
			phys.DestroyPhysical();
		MessageAnimation anim = new MessageAnimation();
		anim.outfitName = uniform.baseName;
		Toolbox.Instance.SendMessage(gameObject, this, anim);
		List<Intrinsic> addedIntrins = Toolbox.Instance.AddIntrinsic(gameObject, uniform.gameObject);
		foreach (Intrinsic intrins in addedIntrins){
			intrins.persistent = true;
		}

		wornUniformName = Toolbox.Instance.CloneRemover(uniform.gameObject.name);
		GameManager.Instance.CheckItemCollection(uniform.gameObject, gameObject);
		Destroy(uniform.gameObject);
		return removedUniform;
	}
	public string DonUniform_desc(Uniform uniform){
		string uniformName = Toolbox.Instance.GetName(uniform.gameObject);
		return "Wear "+uniformName;
	}
	GameObject RemoveUniform(){
		// something has to change here if we're going to standardize name usage
		string prefabName = Toolbox.Instance.ReplaceUnderscore(wornUniformName);
		GameObject uniform = Instantiate(Resources.Load("prefabs/"+prefabName)) as GameObject;
		Toolbox.Instance.RemoveIntrinsic(gameObject, uniform.gameObject);
		uniform.transform.position = transform.position;
		SpriteRenderer sprite = uniform.GetComponent<SpriteRenderer>();
		sprite.sortingLayerName = "ground";
		return uniform;
	}

}
