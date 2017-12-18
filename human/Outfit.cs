using UnityEngine;

public class Outfit : Interactive, IMessagable, ISaveable {
	private bool LoadInitialized = false;
	public string wornUniformName;
	public GameObject initUniform;
    public Controllable.HitState hitState;
	public Intrinsics uniformIntrinsics;
	void Start(){
		if (!LoadInitialized)
			LoadInit();
		if (initUniform != null){
			// Debug.Log("donning init uniform");
			GameObject uniObject = initUniform;
			if (!initUniform.activeInHierarchy){ 
				uniObject = Instantiate(initUniform);
			}
			Uniform uniform = uniObject.GetComponent<Uniform>();
			GameObject removedUniform = DonUniform(uniform, cleanStains:false);
			Destroy(removedUniform);
		}
	}
	public void LoadInit(){
		if (LoadInitialized)	
			return;
		Interaction wearInteraction = new Interaction(this, "Wear", "DonUniformWrapper");
		wearInteraction.dontWipeInterface = false;
		interactions.Add(wearInteraction);

		Interaction stealInteraction = new Interaction(this, "Take Outfit", "StealUniform");
		stealInteraction.validationFunction = true;
		interactions.Add(stealInteraction);

		LoadInitialized = true;
	}
	public void StealUniform(Outfit otherOutfit){
		// TODO: add naked outfit
		GameObject uniObject = RemoveUniform();
		Uniform uniform = uniObject.GetComponent<Uniform>();
		otherOutfit.DonUniform(uniform);
		GoNude();
	}
	public void GoNude(){
		MessageAnimation anim = new MessageAnimation();
		anim.outfitName = "nude";
		Toolbox.Instance.SendMessage(gameObject, this, anim);
		wornUniformName = "nude";
	}
	public bool StealUniform_Validation(Outfit otherOutfit){
		if (otherOutfit == this)
			return false;
		if (hitState >= Controllable.HitState.unconscious)
            return true;
		return false;
	}
	public string StealUniform_desc(Outfit otherOutfit){
		return "Take "+wornUniformName;
	}
	public void DonUniformWrapper(Uniform uniform){
		DonUniform(uniform);
	}
	public GameObject DonUniform(Uniform uniform, bool cleanStains=true){
		GameObject removedUniform = RemoveUniform();
		PhysicalBootstrapper phys = uniform.GetComponent<PhysicalBootstrapper>();
		if (phys)
			phys.DestroyPhysical();
		MessageAnimation anim = new MessageAnimation();
		anim.outfitName = uniform.baseName;
		Toolbox.Instance.SendMessage(gameObject, this, anim);
		uniformIntrinsics = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(uniform.gameObject);
		Intrinsics intrinsics = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(gameObject);
		intrinsics.AddChild(uniformIntrinsics);
		wornUniformName = Toolbox.Instance.CloneRemover(uniform.gameObject.name);
		GameManager.Instance.CheckItemCollection(uniform.gameObject, gameObject);
		ClaimsManager.Instance.WasDestroyed(uniform.gameObject);
		Destroy(uniform.gameObject);
		if (cleanStains){
			foreach(Stain stain in GetComponentsInChildren<Stain>()){
				Destroy(stain.gameObject);
			}
		}
		return removedUniform;
	}

	public string DonUniform_desc(Uniform uniform){
		string uniformName = Toolbox.Instance.GetName(uniform.gameObject);
		return "Wear "+uniformName;
	}
	public GameObject RemoveUniform(){
		// something has to change here if we're going to standardize name usage
		string prefabName = Toolbox.Instance.ReplaceUnderscore(wornUniformName);
		GameObject uniform = Instantiate(Resources.Load("prefabs/"+prefabName)) as GameObject;
		Intrinsics intrinsics = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(gameObject);
		intrinsics.RemoveChild(uniformIntrinsics);
		uniform.transform.position = transform.position;
		SpriteRenderer sprite = uniform.GetComponent<SpriteRenderer>();
		sprite.sortingLayerName = "ground";
		return uniform;
	}
	public void ReceiveMessage(Message incoming){
        if (incoming is MessageHitstun){
			MessageHitstun hits = (MessageHitstun)incoming;
            hitState = hits.hitState;
		}
    }
	public void SaveData(PersistentComponent data){
		data.strings["worn"] = wornUniformName;
		data.ints["hitstate"] = (int)hitState;
	}
	public void LoadData(PersistentComponent data){
		wornUniformName = data.strings["worn"];
		initUniform = Resources.Load("prefabs/"+data.strings["worn"]) as GameObject;
		hitState = (Controllable.HitState)data.ints["hitstate"];
	}
}
