using UnityEngine;
using UnityEngine.Rendering;

public class OrderByY : MonoBehaviour {
    [System.Serializable]
    public struct Follower {
        public SpriteRenderer renderer;
        public int offset;
        public Follower(SpriteRenderer renderer, int offset = 1) {
            this.renderer = renderer;
            this.offset = offset;
        }
    }
    SpriteRenderer spriteRenderer;
    SortingGroup sortGroup;
    PhysicalBootstrapper bootstrapper;
    public Transform footPoint;
    void Start() {
        spriteRenderer = GetComponentInParent<SpriteRenderer>();
        sortGroup = GetComponent<SortingGroup>();
        if (footPoint == null)
            footPoint = transform.Find("footPoint");
        bootstrapper = GetComponent<PhysicalBootstrapper>();
    }
    void Update() {
        if (spriteRenderer.isVisible) {
            Vector3 position = transform.position;
            if (bootstrapper != null) {
                if (bootstrapper.physical != null) {
                    position = bootstrapper.physical.transform.position;
                }
            } else {
                if (footPoint) {
                    position = footPoint.position;
                }
            }
            int pos = Mathf.RoundToInt(position.y * 100f);
            if (sortGroup != null) {
                sortGroup.sortingOrder = (pos * -1);
            } else {
                spriteRenderer.sortingOrder = (pos * -1);
            }
        }
    }
}
