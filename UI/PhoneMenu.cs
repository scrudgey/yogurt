// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneMenu : MonoBehaviour {
    AudioSource source;
    public AudioClip dialTone;
    public AudioClip phoneDown;
    public Transform numbersList;
    public Telephone telephone;
    public Color defaultButtonColor;
    public Color highlightButtonColor;
    public Color disabledButtonColor;
    private PhoneNumberButton selectedButton;
    private Image callButtonImage;
    private Button callButton;
    public List<Button> builtInButtons;
    public UIButtonEffects effects;
    void Start() {
        effects = GetComponent<UIButtonEffects>();
        numbersList = transform.Find("main/Numbers");
        callButtonImage = transform.Find("main/ButtonBar/Call").GetComponent<Image>();
        callButton = transform.Find("main/ButtonBar/Call").GetComponent<Button>();
        callButton.interactable = false;
        callButtonImage.color = disabledButtonColor;
        source = Toolbox.Instance.SetUpAudioSource(gameObject);
        source.spatialBlend = 0;
        selectedButton = null;
    }
    public void CloseButton() {
        UINew.Instance.CloseActiveMenu();
        if (phoneDown != null)
            Toolbox.Instance.AudioSpeaker(phoneDown, telephone.transform.position);
    }
    public void AddNumber(PhoneNumberButton.phoneNumber type) {
        GameObject newEntry = Instantiate(Resources.Load("UI/PhoneNumberButton")) as GameObject;
        newEntry.transform.SetParent(numbersList, false);
        PhoneNumberButton buttonScript = newEntry.GetComponent<PhoneNumberButton>();
        buttonScript.menu = this;
        buttonScript.SetPhoneType(type);
        effects.buttons.Add(newEntry.GetComponent<Button>());
    }
    public void PopulateList() {
        Start();
        effects.buttons = new List<Button>(builtInButtons);
        AddNumber(PhoneNumberButton.phoneNumber.fire);
        AddNumber(PhoneNumberButton.phoneNumber.clown);
        AddNumber(PhoneNumberButton.phoneNumber.pizza);
        effects.Configure();
    }
    public void PhoneButtonCallback(PhoneNumberButton button) {
        if (selectedButton != null) {
            Image selectedImage = selectedButton.GetComponent<Image>();
            selectedImage.color = defaultButtonColor;
        }
        selectedButton = button;
        callButtonImage.color = highlightButtonColor;
        callButton.interactable = true;
        Image newImage = selectedButton.GetComponent<Image>();
        newImage.color = highlightButtonColor;
    }
    public void CallButtonCallback() {
        if (selectedButton == null) {
            Debug.Log("no selected");
            return;
        }
        telephone.MenuCallback(selectedButton.number);
        CloseButton();
    }
}
