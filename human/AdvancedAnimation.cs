using UnityEngine;
// using System.Collections;

// how much of this should i roll into another class? it makes sense to separate all
// animation business into one class, but it depends on so many individual parts
// (humanoid, inventory)

public class AdvancedAnimation : MonoBehaviour {
	private string _spriteSheet;
	private string spriteSheet{
		get { return _spriteSheet;}
		set {
			if (value != _spriteSheet){
				_spriteSheet = value;
				LoadSprites();
			}
		}
	}
	private string _sequence;
	public  string sequence{
		get{ return _sequence;}
		set{ 
			if (_sequence != value){
				_sequence = value;
				UpdateSequence();
			}
		}
	}
	
	private SpriteRenderer spriteRenderer;
	private Controllable controllable;
	private Inventory inventory;
	private bool holding;
	private bool oldHolding;
	private Sprite[] sprites;
	public string baseName;
	private int baseFrame;
	private int frame;

	public void LoadSprites(){
		sprites = Resources.LoadAll<Sprite>("sprites/"+spriteSheet);
	}
	
	public void UpdateSequence(){
		GetComponent<Animation>().Play(sequence);
	}

	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		controllable = GetComponent<Controllable>();
		inventory = GetComponent<Inventory>();
		LoadSprites();
	}

	void LateUpdate(){

		spriteSheet = baseName+"_spritesheet";
		string updateSequence = "generic3";
		holding = inventory.holding;

		if (inventory.swinging){

			updateSequence = updateSequence+"_swing_"+controllable.lastPressed;
			switch (controllable.lastPressed){
			case "down":
				baseFrame = 44;
				break;
			case "up":
				baseFrame = 46;
				break;
			default:
				baseFrame = 42;
				break;
			}

		} else {
			switch (controllable.lastPressed){
			case "down":
				baseFrame = 7;
				break;
			case "up":
				baseFrame = 14;
				break;
			default:
				baseFrame = 0;
				break;
			}
			if (inventory.holding){
				baseFrame += 21;
			}
			if (GetComponent<Rigidbody2D>().velocity.magnitude > 0.1){
				updateSequence = updateSequence + "_run_"+controllable.lastPressed;;
				baseFrame += 1;
			}
			else{
				updateSequence = updateSequence + "_idle_"+controllable.lastPressed;;
			}
		}

		if (oldHolding != holding)
			SetFrame(0);

		sequence = updateSequence;
		oldHolding = holding;
	}

	public void SetFrame(int animationFrame){
		frame = animationFrame + baseFrame;
		spriteRenderer.sprite = sprites[frame];
	}



}
