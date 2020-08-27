using UnityEngine;
using System.Collections.Generic;
public class Telephone : Item {
    AudioSource source;
    public Doorway enterDoor;
    public float fireTime;
    public float clownTime;
    public float pizzaTime;
    public AudioClip phoneUp;
    public AudioClip phoneDown;
    public List<AudioClip> dialSounds;
    void Start() {
        source = Toolbox.Instance.SetUpAudioSource(gameObject);
        Interaction use = new Interaction(this, "Phone", "UsePhone");
        interactions.Add(use);
    }
    public void UsePhone() {
        if (phoneUp != null)
            source.PlayOneShot(phoneUp);
        GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.phone);
        PhoneMenu menu = menuObject.GetComponent<PhoneMenu>();
        menu.PopulateList();
        menu.telephone = this;
    }
    public string UsePhone_desc() {
        return "Use telephone";
    }
    public void FireButtonCallback() {
        if (fireTime <= 0) {
            MessageSpeech message = new MessageSpeech("We'll send someone over right away!");
            Toolbox.Instance.SendMessage(gameObject, this, message);
            fireTime = 10f;
        } else {
            MessageSpeech message = new MessageSpeech("Just sit tight, the team is on their way!");
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
    }
    public void ClownCallback() {
        if (clownTime <= 0) {
            MessageSpeech message = new MessageSpeech("Ho ho! Sit tight! We have dispatched a clown to your location!");
            Toolbox.Instance.SendMessage(gameObject, this, message);
            clownTime = 10f;
        } else {
            MessageSpeech message = new MessageSpeech("Brother clown is with you this day!");
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
    }
    public void PizzaCallback() {
        if (pizzaTime <= 0) {
            MessageSpeech message = new MessageSpeech("One large pizza, coming up!");
            Toolbox.Instance.SendMessage(gameObject, this, message);
            pizzaTime = 10f;
        } else {
            MessageSpeech message = new MessageSpeech("Chill out buddy, the pizza's on its way! Chill!");
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
    }
    public void Update() {
        if (fireTime > 0) {
            fireTime -= Time.deltaTime;
            if (fireTime <= 0) {
                //do fireman spawn
                Vector3 tempPos = enterDoor.transform.position;
                tempPos.y = tempPos.y - 0.05f;
                Instantiate(Resources.Load("prefabs/Fireman"), tempPos, Quaternion.identity);
                enterDoor.PlayEnterSound();
            }
        }
        if (clownTime > 0) {
            clownTime -= Time.deltaTime;
            if (clownTime <= 0) {
                //do clown spawn
                Vector3 tempPos = enterDoor.transform.position;
                tempPos.y = tempPos.y - 0.05f;
                Instantiate(Resources.Load("prefabs/Clown"), tempPos, Quaternion.identity);
                enterDoor.PlayEnterSound();
            }
        }
        if (pizzaTime > 0) {
            pizzaTime -= Time.deltaTime;
            if (pizzaTime <= 0) {
                //do clown spawn
                Vector3 tempPos = enterDoor.transform.position;
                tempPos.y = tempPos.y - 0.05f;
                Instantiate(Resources.Load("prefabs/pizza_deliveryboy"), tempPos, Quaternion.identity);
                enterDoor.PlayEnterSound();
            }
        }
    }
    public void MenuCallback(PhoneNumberButton.phoneNumber type) {
        PlayDialSound();
        switch (type) {
            case PhoneNumberButton.phoneNumber.fire:
                FireButtonCallback();
                break;
            case PhoneNumberButton.phoneNumber.clown:
                ClownCallback();
                break;
            case PhoneNumberButton.phoneNumber.pizza:
                PizzaCallback();
                break;
            default:
                break;
        }
    }
    public void PlayDialSound() {
        source.PlayOneShot(dialSounds[Random.Range(0, dialSounds.Count)]);
    }
}
