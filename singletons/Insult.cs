using Nimrod;
using UnityEngine;

public class Insults {
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

    public static Grammar InsultGrammar(GameObject target){
        Grammar g = Grammar.ObjectToGrammar(target);
        g.Load("insult");

        if (g.Parse("{target-item}") != "none"){
            g.Load("insult_item");
            // flavor
        }
        if (g.Parse("{target-clothes}") != "none"){
            g.Load("insult_clothes");
            // flavor
        }
        if (g.Parse("{target-hat}") != "none"){
            g.Load("insult_hat");
            // flavor
        }

        return g;
    }
}