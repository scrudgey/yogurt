using UnityEngine;

public class Eater : Interactive {
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
	private GameObject eaten;
	private bool LoadInitialized = false;
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
		LoadInitialized = true;
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
	}
	void Update () {
        if (poisonNausea){
            nausea += Time.deltaTime * 50f;
        }
		if (nausea > 50){
			Vomit();
            poisonNausea = false;
		}
		if (nutrition > 100){
			nausea += Time.deltaTime * 2;
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
	public void Eat (Edible food){
		int reaction;
		nutrition += food.nutrition;
        if (food.poison)
            poisonNausea = true;
		MessageHead head = new MessageHead();
		head.type = MessageHead.HeadType.eating;
		head.value = true;
		head.crumbColor = food.pureeColor;
		Toolbox.Instance.SendMessage(gameObject, this, head);
		//randomly store a clone of the object for later vomiting
        if (!food.poison){
            if (eaten){
				ClaimsManager.Instance.WasDestroyed(eaten);
                Destroy(eaten);
            }
            eaten = Instantiate(food.gameObject) as GameObject;
            eaten.SetActive(false);
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
		Toolbox.Instance.AddIntrinsic(gameObject, food.gameObject);
		
        // set up an occurrence flag for this eating!
        OccurrenceEat eatData = new OccurrenceEat();
		eatData.eater = gameObject;
		eatData.edible = food;
        // eatData.food = food.name;
        // eatData.amount = food.nutrition;
		// eatData.vomit = food.vomit;
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
        GameManager.Instance.data.achievementStats.vomit += 1;
		GameManager.Instance.CheckAchievements();

		nausea = 0;
		nutrition = 0;
                
        OccurrenceVomit data = new OccurrenceVomit();
		data.vomiter = gameObject;
		if (eaten){
            data.vomit = eaten.gameObject;
			eaten.SetActive(true);
			eaten.transform.position = transform.position;
            MonoLiquid mono = eaten.GetComponent<MonoLiquid>();
            if (mono){
                GameObject droplet = Toolbox.Instance.SpawnDroplet(transform.position, mono.liquid);
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
	}
}
