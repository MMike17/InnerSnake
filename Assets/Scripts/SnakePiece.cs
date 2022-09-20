using System.Collections;
using UnityEngine;

/// <summary>Represents a snake piece that the player can recover</summary>
public class SnakePiece : MonoBehaviour
{
	[Header("Settings")]
	public float growDuration;
	public float normalSize;
	public float minSize;

	[Header("Scene references")]
	public Transform mesh;
	public AnimateColor anim;
	public new Collider collider;

	void Awake()
	{
		float initialSize = mesh.localScale.x;
	}

	IEnumerator Grow()
	{
		float timer = 0;

		while (timer < growDuration)
		{
			timer += Time.deltaTime;
			mesh.localScale = Vector3.one * Mathf.Lerp(minSize, normalSize, timer / growDuration);
			yield return null;
		}

		mesh.localScale = Vector3.one * normalSize;
		collider.enabled = true;
	}

	public void Collect(float previousOffset)
	{
		collider.enabled = false;
		transform.SetParent(null);

		StartCoroutine(Grow());
		anim.Reset(0.1f);

		this.DelayAction(() => collider.enabled = true, 0.5f);
	}
}