using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveIndicator : MonoBehaviour {
    public Image checkbox;
    public Text description;
    public Sprite unfinishedSprite;
    public Sprite finishedSprite;
    public CommercialProperty targetProperty;
    public string location;

    public void UpdateCheck(Commercial localCommercial) {
        if (targetProperty != null) {
            UpdateCheckProperty(localCommercial);
        }
        if (location != "") {
            UpdateCheckLocation(localCommercial);
        }
    }
    void UpdateCheckProperty(Commercial localCommercial) {
        CommercialProperty localProperty = null;
        localCommercial.properties.TryGetValue(targetProperty.key, out localProperty);
        if (localProperty == null) {
            SetCheck(false);
        } else {
            SetCheck(localProperty.RequirementMet(targetProperty));
        }
    }
    void UpdateCheckLocation(Commercial localCommercial) {
        if (localCommercial.visitedLocations.Contains(location)) {
            SetCheck(true);
        } else {
            SetCheck(false);
        }
    }
    public void SetCheck(bool value) {
        if (value) {
            checkbox.sprite = finishedSprite;
        } else {
            checkbox.sprite = unfinishedSprite;
        }
    }
}
