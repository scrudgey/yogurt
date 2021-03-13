using UnityEngine;

public class Slasher : MonoBehaviour {
    public MessageDamage message;
    void SlashOn() {
        Vector3 startPoint = transform.position;
        GameObject impactObj = GameManager.Instantiate(Resources.Load("PhysicalImpact"), startPoint, Quaternion.identity) as GameObject;
        float angle = Toolbox.Instance.ProperAngle(message.force.x, message.force.y);
        impactObj.transform.RotateAround(impactObj.transform.position, new Vector3(0, 0, 1), angle);
        PhysicalImpact impact = impactObj.GetComponent<PhysicalImpact>();
        impact.useBoxCollider = true;
        impact.size = 0.11f;
        impact.message = message;
        CircleCollider2D circleCollider = impact.GetComponent<CircleCollider2D>();
        BoxCollider2D boxCollider = impact.GetComponent<BoxCollider2D>();
        foreach (Collider2D responsibleCollider in message.responsibleParty.GetComponentsInChildren<Collider2D>()) {
            Physics2D.IgnoreCollision(circleCollider, responsibleCollider, true);
            Physics2D.IgnoreCollision(boxCollider, responsibleCollider, true);
        }
        // Debug.Break();
    }
    void SlashOff() {
        // currently empty but referred to in animation
    }
    void SlashEnd() {
        ClaimsManager.Instance.WasDestroyed(gameObject);
        Destroy(gameObject);
    }
}
