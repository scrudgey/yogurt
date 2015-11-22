using UnityEngine;
using System.Collections;

public class HeadAnimation : MonoBehaviour {

	private Controllable controllable;
	private Speech speech;
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
	private SpriteRenderer parentSprite;
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
	private Head head;

	void LoadSprites(){
		sprites = Resources.LoadAll<Sprite>("sprites/" + spriteSheet);
	}
	
	public void UpdateSequence(){
		GetComponent<Animation>().Play(sequence);
	}

	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		head = GetComponent<Head>();
		foreach (SpriteRenderer renderer in GetComponentsInParent<SpriteRenderer>()){
			if (renderer != spriteRenderer)
				parentSprite = renderer;
		}
		controllable = GetComponentInParent<Controllable>();
		speech = GetComponentInParent<Speech>();

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
			p.GetComponent<Renderer>().sortingLayerName="held item";
		}
		crumbs = GetComponentInChildren<ParticleSystem>();
	}

	public void SetEating(bool eating, Color crumbColor){
		this.crumbColor = crumbColor;
		this.eating = eating;
		crumbs.startColor = crumbColor;
		if (eating){
			eatingCountDown = 2f;
			if (!crumbs.isPlaying)
				crumbs.Play();
		}
	}

	public void SetVomit(bool v){
		vomiting = v;
		vomit.startColor = crumbColor;
		if (vomiting){
			vomitCountDown = 1.5f;
			if (!vomit.isPlaying)
				vomit.Play();
		}
	}

	void Update () {
		string updateSheet = baseName;
		string updateSequence = "generic";

		if (speech.speaking || eating){
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

		switch (controllable.lastPressed){
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

		spriteSheet = updateSheet;
		sequence = updateSequence;

		if (parentSprite)
			spriteRenderer.sortingOrder = parentSprite.sortingOrder;
		if (head.hatRenderer)
			head.hatRenderer.sortingOrder = parentSprite.sortingOrder + 1;

	}

	public void SetFrame(int animationFrame){
		frame = animationFrame + baseFrame;
		spriteRenderer.sprite = sprites[frame];
	}
}
