using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

public class Eater : Interactive {
	
	public float nutrition;

	public enum preference{neutral,likes,dislikes}

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
	private Speech speech;
	private GameObject eaten;
	private bool LoadInitialized = false;

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

	// Use this for initialization
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
	}
	
	// Update is called once per frame
	void Update () {
		if (nausea > 100){
			Vomit();
		}

		if (nutrition > 100){
			nausea += Time.deltaTime * 2;

		}
	}

	public int CheckReaction(Edible food){
		int reaction = 0;
	
		//i can clean this section up with reflection-
		// might be necessary if food types get out of control
		bool[] types = new bool[] {food.vegetable,food.meat,food.immoral,food.offal};
		preference[] prefs = new preference[] {vegetablePreference,meatPreference,immoralPreference,offalPreference};

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

	public void Eat (Edible food){
		string phrase ="";
		int reaction;
		nutrition += food.nutrition;

		if (head)
			head.SetEating(true,food.pureeColor);

		//randomly store a clone of the object for later vomiting
		if(Random.Range(0,1) < 0.1){
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

		food.BeEaten();

	}

	void Vomit(){
		nausea = 0;
		nutrition = 0;
		if (eaten){
			eaten.SetActive(true);
			eaten.transform.position = transform.position;
		}

		if (head)
			head.SetVomit(true);

		if (speech){
			speech.Say("Blaaaaargh!");
		}
	}
}
