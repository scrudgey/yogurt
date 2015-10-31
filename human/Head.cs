using UnityEngine;
using System.Collections;

public class Head : Interactive, IExcludable {
	
	private GameObject hatPoint;
	public Hat hat;
	private SpriteRenderer spriteRenderer;
	private Intrinsics intrinsics;
	
	private bool LoadInitialized = false;
	void Start(){
		if (!LoadInitialized)
			LoadInit();
	}

	void LoadInit(){
//		interactions.Add( new Interaction(this,"Wear","DonHat"));
		Interaction wearAct = new Interaction(this, "Wear", "DonHat");
		wearAct.dontWipeInterface = false;
		interactions.Add(wearAct);

		hatPoint = transform.Find("hatPoint").gameObject;
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		intrinsics = gameObject.GetComponentInParent<Intrinsics>();
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
		
		SpriteRenderer hatRenderer = hat.GetComponent<SpriteRenderer>();
		if (hatRenderer)
			hatRenderer.sortingOrder = spriteRenderer.sortingOrder+1;
		

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

		if (intrinsics){
			Intrinsics hatIntrinsic = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(hat.gameObject);
			intrinsics.AddIntrinsic(hatIntrinsic);
		}
	}
	
	void RemoveHat(){
		if (intrinsics){
			Intrinsics hatIntrinsic = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(hat.gameObject);
			intrinsics.RemoveIntrinsic(hatIntrinsic);
		}
		
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
			hatRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
		}
		HatAnimation hatAnimator = hat.GetComponent<HatAnimation>();
		if (hatAnimator){
			hatAnimator.CheckDependencies();
		}
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
