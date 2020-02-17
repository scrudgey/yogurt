using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Easings;
using UnityEngine.EventSystems;

public partial class UINew : Singleton<UINew> {
    public enum MenuType {
        none, escape, inventory, speech, closet,
        scriptSelect, commercialReport, newDayReport, email,
        diary, dialogue, phone, perk, teleport, tv, perkBrowser
    }
    private Dictionary<MenuType, string> menuPrefabs = new Dictionary<MenuType, string>{
        {MenuType.escape,                   "UI/PauseMenu"},
        {MenuType.inventory,                "UI/InventoryScreen"},
        {MenuType.speech,                   "UI/SpeechMenu"},
        {MenuType.closet,                   "UI/ClosetMenu"},
        {MenuType.scriptSelect,             "UI/ScriptSelector"},
        {MenuType.commercialReport,         "UI/commercialReport"},
        {MenuType.newDayReport,             "UI/NewDayReport"},
        {MenuType.email,                    "UI/EmailUI"},
        {MenuType.diary,                    "UI/Diary"},
        {MenuType.dialogue,                 "UI/DialogueMenu"},
        {MenuType.phone,                    "UI/PhoneMenu"},
        {MenuType.perk,                     "UI/PerkMenu"},
        {MenuType.teleport,                 "UI/TeleportMenu"},
        {MenuType.tv,                       "UI/TVMenu"},
        {MenuType.perkBrowser,              "UI/PerkBrowser"}
    };

    private static List<MenuType> actionRequired = new List<MenuType> { MenuType.commercialReport, MenuType.diary, MenuType.perk, MenuType.dialogue };
    public GameObject activeMenu;
    private MenuType activeMenuType;


    public GameObject ShowMenu(MenuType typeMenu) {
        if (activeMenu == null) {
            activeMenuType = MenuType.none;
        }
        if (activeMenuType == typeMenu) {
            CloseActiveMenu();
            return null;
        }
        if (Controller.Instance.state == Controller.ControlState.waitForMenu)
            return null;
        CloseActiveMenu();
        activeMenu = GameObject.Instantiate(Resources.Load(menuPrefabs[typeMenu])) as GameObject;
        Canvas canvas = activeMenu.GetComponent<Canvas>();
        canvas.worldCamera = GameManager.Instance.cam;
        activeMenuType = typeMenu;
        if (actionRequired.Contains(typeMenu)) {
            Controller.Instance.state = Controller.ControlState.waitForMenu;
        } else {
            Controller.Instance.state = Controller.ControlState.inMenu;
        }
        Time.timeScale = 0f;
        return activeMenu;
    }
    public void CloseActiveMenu() {
        if (activeMenu) {
            activeMenuType = MenuType.none;
            Destroy(activeMenu);
            activeMenu.SendMessage("OnDestroy", options: SendMessageOptions.DontRequireReceiver);
            Time.timeScale = 1f;
            Controller.Instance.MenuClosedCallback();
        }
    }


    // TODO: why is this stuff separate?
    public void ShowInventoryMenu() {
        GameObject inventoryMenu = ShowMenu(MenuType.inventory);
        if (inventoryMenu != null) {
            Inventory inventory = GameManager.Instance.playerObject.GetComponent<Inventory>();
            InventoryMenu menu = inventoryMenu.GetComponent<InventoryMenu>();
            menu.Initialize(inventory);
        }
    }
}