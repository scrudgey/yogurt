using UnityEngine;

public class DestroyAfterTime : MonoBehaviour {
	public float lifetime;
	void Start () {
		// TODO: implement claimsmanager wasdestroyed with timer
		ClaimsManager.Instance.WasDestroyed(gameObject);
		Destroy(gameObject, lifetime);
	}
}
