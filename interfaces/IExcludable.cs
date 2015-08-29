using UnityEngine;
using System.Collections;

public interface IExcludable
{
	void DropMessage(GameObject obj);
	void WasDestroyed(GameObject obj);
}