using UnityEngine;

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
	private bool swinging;
	private bool holding;
	private bool throwing;
	private bool oldHolding;
	private Sprite[] sprites;
	public string baseName;
	private int baseFrame;
	private int frame;


	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		controllable = GetComponent<Controllable>();
		LoadSprites();
	}
	public void Holding(bool val){
		holding = val;
	}
	public void Swinging(bool val){
		swinging = val;
	}
	public void Throwing(bool val){
		throwing = val;
	}
	
	public void LoadSprites(){
		sprites = Resources.LoadAll<Sprite>("sprites/"+spriteSheet);
	}
	public void UpdateSequence(){
		GetComponent<Animation>().Play(sequence);
	}

	void LateUpdate(){
		spriteSheet = baseName+"_spritesheet";
		string updateSequence = "generic3";
		
		if (swinging){
			updateSequence = GetSwingState(updateSequence);
		} else if (throwing){
			updateSequence = GetThrowState(updateSequence);
		} else {
			updateSequence = GetWalkState(updateSequence);		
			if (oldHolding != holding)
				SetFrame(0);
		}

		sequence = updateSequence;
		oldHolding = holding;
	}

	string GetSwingState(string updateSequence){
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
		return updateSequence;
	}
	
	string GetThrowState(string updateSequence){
		updateSequence = updateSequence+"_throw_"+controllable.lastPressed;
		switch (controllable.lastPressed){
			case "down":
				baseFrame = 50;
				break;
			case "up":
				baseFrame = 52;
				break;
			default:
				baseFrame = 48;
				break;
		}
		return updateSequence;
	}

	string GetWalkState(string updateSequence){
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
		if (holding){
			baseFrame += 21;
		}
		if (GetComponent<Rigidbody2D>().velocity.magnitude > 0.1){
			updateSequence = updateSequence + "_run_"+controllable.lastPressed;;
			baseFrame += 1;
		}
		else{
			updateSequence = updateSequence + "_idle_"+controllable.lastPressed;;
		}
		return updateSequence;
	}

	public void SetFrame(int animationFrame){
		frame = animationFrame + baseFrame;
		spriteRenderer.sprite = sprites[frame];
	}



}
