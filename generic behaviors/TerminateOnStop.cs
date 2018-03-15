using UnityEngine;

public class TerminateOnStop : MonoBehaviour {
    void Update() {
        if (GetComponent<Rigidbody2D>().velocity.magnitude < 0.01) {
            if (GetComponent<Rigidbody2D>())
                Destroy(GetComponent<Rigidbody2D>());
            if (GetComponent<Collider2D>())
                Destroy(GetComponent<Collider2D>());
            PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
            if (pb){
                pb.DestroyPhysical();
                Destroy(pb);
            }
                // Destroy(GetComponent<PhysicalBootstrapper>());
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sortingLayerName = "background";
            renderer.sortingOrder = 50;
            Destroy(this);
        }
    }
}

