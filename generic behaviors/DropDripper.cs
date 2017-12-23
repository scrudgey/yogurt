using UnityEngine;

public class DropDripper : MonoBehaviour, ISaveable {
	public string liquidType;
	public Liquid liquid;
	private PhysicalBootstrapper pBoot;
	public GameObject dripFX;
	private Pickup pickup;
	public float interval;
	private float timer;
	public int amount=5;
	void Start () {
		// timer = -0.5f;
		liquid = Liquid.LoadLiquid(liquidType);
		pBoot = GetComponent<PhysicalBootstrapper>();
		pickup = GetComponent<Pickup>();
		if (pBoot == null){
			Debug.Log("no bootstrapper found for dripper");
			Destroy(this);
		}
		dripFX = Instantiate(Resources.Load("particles/blood_trail"), transform.position, Quaternion.identity) as GameObject;
		dripFX.transform.SetParent(transform, true);
		dripFX.SetActive(false);
	}
	void Update () {
		timer += Time.deltaTime;
		if (timer > interval){
			if (pBoot.physical != null){
				if (pBoot.physical.height > 0.05){
					dripFX.SetActive(true);
					Drip(pBoot.physical.height);
				}
				return;
			}
			if (pickup != null){
				if (pickup.holder != null){
					Drip(pickup.holder.dropHeight);
					dripFX.SetActive(true);
				}
				return;
			}
		}
	}
	void Drip(float height){
		if (amount <= 0)
			return;
		amount -= 1;
		timer = 0;
		Vector3 initPosition = transform.position;
		initPosition.z = height;
		GameObject droplet = Toolbox.Instance.SpawnDroplet(initPosition, liquid, Vector3.zero);
		if (pickup != null){
			if (pickup.holder != null){
				foreach(Collider2D holderCollider in pickup.holder.GetComponentsInChildren<Collider2D>()){
					Collider2D dropCollider = droplet.GetComponent<Collider2D>();
					Physics2D.IgnoreCollision(holderCollider, dropCollider, true);
				}
			}
		}
	}
	public void SaveData(PersistentComponent data){
		data.ints["amount"] = amount;
	}
	public void LoadData(PersistentComponent data){
		amount = data.ints["amount"];
	}
}
