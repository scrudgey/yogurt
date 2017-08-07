using UnityEngine;

public class DeathTrap1 : MonoBehaviour {
	public GameObject potion;
	public GameObject door;
	public Transform firePoint;
	public GameObject dart;
	public float timer;
	public bool trapActive;
	public bool trapSprung;
	public AudioClip fireSound;
	public AudioClip doorOpenSound;
	public AudioClip tripSound;
	public Pickup potionPickup;
	public float fireAngle = 224f;
	public int fireCycles;
	public bool trapDone;
	public CameraControl cameraControl;
	public GameObject doorWay;
	void Awake(){
		doorWay.SetActive(false);
		// trapSprung = true;
		// trapActive = true;
	}
	void Update(){
		timer += Time.deltaTime;
		if (timer > 2 && !trapActive){
			trapActive = true;
			timer = 0;
		}
		if (!trapActive)
			return;
		if (potion == null && !trapSprung){
			trapSprung = true;
			Toolbox.Instance.AudioSpeaker(tripSound, door.transform.position);
			cameraControl.Shake(0.2f);
			timer = -3f;
		}
		if (potionPickup.holder != null && !trapSprung){
			trapSprung = true;
			Toolbox.Instance.AudioSpeaker(tripSound, potion.transform.position);
			cameraControl.Shake(0.2f);
			timer = -3f;
		}
		if (trapSprung && timer > 0.1f && !trapDone){
			fireAngle -= 5f;
			timer = 0;
			GameObject dartObj = Instantiate(dart, firePoint.position, Quaternion.LookRotation(Vector2.left, Vector3.up));
			Rigidbody2D dartBody = dartObj.GetComponent<Rigidbody2D>();
			dartBody.velocity = new Vector2(Mathf.Cos(fireAngle * 2 * Mathf.PI / 360f), Mathf.Sin(fireAngle * 2 * Mathf.PI / 360f))*5f;
			Toolbox.Instance.AudioSpeaker(fireSound, firePoint.position);
			if (fireAngle <= 130f){
				fireAngle = 224f;
				timer = -1f;
				fireCycles += 1;
				if (fireCycles >= 3){
					trapDone = true;
					timer = 0;
				}
			}
		}
		if (timer > 2f && trapDone && door != null){
			Toolbox.Instance.AudioSpeaker(doorOpenSound, firePoint.position);
			Destroy(door);
			doorWay.SetActive(true);
		}
	}
}
