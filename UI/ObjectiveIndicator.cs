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

    public void UpdateCheck(Commercial localCommercial) {
        CommercialProperty localProperty = null;
        localCommercial.properties.TryGetValue(targetProperty.key, out localProperty);
        if (localProperty == null) {
            SetCheck(false);
        } else {
            SetCheck(localProperty.RequirementMet(targetProperty));
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
