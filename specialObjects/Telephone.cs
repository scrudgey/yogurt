using UnityEngine;

public class Telephone : Item {
    AudioSource source;
    public Doorway enterDoor;
    public float fireTime;
    public float clownTime;
    public AudioClip phoneUp;
    public AudioClip phoneDown;
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
            MessageSpeech message = new MessageSpeech();
            message.phrase = "We'll send someone over right away!";
            Toolbox.Instance.SendMessage(gameObject, this, message);
            fireTime = 10f;
        } else {
            MessageSpeech message = new MessageSpeech();
            message.phrase = "Just sit tight, the team is on their way!";
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
    }
    public void ClownCallback() {
        if (fireTime <= 0) {
            MessageSpeech message = new MessageSpeech();
            message.phrase = "Ho ho! Sit tight! We have dispatched a clown to your location!";
            Toolbox.Instance.SendMessage(gameObject, this, message);
            clownTime = 10f;
        } else {
            MessageSpeech message = new MessageSpeech();
            message.phrase = "Brother clown is with you this day!";
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
    }
    public void MenuCallback(PhoneNumberButton.phoneNumber type) {
        switch (type) {
            case PhoneNumberButton.phoneNumber.fire:
                FireButtonCallback();
                break;
            case PhoneNumberButton.phoneNumber.clown:
                ClownCallback();
                break;
            default:
                break;
        }
    }
}
