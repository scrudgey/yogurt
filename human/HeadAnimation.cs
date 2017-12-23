﻿using UnityEngine;

public class HeadAnimation : MonoBehaviour, IMessagable, IDirectable, ISaveable {
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
	public Controllable.HitState hitState;
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

		GameObject vomObject = Instantiate(Resources.Load("particles/vom"), transform.position, Quaternion.identity) as GameObject;
		vomit = vomObject.GetComponent<ParticleSystem>();
		vomObject.transform.SetParent(transform, false);
		vomObject.transform.localPosition = new Vector3(-0.009f, 0.0292f, 0);

		GameObject crumbObject = Instantiate(Resources.Load("particles/crumbs"), transform.position, Quaternion.identity) as GameObject;
		crumbs = crumbObject.GetComponent<ParticleSystem>();
		crumbObject.transform.SetParent(transform, false);
		crumbObject.transform.localPosition = new Vector3(0.0121f, 0.0250f, 0);
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
	public void SaveData(PersistentComponent data){
		data.ints["hitstate"] = (int)hitState;
		data.strings["baseName"] = baseName;
	}
	public void LoadData(PersistentComponent data){
		hitState = (Controllable.HitState)data.ints["hitstate"];
		baseName = data.strings["baseName"];
	}
}
