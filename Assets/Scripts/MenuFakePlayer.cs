using UnityEngine;

using static GameManager;
using Random = UnityEngine.Random;

/// <summary>Fake player used in the main menu</summary>
public class MenuFakePlayer : MonoBehaviour
{
	// TODO : fix this shit

	[Range(0.1f, 0.5f)]
	public float minAnimDistance;
	public float mainAnimSpeed;

	Vector3 animTarget;
	Vector3 center;
	float animSphereSize;
	float showMenuDistance;
	bool showLevels;

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

		Instantiate(this, center, Quaternion.identity);
		PickNewAnimTarget();
	}

	public void Stop()
	{
		showLevels = true;
		PickNewAnimTarget();
	}
}