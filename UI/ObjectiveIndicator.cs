using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveIndicator : MonoBehaviour {
    public Image checkbox;
    public TextMeshProUGUI description;
    public Sprite unfinishedSprite;
    public Sprite finishedSprite;
    // public CommercialProperty targetProperty;
    public string location;
    public Objective objective;

    public void UpdateCheck() {
        SetCheck(objective.RequirementsMet(GameManager.Instance.data.activeCommercial));
    }
    public void SetCheck(bool value) {
        if (value) {
            checkbox.sprite = finishedSprite;
        } else {
            checkbox.sprite = unfinishedSprite;
        }
    }
}
