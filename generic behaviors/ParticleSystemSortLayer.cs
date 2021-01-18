using UnityEngine;
public class ParticleSystemSortLayer : MonoBehaviour {
    void Start() {
        {
            GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingLayerName = "air";
        }
    }
}
