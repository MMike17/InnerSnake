using UnityEngine;

public class TEST_Pickup : MonoBehaviour
{
	public Pickup[] pickups;

	void Awake()
	{
		foreach (Pickup pickup in pickups)
			pickup.Init(Vector3.zero, 0, 1);
	}
}