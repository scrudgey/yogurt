public enum ImpactResult {normal, repel, strong}

public interface IDamageable {
    ImpactResult TakeDamage(MessageDamage message);
}