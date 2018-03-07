using UnityEngine;

public class Slasher : MonoBehaviour {
    public MessageDamage message;
    void SlashOn() {
        Vector3 startPoint = transform.position;
        startPoint.x += message.force.normalized.x / 4f;
        startPoint.y += message.force.normalized.y / 4f;
        GameObject impactObj = GameManager.Instantiate(Resources.Load("PhysicalImpact"), startPoint, Quaternion.identity) as GameObject;
        PhysicalImpact impact = impactObj.GetComponent<PhysicalImpact>();
        impact.size = 0.11f;
        impact.message = message;
        CircleCollider2D impactCollider = impact.GetComponent<CircleCollider2D>();
        foreach (Collider2D responsibleCollider in message.responsibleParty.GetComponentsInChildren<Collider2D>())
            Physics2D.IgnoreCollision(impactCollider, responsibleCollider, true);
    }
    void SlashOff() {
        // currently empty but referred to in animation
    }
    void SlashEnd() {
        ClaimsManager.Instance.WasDestroyed(gameObject);
        Destroy(gameObject);
    }
}
