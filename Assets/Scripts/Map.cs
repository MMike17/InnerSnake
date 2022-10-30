using UnityEngine;
using static MapsManager;

/// <summary>Represents a playable game map</summary>
public class Map : MonoBehaviour
{
	[Header("Settings")]
	public MapSize size;

	[Header("Scene references")]
	public Transform gridPoints;
	public Transform mesh;

	public float Radius => transform.localScale.x;

	void OnDrawGizmos()
	{
		if (gridPoints == null)
			return;

		Gizmos.color = new Color(1, 1, 0.5f, 0.5f);

		foreach (Transform point in gridPoints)
			Gizmos.DrawSphere(point.position, 0.5f);
	}
}