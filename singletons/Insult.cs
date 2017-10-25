using Nimrod;
using UnityEngine;
// using System.Collections.Generic;

public class Insult {
	public static void DoInsult(GameObject insulter, GameObject insulted){
		string content = ComposeInsult(insulted);
		MessageSpeech message = new MessageSpeech(content);
		Toolbox.Instance.SendMessage(insulter, null, message);

		MessageInsult messageInsult = new MessageInsult();
		Toolbox.Instance.SendMessage(insulted, insulter.GetComponent<Component>(), messageInsult);

		OccurrenceSpeech of = new OccurrenceSpeech();
		of.target = Toolbox.Instance.CloneRemover(insulted.name);
		of.speaker = Toolbox.Instance.CloneRemover(insulter.name);
		of.insult = true;
		// of.chaos = 20f;
		// of.offensive = Random.Range(20, 30);
		// of.disturbing = 2f;
		Toolbox.Instance.OccurenceFlag(insulter, of);
	}
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
