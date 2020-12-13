using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
public class Telephone : Item {
    AudioSource source;
    public Doorway enterDoor;
    public float fireTime;
    public float clownTime;
    public float pizzaTime;
    public AudioClip phoneUp;
    public AudioClip phoneDown;
    public List<AudioClip> dialSounds;
    public AudioClip ringSound;
    public bool isRinging;
    public AnimateUIBubble newBubble;
    public string incomingCall;
    void Awake() {
        source = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    void Start() {
        string sceneName = SceneManager.GetActiveScene().name;
        Interaction use = new Interaction(this, "Phone", "UsePhone");
        interactions.Add(use);

        if (newBubble == null)
            newBubble = GetComponent<AnimateUIBubble>();
        newBubble.DisableFrames();
        CheckBubble();

        if (sceneName == "apartment") {
            if (GameManager.Instance.data != null && GameManager.Instance.data.phoneQueue.Count > 0) {
                PhoneCall(GameManager.Instance.data.phoneQueue[0]);
            }
        }
    }
    public void CheckBubble() {
        if (newBubble == null)
            newBubble = GetComponent<AnimateUIBubble>();
        if (isRinging) {
            newBubble.EnableFrames();
        } else {
            newBubble.DisableFrames();
        }
    }
    public void UsePhone() {
        if (phoneUp != null)
            source.PlayOneShot(phoneUp);
        if (isRinging) {
            AnswerPhone();
            return;
        }
        GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.phone);
        PhoneMenu menu = menuObject.GetComponent<PhoneMenu>();
        menu.PopulateList();
        menu.telephone = this;
    }
    public void AnswerPhone() {
        MySaver.Save();
        isRinging = false;
        CheckBubble();
        // switch on cutscene type
        GameManager.Instance.data.phoneQueue.Remove(incomingCall);
        if (incomingCall == "office") {
            GameManager.Instance.OfficeCutscene();
        } else if (incomingCall == "airplane") {
            GameManager.Instance.AirplaneCutscene();
        } else if (incomingCall == "bar") {
            GameManager.Instance.BarCutscene();
        }
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

    public void PhoneCall(string cutscene) {
        incomingCall = cutscene;
        isRinging = true;
        CheckBubble();
        StartCoroutine(Ring());
    }
    public IEnumerator Ring() {
        Vector3 initialPosition = transform.position;
        float timer = 0;
        while (isRinging) {
            if (timer <= 0) {
                // do ring
                source.clip = ringSound;
                source.Play();
            }
            timer += Time.deltaTime;
            if (timer < 5f) {
                // do shake
                if (source.isPlaying) {
                    transform.position = (Vector2)initialPosition + (Random.insideUnitCircle / 100f);
                }
            } else {
                timer = 0;

            }
            yield return null;
        }
    }
}
