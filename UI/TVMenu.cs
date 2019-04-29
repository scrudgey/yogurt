using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TVMenu : MonoBehaviour {
    public Text subtitle;
    public Image image;
    public void ChannelCallback(int number) {
        Debug.Log(number);
    }
    public void PowerButtonCallback() {
        UINew.Instance.CloseActiveMenu();
    }
}
