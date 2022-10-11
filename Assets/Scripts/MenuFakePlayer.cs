using System.Collections.Generic;
using UnityEngine;

using static GameManager;
using Random = UnityEngine.Random;

/// <summary>Fake player used in the main menu</summary>
public class MenuFakePlayer : MonoBehaviour
{
	[Header("Settings")]
	[Range(0.1f, 0.5f)]
	public float minAnimDistance;
	public float mainAnimSpeed;
	[Space]
	public int maxTrailLength;
	public float drawDelay;

	[Header("Scene references")]
	public LineRenderer line;

	Vector3 animTarget;
	Vector3 center;
	float animSphereSize;
	float showMenuDistance;
	float timer;
	bool showLevels = false;

	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawSphere(center, animSphereSize);
	}

	void Update()
	{
		float targetDistance = Vector3.Distance(transform.position, animTarget);

		if (showLevels)
			transform.localScale = Vector3.one * Mathf.Lerp(0, 0.4f, targetDistance / showMenuDistance);

		if (targetDistance <= 0.1f)
		{
			if (showLevels)
			{
				GameManager.ChangeState(GameState.Level_selection);

				showLevels = false;
				Destroy(transform.gameObject);
			}
			else
				PickNewAnimTarget();
		}

		transform.position = Vector3.MoveTowards(transform.position, animTarget, mainAnimSpeed * Time.deltaTime);

		Trail();
	}

	void Trail()
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

	void PickNewAnimTarget()
	{
		if (showLevels)
		{
			animTarget = center;
			showMenuDistance = Vector3.Distance(center, transform.position);

			transform.LookAt(animTarget, animTarget - transform.position - transform.forward);
		}
		else
		{
			int loopCount = 0;

		pickRandom:
			animTarget = center + Random.onUnitSphere * animSphereSize;
			loopCount++;

			if (loopCount < 3 && Vector3.Distance(animTarget, transform.position) < animSphereSize * minAnimDistance)
				goto pickRandom;

			transform.LookAt(animTarget, animTarget - transform.position - transform.forward);
			SoundsManager.PlaySound("UI", Random.Range(0.8f, 1.2f), 0.2f);
		}
	}

	public void Init(Vector3 center, float sphereSize)
	{
		this.center = center;
		animSphereSize = sphereSize;

		PickNewAnimTarget();
	}

	public void Stop()
	{
		showLevels = true;
		PickNewAnimTarget();
	}
}