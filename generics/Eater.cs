using UnityEngine;

public class Eater : Interactive {
	
	public float nutrition;

	public enum preference{neutral, likes, dislikes}

	public preference vegetablePreference;
	public preference meatPreference;
	public preference immoralPreference;
	public preference offalPreference;
	private HeadAnimation head;
	private float _nausea;
	private float lastNausea;
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
	private Speech speech;
	private GameObject eaten;
	private bool LoadInitialized = false;
	private Intrinsics intrinsics;

	private void CheckNausea(){
		if ( nausea > 50 && lastNausea < 50){
			lastNausea = nausea;
			if (speech){
				speech.Say("I don't feel so good!");
			}
		}
		if ( nausea > 75 && lastNausea < 75){
			lastNausea = nausea;
			if (speech){
				speech.Say("I'm gonna puke!");
			}
		}
		if (nausea < 50){
			lastNausea = 0;
		}
	}

	void Start () {
		speech = GetComponent<Speech> ();
		head = GetComponentInChildren<HeadAnimation>();
		if (!LoadInitialized)
			LoadInit();
	}

	public void LoadInit(){
		Interaction eatAction = new Interaction(this, "Eat", "Eat");
		eatAction.defaultPriority = 1;
		eatAction.dontWipeInterface = false;
		interactions.Add(eatAction);
		LoadInitialized = true;
		intrinsics = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
        if (poisonNausea){
            nausea += Time.deltaTime * 50f;
        }
        
		if (nausea > 100){
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
		string phrase ="";
		int reaction;
		nutrition += food.nutrition;
        if (food.poison)
            poisonNausea = true;

		if (head)
			head.SetEating(true,food.pureeColor);

		//randomly store a clone of the object for later vomiting
        if (!food.poison){
            if (eaten){
                Destroy(eaten);
            }
            eaten = Instantiate(food.gameObject) as GameObject;
            eaten.SetActive(false);
        }

		//update our status based on our reaction to the food
		reaction = CheckReaction(food);
		if(reaction > 0){
			phrase = "Yummy!";
		}
		if(reaction < 0){
			phrase = "Yuck!";
			nausea += 30;
		}

		// if we can speak, say the thing
		if (speech && phrase != ""){
			speech.Say(phrase);
		}

		if (nutrition > 50){
			speech.Say("I'm full!");
		}

		if (nutrition > 75){
			speech.Say("I can't eat another bite!");
		}

		if (intrinsics){
			Intrinsics foodIntrinsic = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(food.gameObject);
			intrinsics.AddIntrinsic(foodIntrinsic);
		}
        
        // set up an occurrence flag for this eating!
        Occurrence flag = Toolbox.Instance.OccurenceFlag(gameObject);
        OccurrenceEat eatData = new OccurrenceEat();
        eatData.food = food.name;
        eatData.amount = food.nutrition;
        MonoLiquid mliquid = food.GetComponent<MonoLiquid>();
        if (mliquid){
            eatData.liquid = mliquid.liquid;
            if (mliquid.liquid.vomit){
                eatData.disgusting += 100f;
                eatData.chaos += 200f;
            }
            if (mliquid.liquid.offal){
                eatData.disgusting += 75f;
                eatData.chaos += 50f;
            }
            if (mliquid.liquid.immoral){
                eatData.disturbing += 100f;
                eatData.chaos += 150f;
            }
        }
        flag.data.Add(eatData);

		food.BeEaten();

	}

	void Vomit(){
		nausea = 0;
		nutrition = 0;
                
        Occurrence flag = Toolbox.Instance.OccurenceFlag(gameObject);
        OccurrenceVomit data = new OccurrenceVomit();
        data.disgusting = 100f;
        
		if (eaten){
            data.vomit = eaten.name;
			eaten.SetActive(true);
			eaten.transform.position = transform.position;
            MonoLiquid mono = eaten.GetComponent<MonoLiquid>();
            if (mono){
                Toolbox.Instance.SpawnDroplet(transform.position, mono.liquid);
                mono.liquid.vomit = true;
                mono.edible.vomit = true;
                data.liquid = mono.liquid;
            }
            Edible edible = eaten.GetComponent<Edible>();
            if (edible){
                edible.vomit = true;
            }
		}
		if (head)
			head.SetVomit(true);
		if (speech){
			speech.Say("Blaaaaargh!");
		}

        flag.data.Add(data);
	}
}
