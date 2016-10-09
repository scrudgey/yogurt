using UnityEngine;
// using System.Collections;

public class Head : Interactive, IExcludable {
	
	private GameObject hatPoint;
	public Hat hat;
	public SpriteRenderer hatRenderer;
	
	private bool LoadInitialized = false;
	void Start(){
		if (!LoadInitialized)
			LoadInit();
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
			yorder.enabled = false;
		
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
		Intrinsics hatIntrinsic = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(hat.gameObject);
		MessageIntrinsic message = new MessageIntrinsic();
		message.addIntrinsic = hatIntrinsic;
		Toolbox.Instance.SendMessage(transform.parent.gameObject, this, message);

		GameManager.Instance.CheckItemCollection(h.gameObject, transform.parent.gameObject);
	}
	
	void RemoveHat(){
		Intrinsics hatIntrinsic = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(hat.gameObject);
		MessageIntrinsic message = new MessageIntrinsic();
		message.removeIntrinsic = hatIntrinsic;
		Toolbox.Instance.SendMessage(gameObject, this, message);
		
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
//			hatRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
		}
		OrderByY yorder = hat.GetComponent<OrderByY>();
		if (yorder)
			yorder.enabled = true;
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
