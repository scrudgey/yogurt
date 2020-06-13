using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;

public class SpiritCutsceneTrigger : MonoBehaviour {
    public Transform spiritSpawnPoint;
    public GameObject spiritPrefab;
    GameObject spirit;
    public Pickup knife;
    void Update() {
        if (knife.holder != null && spirit == null) {
            spirit = GameObject.Instantiate(spiritPrefab, spiritSpawnPoint.position, Quaternion.identity) as GameObject;
            DecisionMaker ai = spirit.GetComponent<DecisionMaker>();
            PersonalAssessment assessment = ai.awareness.FormPersonalAssessment(GameManager.Instance.playerObject);
            assessment.status = PersonalAssessment.friendStatus.enemy;
            AudioClip teleportEnter = Resources.Load("sounds/clown/clown4") as AudioClip;
            GameManager.Instance.PlayPublicSound(teleportEnter);
            GameObject.Instantiate(Resources.Load("particles/ChelaPoof"), spiritSpawnPoint.position, Quaternion.identity);
            StartCoroutine(WaitAndStart());
        }
    }

    IEnumerator WaitAndStart() {
        Controllable playerControllable = InputController.Instance.focus;
        Controllable spiritControllable = spirit.GetComponent<Controllable>();
        using (Controller controller = new Controller(playerControllable)) {
            using (Controller spiritController = new Controller(spiritControllable)) {
                yield return new WaitForSeconds(2f);
                Destroy(gameObject);
            }
        }
    }
}
