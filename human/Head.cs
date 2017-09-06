using UnityEngine;
public class Head : Interactive, IExcludable {
	private GameObject hatPoint;
	public Hat hat;
	public Hat initHat;
	private bool LoadInitialized = false;
	private SpriteRenderer spriteRenderer;
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
		wearAct.validationFunction = true;
		wearAct.playerOnOtherConsent = false;
		interactions.Add(wearAct);
		hatPoint = transform.Find("hatPoint").gameObject;
		LoadInitialized = true;
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	public void DonHat(Hat h){
		if (hat)
			RemoveHat();
		ClaimsManager.Instance.ClaimObject(h.gameObject, this);
		hat = h;

		PhysicalBootstrapper phys = hat.GetComponent<PhysicalBootstrapper>();
		if (phys)
			phys.DestroyPhysical();
		hatPoint.transform.localScale = Vector3.one;
		transform.localScale = Vector3.one;
		hat.transform.rotation = Quaternion.identity;
		if (hat.helmet){
			hat.transform.position = transform.position;
			hat.transform.SetParent(transform, true);
			spriteRenderer.enabled = false;
		} else {
			hat.transform.position = hatPoint.transform.position;
			hat.transform.SetParent(hatPoint.transform, true);
		}
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
	public string DonHat_desc(Hat h){
		return "Wear "+Toolbox.Instance.GetName(h.gameObject);
	}
	public bool DonHat_Validation(Hat h){
		// Debug.Log(Messenger.Instance.claimedItems.ContainsKey(h.gameObject));
		return !ClaimsManager.Instance.claimedItems.ContainsKey(h.gameObject);
	}
	void RemoveHat(){
		if (hat.helmet){
			spriteRenderer.enabled = true;
		}
		Toolbox.Instance.RemoveIntrinsic(transform.parent.gameObject, hat.gameObject);
		ClaimsManager.Instance.DisclaimObject(hat.gameObject,this);
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
