using UnityEngine;
using System.Collections;

public class HatAnimation : MonoBehaviour {

	private Controllable controllable;
	public Sprite rightSprite;
	public Sprite downSprite;
	public Sprite upSprite;
	private SpriteRenderer spriteRenderer;
	private string _direction;
	public string direction{
		get{ return _direction;}
		set{
			if (_direction != value){
				_direction = value;
				UpdateSprite();
			}
		}
	}

	public void CheckDependencies(){
		controllable = GetComponentInParent<Controllable>();
	}

	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		controllable = GetComponentInParent<Controllable>();
	}


	public void UpdateSprite(){
		switch (controllable.lastPressed){
		case "down":
			spriteRenderer.sprite = downSprite;
			break;
		case "up":
			spriteRenderer.sprite = upSprite;
			break;
		default:
			spriteRenderer.sprite = rightSprite;
			break;
		}
	}
}
