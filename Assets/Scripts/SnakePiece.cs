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
	public AnimateColor colorAnim;
	public new Collider collider;
	public Animator anim;
	public Collider lineCollider;
	public LineRenderer line;
	public AnimateLineColor lineAnim;

	public Vector3 backPoint => transform.position - transform.forward * mesh.lossyScale.z;
	public float targetLineWidth => Mathf.Lerp(mesh.lossyScale.x, mesh.lossyScale.y, 0.5f);

	Vector3 forwardPoint => transform.position + transform.forward * mesh.lossyScale.z;

	void Awake()
	{
		float initialSize = mesh.localScale.x;
	}

	IEnumerator Grow()
	{
		anim.enabled = false;
		float timer = 0;

		while (timer < growDuration)
		{
			timer += Time.deltaTime;
			mesh.localScale = Vector3.one * Mathf.Lerp(minSize, normalSize, timer / growDuration);
			yield return null;
		}

		mesh.localScale = Vector3.one * normalSize;
		collider.enabled = true;

		line.enabled = true;
		lineCollider.enabled = true;
		anim.enabled = true;
		anim.Play("Active");
	}

	public void Collect(float previousOffset)
	{
		collider.enabled = false;
		transform.SetParent(null);

		StartCoroutine(Grow());
		colorAnim.Reset(previousOffset + 0.1f);
		lineAnim.Reset(previousOffset + 0.05f);

		this.DelayAction(() => collider.enabled = true, 0.5f);
	}

	public void UpdateLine(Vector3 targetPos, float endSize)
	{
		Vector3[] positions = new Vector3[line.positionCount];
		line.GetPositions(positions);

		positions[0] = forwardPoint;
		positions[1] = targetPos;
		line.SetPositions(positions);

		line.startWidth = targetLineWidth;
		line.endWidth = endSize;

		float colliderSize = Vector3.Distance(forwardPoint, targetPos) - 1;
		lineCollider.transform.position = Vector3.Lerp(forwardPoint, targetPos, 0.5f);
		lineCollider.transform.forward = lineCollider.transform.position - transform.position;
	}
}