using UnityEngine;

public class MayorKey : Interactive {
    void Awake() {
        Interaction eatAction = new Interaction(this, "Unlock", "Unlock");
        // eatAction.otherOnPlayerConsent = false;
        eatAction.validationFunction = true;
        eatAction.descString = "Unlock";
        // eatAction.debug = true;
        interactions.Add(eatAction);
    }
    public void Unlock(MayorLock mayorlock) {
        mayorlock.Unlock();
    }
    public bool Unlock_Validation(MayorLock mayorLock) {
        return mayorLock.Unlockable();
    }
}
