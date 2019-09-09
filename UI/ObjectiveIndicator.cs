﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveIndicator : MonoBehaviour {
    public Image checkbox;
    public Text description;
    public Sprite unfinishedSprite;
    public Sprite finishedSprite;
    // public CommercialProperty targetProperty;
    public string location;
    public Objective objective;

    public void UpdateCheck(Commercial localCommercial) {
        SetCheck(objective.RequirementsMet(localCommercial));
    }
    public void SetCheck(bool value) {
        if (value) {
            checkbox.sprite = finishedSprite;
        } else {
            checkbox.sprite = unfinishedSprite;
        }
    }
}
