using UnityEngine;

public class HeadAnimation : MonoBehaviour, IMessagable, IDirectable {
	private bool speaking;
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
	private string sequence{
		get{ return _sequence;}
		set{ 
			if (_sequence != value){
				_sequence = value;
				UpdateSequence();
			}
		}
	}
	private Sprite[] sprites;
	private SpriteRenderer spriteRenderer;
	public string baseName;
	private int baseFrame;
	private int frame;
	public bool eating;
	private ParticleSystem crumbs;
	private ParticleSystem vomit;
	private bool vomiting;
	public Color crumbColor = Color.white;
	private float eatingCountDown;
	private float vomitCountDown;
	// private Head head;
	private Controllable.HitState hitState;
	private string lastPressed;
	void LoadSprites(){
		sprites = Resources.LoadAll<Sprite>("spritesheets/" + spriteSheet);
	}
	public void UpdateSequence(){
		GetComponent<Animation>().Play(sequence);
	}
	void Awake () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		MessageDirectable directableMessage = new MessageDirectable();
		directableMessage.addDirectable = (IDirectable)this;
		Toolbox.Instance.SendMessage(gameObject, this, directableMessage);

		ParticleSystem[] ps = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem p in ps){
			switch (p.name){
			case "crumbs":
				crumbs = p;
				break;
			case "vom":
				vomit = p;
				break;
			default:
				break;
			}
			p.GetComponent<Renderer>().sortingLayerName="air";
		}
		crumbs = GetComponentInChildren<ParticleSystem>();
	}
	public void ReceiveMessage(Message incoming){
		if (incoming is MessageHead){
			MessageHead message = (MessageHead)incoming;
			switch (message.type){
				case MessageHead.HeadType.eating:
				crumbColor = message.crumbColor;
				eating = message.value;
				var mainModule = crumbs.main;
				mainModule.startColor = crumbColor;

				if (eating){
					eatingCountDown = 2f;
					if (!crumbs.isPlaying)
						crumbs.Play();
				}
				break;

				case MessageHead.HeadType.vomiting:
				vomiting = message.value;
				mainModule = crumbs.main;
				mainModule.startColor = crumbColor;
				if (vomiting){
					vomitCountDown = 1.5f;
					if (!vomit.isPlaying)
						vomit.Play();
				}
				break;

				case MessageHead.HeadType.speaking:
				speaking = message.value;
				break;

				default:
				break;
			}	
		}
		if (incoming is MessageHitstun){
			MessageHitstun message = (MessageHitstun)incoming;
			hitState = message.hitState;
		}
	}
	void Update () {
		string updateSheet = baseName;
		string updateSequence = "generic";

		if (speaking || eating){
			updateSequence = updateSequence + "_speak";
		}else{
			updateSequence = updateSequence + "_idle";
		}
		if (eatingCountDown > 0){
			eatingCountDown -= Time.deltaTime;
			if (eatingCountDown < 0){
				eating = false;
				crumbs.Stop();
			}
		}
		if (vomitCountDown > 0){
			vomitCountDown -= Time.deltaTime;
			if (vomitCountDown < 0){
				vomiting = false;
				vomit.Stop();
			}
		}
		updateSheet = updateSheet + "_head";
		switch (lastPressed){
		case "down":
			baseFrame = 2;
			break;
		case "up":
			baseFrame = 4;
			break;
		default:
			baseFrame = 0;
			break;
		}
		if (hitState > Controllable.HitState.none && !speaking && !eating){
			baseFrame +=1 ;
		}
		spriteSheet = updateSheet;
		sequence = updateSequence;
	}
	public void SetFrame(int animationFrame){
		frame = animationFrame + baseFrame;
		spriteRenderer.sprite = sprites[frame];
	}
	public void DirectionChange(Vector2 newdir){
		lastPressed = Toolbox.Instance.DirectionToString(newdir);
	}
}
