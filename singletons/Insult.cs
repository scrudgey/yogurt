using Nimrod;
using UnityEngine;

public class Insults {
	public static string ComposeInsult(GameObject inTarget){
		Grammar grammar = new Grammar();
		grammar.Load("insult");
		GameObject target = Controller.Instance.GetBaseInteractive(inTarget.transform);
		Outfit targetOutfit = target.GetComponent<Outfit>();
		Head targetHead = target.GetComponentInChildren<Head>();
		DecisionMaker targetDM = target.GetComponent<DecisionMaker>();
		string possessionName = null;
		string clothesName = "";
		string hatName = "";
		if (targetDM != null){
			if(targetDM.possession != null){
				possessionName = targetDM.possession.name;
				grammar.Load("insult_item");
				grammar.AddSymbol("item-name", possessionName);
			}
		}
		if (targetOutfit != null){
			clothesName = targetOutfit.wornUniformName;
			grammar.Load("insult_clothes");
			grammar.AddSymbol("clothes-name", clothesName);
		}
		if (targetHead != null){
			if (targetHead.hat != null){
				hatName = targetHead.hat.name;
				grammar.Load("insult_hat");
				grammar.AddSymbol("hat-name", hatName);
			}
		}
		return grammar.Parse("{main}");
	}
	
	public static string RespondToInsult(PersonalAssessment assessment, Personality personality){
		// consider: insulter status
		// what am i doing? acting? selling?
		// stoicism
		PersonalAssessment.friendStatus status = assessment.status;
		switch(personality.stoicism){
			case (Personality.Stoicism.neutral):
			break;
			case (Personality.Stoicism.fragile):
			break;
			case (Personality.Stoicism.noble):
			break;
			default:
			break;
		}
		return "";
	}
}