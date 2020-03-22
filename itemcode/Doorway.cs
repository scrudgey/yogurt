using UnityEngine;
public class Doorway : Interactive {
    public string destination;
    public int destinationEntry;
    public int entryID;
    public bool spawnPoint = false;
    public AudioClip leaveSound;
    public AudioClip enterSound;
    protected AudioSource audioSource;
    public string leaveDesc;
    public string actionDesc = "Exit";
    public Transform enterPoint;
    public bool smallEntrance;
    public virtual void Awake() {
        Interaction leaveaction = new Interaction(this, actionDesc, "Leave");
        interactions.Add(leaveaction);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public virtual void Enter(GameObject player) {
        Vector3 tempPos = transform.position;
        if (enterPoint) {
            tempPos = enterPoint.position;
        } else {
            tempPos.y = tempPos.y - 0.05f;
        }
        player.transform.position = tempPos;
        PlayEnterSound();
    }
    public void Leave() {
        if (smallEntrance) {
            Inventory playerInventory = GameManager.Instance.playerObject.GetComponent<Inventory>();
            if (playerInventory) {
                if (playerInventory.holding) {
                    if (playerInventory.holding.heavyObject) {
                        MessageSpeech message = new MessageSpeech("I can't fit holding this!");
                        Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
                        return;
                    }
                }
            }
        }
        if (leaveSound != null)
            GameManager.Instance.publicAudio.PlayOneShot(leaveSound);
        GameManager.Instance.LeaveScene(destination, destinationEntry);
    }
    public void PlayEnterSound() {
        if (audioSource == null) {
            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        }
        if (enterSound != null) {
            audioSource.PlayOneShot(enterSound);
        }
    }
    public void PlayExitSound() {
        if (audioSource == null) {
            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        }
        if (enterSound != null) {
            audioSource.PlayOneShot(leaveSound);
        }
    }
    public string Leave_desc() {
        if (leaveDesc != "") {
            return leaveDesc;
        } else {
            return "Go to " + destination;
        }
    }
}
