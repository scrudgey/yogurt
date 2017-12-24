using UnityEngine;
using System.Collections.Generic;

public class Eater : Interactive, IMessagable, ISaveable {
	public float nutrition;
	public enum preference{neutral, likes, dislikes}
	enum nauseaStatement{none, warning, imminent}
	nauseaStatement lastStatement;
	public preference vegetablePreference;
	public preference meatPreference;
	public preference immoralPreference;
	public preference offalPreference;
	private AudioSource audioSource;
	private float _nausea;
	public float nausea{
		get { 
			return _nausea;
		}
		set {
			_nausea = value;
			CheckNausea();
			}
	}
    private bool poisonNausea;
	private Queue<GameObject> eatenQueue;
	private bool LoadInitialized = false;
	public float vomitCountDown;
	private void CheckNausea(){
		//TODO: this is spawning lots of flags
		if (nausea > 15 && nausea < 30 && lastStatement != nauseaStatement.warning){
			lastStatement = nauseaStatement.warning;
			MessageSpeech message = new MessageSpeech("I don't feel so good!!", eventData: new EventData(chaos:1, disturbing:1, positive:-1));
			Toolbox.Instance.SendMessage(gameObject, this, message);
		}
		if (nausea > 30 && lastStatement != nauseaStatement.imminent){
			lastStatement = nauseaStatement.imminent;
			MessageSpeech message = new MessageSpeech("I'm gonna puke!'", eventData: new EventData(chaos:2, disturbing:1, positive:-2));
			Toolbox.Instance.SendMessage(gameObject, this, message);
		}
	}
	void Start () {
		if (!LoadInitialized)
			LoadInit();
	}
	public void LoadInit(){
		Interaction eatAction = new Interaction(this, "Eat", "Eat");
		eatAction.defaultPriority = 1;
		eatAction.dontWipeInterface = false;
		eatAction.otherOnPlayerConsent = false;
		interactions.Add(eatAction);
		eatenQueue = new Queue<GameObject>();
		LoadInitialized = true;
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
	}
	void Update () {
        if (poisonNausea){
            nausea += Time.deltaTime * 30f;
        }
		if (nausea > 50){
			Vomit();
		}
		if (nutrition > 100){
			nausea += Time.deltaTime * 2;
		}
		if (vomitCountDown > 0){
			vomitCountDown -= Time.deltaTime;
			if (vomitCountDown <= 0){
				MessageHead head = new MessageHead();
				head.type = MessageHead.HeadType.vomiting;
				head.value = false;
				Toolbox.Instance.SendMessage(gameObject, this, head);
			}
		}
	}
	public int CheckReaction(Edible food){
		int reaction = 0;
		//i can clean this section up with reflection-
		// might be necessary if food types get out of control
		bool[] types = new bool[] {food.vegetable, food.meat, food.immoral, food.offal};
		preference[] prefs = new preference[] {vegetablePreference, meatPreference, immoralPreference, 	offalPreference};
		for (int i =0; i< prefs.Length; i++){
			if (types[i]){
				switch (prefs[i]){
				case preference.dislikes:
					reaction -= 2;
					break;
				case preference.likes:
					reaction++;
					break;
				default:
					break;
				}
			}
		}
		return reaction;
	}
	public string Eat_desc(Edible food){
		string foodname = Toolbox.Instance.GetName(food.gameObject);
		return "Eat "+foodname;
	}
	public void Eat(Edible food){
		int reaction;
		nutrition += food.nutrition;
		MessageHead head = new MessageHead();
		head.type = MessageHead.HeadType.eating;
		head.value = true;
		head.crumbColor = food.pureeColor;
		Toolbox.Instance.SendMessage(gameObject, this, head);
		//randomly store a clone of the object for later vomiting
		GameObject eaten = Instantiate(food.gameObject) as GameObject;
		eaten.name = Toolbox.Instance.CloneRemover(eaten.name);
		eatenQueue.Enqueue(eaten);
		eaten.SetActive(false);
		if (eatenQueue.Count > 2){
			GameObject oldEaten = eatenQueue.Dequeue();
			ClaimsManager.Instance.WasDestroyed(oldEaten);
			Destroy(oldEaten);
		}
		//update our status based on our reaction to the food
		reaction = CheckReaction(food);
		if(reaction > 0){
			Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Yummy!", eventData:new EventData(positive:1)));
		}
		if(reaction < 0){
			nausea += 30;
			Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Yuck", eventData:new EventData(positive:-1)));
		}
		if (nutrition > 50){
			Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I'm full!"));
		}
		if (nutrition > 75){
			Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I can't eat another bite!"));
		}
		Toolbox.Instance.AddLiveBuffs(gameObject, food.gameObject);
        // set up an occurrence flag for this eating!
        OccurrenceEat eatData = new OccurrenceEat();
		eatData.eater = gameObject;
		eatData.edible = food;
		MonoLiquid mliquid = food.GetComponent<MonoLiquid>();
		if (mliquid){
			eatData.liquid = mliquid.liquid;
			if (mliquid.liquid != null){
				if (mliquid.liquid.name == "yogurt"){
					GameManager.Instance.data.achievementStats.yogurtEaten += 1;
					GameManager.Instance.CheckAchievements();
				}
			}
		}
		Toolbox.Instance.OccurenceFlag(gameObject, eatData);
		// play eat sound
		if (food.eatSound != null){
			Toolbox.Instance.AudioSpeaker(food.eatSound, transform.position);
		}
		GameManager.Instance.CheckItemCollection(food.gameObject, gameObject);
		food.BeEaten();
	}
	void Vomit(){
		vomitCountDown = 1.5f;

        GameManager.Instance.data.achievementStats.vomit += 1;
		GameManager.Instance.CheckAchievements();

		nausea = 0;
		nutrition = 0;
                
        OccurrenceVomit data = new OccurrenceVomit();
		data.vomiter = gameObject;
		if (eatenQueue.Count > 0){
			GameObject eaten = eatenQueue.Dequeue();
            data.vomit = eaten.gameObject;
			eaten.SetActive(true);
			eaten.transform.position = transform.position;
            MonoLiquid mono = eaten.GetComponent<MonoLiquid>();
            if (mono){
                GameObject droplet = Toolbox.Instance.SpawnDroplet(mono.liquid, 0f, gameObject, 0.15f);
                mono.liquid.vomit = true;
                mono.edible.vomit = true;
				if (mono.liquid.name == "yogurt"){
			        GameManager.Instance.data.achievementStats.yogurtVomit += 1;
					GameManager.Instance.CheckAchievements();
				}
				CircleCollider2D dropCollider = droplet.GetComponent<CircleCollider2D>();
				foreach(Collider2D collider in GetComponentsInChildren<Collider2D>()){
					Physics2D.IgnoreCollision(dropCollider, collider, true);
				}
            }
            Edible edible = eaten.GetComponent<Edible>();
            if (edible){
                edible.vomit = true;
            }
			eaten = null;
		}
		Toolbox.Instance.OccurenceFlag(gameObject, data);
		MessageHead head = new MessageHead();
		head.type = MessageHead.HeadType.vomiting;
		head.value = true;
		Toolbox.Instance.SendMessage(gameObject, this, head);
		Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Blaaaaargh!"));
		if (audioSource){
			audioSource.PlayOneShot(Resources.Load("sounds/vomit", typeof(AudioClip)) as AudioClip);
		}
		Liquid vomitLiquid = Liquid.LoadLiquid("vomit");
		vomitLiquid.vomit = true;
		Toolbox.Instance.SpawnDroplet(vomitLiquid, 0f, gameObject, 0.15f);
	}
	public void ReceiveMessage(Message incoming){
		if (incoming is MessageNetIntrinsic){
			MessageNetIntrinsic message = (MessageNetIntrinsic)incoming;
			if (message.netBuffs[BuffType.poison].boolValue){
				poisonNausea = true;
			} else {
				poisonNausea = false;
			}
		}
	}
	public void SaveData(PersistentComponent data){
		data.floats["nutrition"] = nutrition;
		int index = 0;
		while (eatenQueue.Count > 0){
			GameObject eaten = eatenQueue.Dequeue();
			MySaver.UpdateGameObjectReference(eaten, data, "eaten"+index.ToString());
			MySaver.AddToReferenceTree(gameObject, eaten);
			index ++;
		}
	}
	public void LoadData(PersistentComponent data){
		nutrition = data.floats["nutrition"];
		if (data.ints.ContainsKey("eaten1")){
			GameObject eaten = MySaver.IDToGameObject(data.ints["eaten1"]);
			eatenQueue.Enqueue(eaten);
			eaten.SetActive(false);
		}
		if (data.ints.ContainsKey("eaten0")){
			GameObject eaten = MySaver.IDToGameObject(data.ints["eaten0"]);
			eatenQueue.Enqueue(eaten);
			eaten.SetActive(false);
		}
		Debug.Log(eatenQueue.Count);
	}
}
