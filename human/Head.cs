using UnityEngine;
public class Head : Interactive, IExcludable {
	private GameObject hatPoint;
	public Hat hat;
	public SpriteRenderer hatRenderer;
	public Hat initHat;
	private bool LoadInitialized = false;
	void Start(){
		if (!LoadInitialized)
			LoadInit();
		if (initHat)
			DonHat(initHat);
	}
	void LoadInit(){
		Interaction wearAct = new Interaction(this, "Wear", "DonHat");
		wearAct.dontWipeInterface = false;
		interactions.Add(wearAct);
		hatPoint = transform.Find("hatPoint").gameObject;
		LoadInitialized = true;
	}
	
	public void DonHat(Hat h){
		if (hat)
			RemoveHat();

		Messenger.Instance.ClaimObject(h.gameObject,this);
		hat = h;

		PhysicalBootstrapper phys = hat.GetComponent<PhysicalBootstrapper>();
		if (phys)
			phys.DestroyPhysical();

		OrderByY yorder = hat.GetComponent<OrderByY>();
		if (yorder)
			yorder.AddFollower(gameObject, 1);
		// if (yorder)
		// 	yorder.enabled = false;
		
		hatRenderer = hat.GetComponent<SpriteRenderer>();
		hat.transform.position = hatPoint.transform.position;
		hatPoint.transform.localScale = Vector3.one;
		transform.localScale = Vector3.one;
		hat.transform.rotation = Quaternion.identity;
		hat.transform.parent = hatPoint.transform;
		hat.transform.rotation = Quaternion.identity;
		hat.GetComponent<Rigidbody2D>().isKinematic = true;
		hat.GetComponent<Collider2D>().isTrigger = true;
		HatAnimation hatAnimator = hat.GetComponent<HatAnimation>();
		if (hatAnimator){
			hatAnimator.CheckDependencies();
		}
		Toolbox.Instance.AddIntrinsic(transform.parent.gameObject, hat.gameObject);
		GameManager.Instance.CheckItemCollection(transform.parent.gameObject, h.gameObject);
	}
	
	void RemoveHat(){
		Toolbox.Instance.RemoveIntrinsic(transform.parent.gameObject, hat.gameObject);
		Messenger.Instance.DisclaimObject(hat.gameObject,this);
		
		hat.GetComponent<Rigidbody2D>().isKinematic = false;
		hat.GetComponent<Collider2D>().isTrigger = false;

		PhysicalBootstrapper phys = hat.GetComponent<PhysicalBootstrapper>();
		if (phys){
			phys.InitPhysical(0.2f,Vector2.zero);
		} else {
			hat.transform.parent = null;
		}
		SpriteRenderer hatRenderer = hat.GetComponent<SpriteRenderer>();
		if (hatRenderer){
			hatRenderer.sortingLayerName = "main";
		}
		OrderByY yorder = hat.GetComponent<OrderByY>();
		if (yorder)
			yorder.RemoveFollower(gameObject);
		HatAnimation hatAnimator = hat.GetComponent<HatAnimation>();
		if (hatAnimator){
			hatAnimator.CheckDependencies();
		}
		hat = null;
		hatRenderer = null;

	}
	public void DropMessage(GameObject obj){
		RemoveHat();
	}
	public void WasDestroyed(GameObject obj){
		if (obj == hat.gameObject){
			hat = null;
		}
	}
}
