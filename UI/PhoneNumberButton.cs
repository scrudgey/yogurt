// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PhoneNumberButton : MonoBehaviour {
    public PhoneMenu menu;
    public enum phoneNumber { fire, police, clown }
    public phoneNumber number;
    public Image image;
    public void Clicked() {
        menu.PhoneButtonCallback(this);
    }
    public void Start() {
        image = GetComponent<Image>();
    }
    public void SetPhoneType(phoneNumber number) {
        Text display = transform.Find("Text").GetComponent<Text>();
        this.number = number;
        switch (number) {
            case phoneNumber.fire:
                display.text = "fire";
                break;
            case phoneNumber.police:
                display.text = "police";
                break;
            case phoneNumber.clown:
                display.text = "clown";
                break;
            default:
                break;
        }
    }
}
