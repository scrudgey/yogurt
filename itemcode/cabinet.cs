using UnityEngine;
// using System.Collections;
using System.Collections.Generic;

public class Cabinet : Interactive {
	public bool open = false;
	public bool opened = false;
	public Sprite closedSprite;
	public Sprite openSprite;
	public AudioClip openSound;
	public AudioClip closeSound;
	public List<GameObject> contained;
	private AudioSource audioSource;
	void Start () {
		// Interaction openAct = n
		Interaction openAct = new Interaction(this, "Open", "Open");
		Interaction closeAct = new Interaction(this, "Close", "Close");
		openAct.validationFunction = true;
		closeAct.validationFunction = true;
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		interactions.Add(openAct);
		interactions.Add(closeAct);
	}
	public void Open(){
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = openSprite;
		foreach (GameObject prefab in contained){
			GameObject newObj = Instantiate(prefab) as GameObject;
			newObj.name = Toolbox.Instance.CloneRemover(newObj.name);
			Vector2 newPos = transform.position;
			newPos.y -= 0.2f;
			newObj.transform.position = newPos;
		}
		contained = new List<GameObject>();
		audioSource.PlayOneShot(openSound);
		open = true;
		opened = true;
	}
	public string Open_desc(){
		string myname = Toolbox.Instance.GetName(gameObject);
		return "Open "+myname;
	}
	public string Close_desc(){
		string myname = Toolbox.Instance.GetName(gameObject);
		return "Close "+myname;
	}
	public void Close(){
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = closedSprite;
		audioSource.PlayOneShot(closeSound);
		open = false;
	}
	public bool Open_Validation(){
		return !open;
	}
	public bool Close_Validation(){
		return open;
	}
}
