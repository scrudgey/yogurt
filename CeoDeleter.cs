using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeoDeleter : MonoBehaviour {

    void Start() {
        if (GameManager.Instance.data.ceoFled) {
            Destroy(gameObject);
        }
    }

}
