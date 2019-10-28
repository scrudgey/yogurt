using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MayorLock : Interactive {
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.transform.root.GetComponentInChildren<MayorKey>() != null) {
            Unlock();
        }
    }
    public virtual void Unlock() {
        Toolbox.Instance.AudioSpeaker("8-bit/BOUNCE1", transform.position);
        Destroy(gameObject);
    }
    public virtual bool Unlockable() {
        return true;
    }
}
