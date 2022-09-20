using UnityEngine;

public class RaycastTester : MonoBehaviour
{
	public float maxDistance;
	public LineRenderer line;

	RaycastHit hit;

	void Update()
	{
		Vector3 targetPos;

		if (Physics.Raycast(transform.position, transform.forward, out hit))
			targetPos = hit.point;
		else
			targetPos = transform.forward * maxDistance;

		line.SetPositions(new Vector3[]
		{
			transform.position,
			targetPos
		});
	}
}