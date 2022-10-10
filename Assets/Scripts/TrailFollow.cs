using System.Collections.Generic;
using UnityEngine;

/// <summary>Animates the trail following</summary>
public class TrailFollow : MonoBehaviour
{
	// TODO : merge this into fake player
	[Header("Settings")]
	public int maxTrailLength;
	public float drawDelay;

	[Header("Scene references")]
	public LineRenderer line;

	float timer;

	void Update()
	{
		timer += Time.deltaTime;

		if (timer >= drawDelay)
		{
			timer = 0;

			Vector3[] positions = new Vector3[line.positionCount];
			line.GetPositions(positions);

			List<Vector3> newPos = new List<Vector3>(positions);

			if (newPos.Count >= maxTrailLength)
				newPos.RemoveAt(newPos.Count - 1);

			newPos.Insert(0, transform.position);
			line.positionCount = newPos.Count;
			line.SetPositions(newPos.ToArray());
		}
		else if (line.positionCount > 0)
			line.SetPosition(0, transform.position);
	}
}