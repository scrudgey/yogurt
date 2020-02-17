using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBubble : MonoBehaviour {
    public string mouseOverText = "Inspect new items";
    public void MouseOver() {
        UINew.Instance.actionButtonText = mouseOverText;
    }
}
