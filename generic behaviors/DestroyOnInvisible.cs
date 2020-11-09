using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DestroyOnInvisible : MonoBehaviour {
    Camera renderingCamera;
    public void Start() {
        renderingCamera = GameManager.Instance.cam;
        if (renderingCamera == null) {
            renderingCamera = FindObjectOfType<Camera>();
        }
    }
    public void Update() {
        Vector2 initLocation = (Vector2)transform.position;
        Vector2 initPosition = renderingCamera.WorldToScreenPoint(initLocation);
        if (initPosition.x < 0 || initPosition.y < 0 || initPosition.x > renderingCamera.pixelWidth || initPosition.y > renderingCamera.pixelHeight) {
            Destroy(gameObject);
            UINew.Instance.ClearWorldButtons();
            InputController.Instance.ResetLastLeftClicked();
        }
    }
}