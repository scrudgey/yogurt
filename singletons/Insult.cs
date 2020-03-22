using Nimrod;
using UnityEngine;

public class Insult {
    public static Grammar ObjectToGrammar(GameObject target) {
        Grammar g = new Grammar();
        g.Load("insult");

        g.AddSymbol("target-item", "none");
        g.AddSymbol("target-clothes", "none");
        g.AddSymbol("target-hat", "none");

        // insult possessions
        DecisionMaker targetDM = target.GetComponent<DecisionMaker>();
        if (targetDM != null) {
            if (targetDM.possession != null) {
                string possessionName = targetDM.possession.name;
                g.SetSymbol("target-item", possessionName);
                g.Load("insult_item");
            }
        }

        // insult outfit
        Outfit targetOutfit = target.GetComponent<Outfit>();
        if (targetOutfit != null) {
            g.SetSymbol("target-clothes", targetOutfit.readableUniformName);
            g.SetSymbol("target-clothes-plural", targetOutfit.pluralUniformType);
            g.Load("insult_clothes");
        }

        // insult hat
        Head targetHead = target.GetComponentInChildren<Head>();
        if (targetHead != null) {
            if (targetHead.hat != null) {
                string hatName = targetHead.hat.name;
                g.SetSymbol("target-hat", hatName);
                g.Load("insult_hat");
            }
        }

        return g;
    }
    public static string ComposeInsult(GameObject insultTarget) {
        GameObject target = Controller.Instance.GetBaseInteractive(insultTarget.transform);
        Grammar grammar = InsultGrammar(target);
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

    public static Grammar InsultGrammar(GameObject target) {
        Grammar g = Insult.ObjectToGrammar(target);
        return g;
    }
}