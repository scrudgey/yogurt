// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PhoneNumberButton : MonoBehaviour {
    public PhoneMenu menu;
    public enum phoneNumber { fire, police, clown, pizza }
    public phoneNumber number;
    public Image image;
    public Text display;
    public Text numberText;
    public void Clicked() {
        menu.PhoneButtonCallback(this);
    }
    public void Start() {
        image = GetComponent<Image>();
    }
    public void SetPhoneType(phoneNumber number) {
        this.number = number;
        switch (number) {
            case phoneNumber.fire:
                display.text = "fire";
                numberText.text = "911";
                break;
            case phoneNumber.police:
                display.text = "police";
                numberText.text = "911";
                break;
            case phoneNumber.clown:
                display.text = "clown";
                numberText.text = "555-6294";
                break;
            case phoneNumber.pizza:
                display.text = "pizza";
                numberText.text = "555-2717";
                break;
            default:
                break;
        }
    }
}
