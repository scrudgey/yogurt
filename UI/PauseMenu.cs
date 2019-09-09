using UnityEngine;
public class PauseMenu : MonoBehaviour {
    public void Start() {
        Canvas menuCanvas = GetComponent<Canvas>();
        menuCanvas.worldCamera = GameManager.Instance.cam;
    }
    public void SaveAndQuitClick() {
        MySaver.Save();
        MySaver.SaveObjectDatabase();
        UINew.Instance.CloseActiveMenu();
        GameManager.Instance.TitleScreen();
    }
    public void QuitClick() {
        UINew.Instance.CloseActiveMenu();
        GameManager.Instance.TitleScreen();
    }
    public void ContinueClick() {
        UINew.Instance.CloseActiveMenu();
    }
    public void SaveClick() {
        MySaver.Save();
        MySaver.SaveObjectDatabase();
        UINew.Instance.CloseActiveMenu();
    }
    public void PerkClick() {
        UINew.Instance.CloseActiveMenu();
        UINew.Instance.ShowMenu(UINew.MenuType.perkBrowser);
    }
}
