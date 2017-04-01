using UnityEngine;
public class Head : Interactive, IExcludable {
	private GameObject hatPoint;
	public Hat hat;
	public Hat initHat;
	private bool LoadInitialized = false;
	void Start(){
		if (!LoadInitialized)
			LoadInit();
		if (initHat){
			if (initHat.isActiveAndEnabled){
				DonHat(initHat);
			} else {
				Hat instance = Instantiate(initHat) as Hat;
				DonHat(instance);
			}
		}
	}
	void LoadInit(){
		Interaction wearAct = new Interaction(this, "Wear", "DonHat");
		wearAct.dontWipeInterface = false;
		interactions.Add(wearAct);
		hatPoint = transform.Find("hatPoint").gameObject;
		LoadInitialized = true;
		OrderByY order = GetComponent<OrderByY>();
		if (order != null){
			order.AddFollower(transform.parent.gameObject, 1);
		}
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
			hatAnimator.RegisterDirectable();
		}
		Toolbox.Instance.AddIntrinsic(transform.parent.gameObject, hat.gameObject);
		GameManager.Instance.CheckItemCollection(transform.parent.gameObject, h.gameObject);
	}
	
	void RemoveHat(){
		Toolbox.Instance.RemoveIntrinsic(transform.parent.gameObject, hat.gameObject);
		Messenger.Instance.DisclaimObject(hat.gameObject,this);
		HatAnimation hatAnimator = hat.GetComponent<HatAnimation>();
		if (hatAnimator){
			hatAnimator.RemoveDirectable();
		}
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
		hat = null;
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
