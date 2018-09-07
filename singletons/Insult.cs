using Nimrod;
using UnityEngine;

public class Insults {
    public static string ComposeInsult(GameObject insultTarget) {
        GameObject target = Controller.Instance.GetBaseInteractive(insultTarget.transform);
        Grammar grammar = TargetToNimrod(target);
        return grammar.Parse("{main}");
    }

    public static string RespondToInsult(PersonalAssessment assessment, Personality personality) {
        // consider: insulter status
        // what am i doing? acting? selling?
        // stoicism
        // PersonalAssessment.friendStatus status = assessment.status;
        // switch (personality.stoicism) {
        //     case (Personality.Stoicism.neutral):
        //         break;
        //     case (Personality.Stoicism.fragile):
        //         break;
        //     case (Personality.Stoicism.noble):
        //         break;
        //     default:
        //         break;
        // }
        return "";
    }

    public static Grammar TargetToNimrod(GameObject target){
        Grammar g = new Grammar();
        // DecisionMaker targetDM = target.GetComponent<DecisionMaker>();
        // if (targetDM != null) {
        //     if (targetDM.possession != null) {
        //         possessionName = targetDM.possession.name;
        //         g.Load("insult_item");
        //         g.AddSymbol("item-name", possessionName);
        //     }
        // }

        // Outfit targetOutfit = target.GetComponent<Outfit>();
        // if (targetOutfit != null) {
        //     clothesName = targetOutfit.wornUniformName;
        //     grammar.Load("insult_clothes");
        //     grammar.AddSymbol("clothes-name", clothesName);
        // }

        // Head targetHead = target.GetComponentInChildren<Head>();
        // if (targetHead != null) {
        //     if (targetHead.hat != null) {
        //         hatName = targetHead.hat.name;
        //         grammar.Load("insult_hat");
        //         grammar.AddSymbol("hat-name", hatName);
        //     }
        // }

        return g;
    }
}