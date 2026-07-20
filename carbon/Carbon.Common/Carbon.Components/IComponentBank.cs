using UnityEngine;

namespace Carbon.Components;

public interface IComponentBank
{
	bool Remove(GameObject go, bool destroy = true);

	bool Destroy(GameObject go);
}
