using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recruiter : Interactive {
    Speech mySpeech;
    public static GameObject target;
    void Start() {
        mySpeech = GetComponent<Speech>();
        Interaction speak = new Interaction(this, "Recruit", "Recruit");
        speak.unlimitedRange = true;
        speak.validationFunction = true;
        // speak.otherOnSelfConsent = false; 
        speak.selfOnSelfConsent = false;
        speak.holdingOnOtherConsent = false;
        speak.dontWipeInterface = false;
        interactions.Add(speak);
    }
    public void Recruit(Speech other) {
        target = other.gameObject;
        UINew.Instance.RefreshUI();
        GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.dialogue);
        DialogueMenu dialogue = menuObject.GetComponent<DialogueMenu>();
        dialogue.menuClosed += RecruitCallback;
        dialogue.RecruitAsk(other);
    }
    public bool Recruit_Validation(Speech other) {
        return GameManager.Instance.data.state == GameState.ceoPlus;
    }
    public string Recruit_desc(Speech other) {
        string theirName = Toolbox.Instance.GetName(other.gameObject);
        return $"Recruit {theirName}...";
    }
    public static void RecruitCallback() {
        string newName = Toolbox.Instance.GetName(target);

        GameManager.Instance.data = GameManager.Instance.InitializedGameData();
        GameManager.Instance.data.state = GameState.normal;
        GameManager.Instance.data.defaultGender = Toolbox.GetGender(target);
        GameManager.Instance.data.defaultSkinColor = Toolbox.GetSkinColor(target);
        GameManager.Instance.data.prefabName = Toolbox.GetPrefabPath(target).Split('/')[1];
        Debug.Log($"recruiting {GameManager.Instance.data.prefabName}");

        PersonRandomizer randomizer = target.GetComponent<PersonRandomizer>();
        if (randomizer != null) {
            PrefabFromRandomCharacter();
        }

        GameManager.Instance.saveGameName = newName;
        GameManager.Instance.SaveGameData();
        GameManager.Instance.NewGame();
    }

    static void PrefabFromRandomCharacter() {
        Gender selectedGender = Toolbox.GetGender(target);
        SkinColor selectedSkinColor = Toolbox.GetSkinColor(target);

        if (selectedGender == Gender.male) {
            switch (selectedSkinColor) {
                default:
                case SkinColor.light:
                    GameManager.Instance.data.prefabName = "Tom";
                    break;
                case SkinColor.dark:
                    GameManager.Instance.data.prefabName = "Brm";
                    break;
                case SkinColor.darker:
                    GameManager.Instance.data.prefabName = "Blm";
                    break;
            }
        } else {
            switch (selectedSkinColor) {
                default:
                case SkinColor.light:
                    GameManager.Instance.data.prefabName = "Tina";
                    break;
                case SkinColor.dark:
                    GameManager.Instance.data.prefabName = "Brf";
                    break;
                case SkinColor.darker:
                    GameManager.Instance.data.prefabName = "Blf";
                    break;
            }
        }
    }
}
