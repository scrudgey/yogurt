using UnityEngine;
public class SingletonInitializer : MonoBehaviour {
    void Start() {
        Toolbox.InitializeInstance();
        GameManager.InitializeInstance();
        ClaimsManager.InitializeInstance();
        UINew.InitializeInstance();
        CutsceneManager.InitializeInstance();
    }
}
