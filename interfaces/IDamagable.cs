// using UnityEngine;
// using System.Collections;

public interface IDamagable
{
	void TakeDamage(damageType type , float damage);
	void IntrinsicsChanged(Intrinsic intrinsic);
}