using UnityEngine;

public class Spawner : MonoBehaviour {
	public GameObject target;
	private float timer;
	public float delayTime;
	void Update () {
		timer += Time.deltaTime;
		if (delayTime == 0){
			timer = -1;
			Destroy(gameObject);
			ClaimsManager.Instance.WasDestroyed(gameObject);
		}
		if (timer > delayTime){
			Instantiate(target, transform.position, Quaternion.identity);
			Destroy(gameObject);
			ClaimsManager.Instance.WasDestroyed(gameObject);
		}
	}
}
