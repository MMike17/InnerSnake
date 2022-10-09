using UnityEngine;

/// <summary>Elements that the player can pickup</summary>
public class Pickup : MonoBehaviour
{
	[Header("Settings")]
	public float rotationAnimSpeed;
	public float upDownAnimSpeed;
	public float upDownAnimMagnetude;

	[Header("Scene references")]
	public SnakePiece piece;
	public Transform vfx;

	MinMax upDownRange;

	public void Init(Vector3 mapCenter, float mapRadius, float playerHeight)
	{
		transform.position = mapCenter + Vector3.Normalize(transform.position - mapCenter).normalized * mapRadius;

		// offset 0.7f to not touch ground
		float min = playerHeight;
		upDownRange = new MinMax(min, min + upDownAnimMagnetude);
	}

	void Update()
	{
		transform.Rotate(Vector3.up * rotationAnimSpeed * Time.deltaTime, Space.Self);
		piece.transform.position = transform.position + transform.up * upDownRange.GetValue(Mathf.Sin(Time.time * upDownAnimSpeed) / 2 + 0.5f);
		vfx.position = piece.transform.position;
	}

	public void Collect(float previousOffset)
	{
		SoundsManager.PlaySound("Pickup");

		piece.Collect(previousOffset);
		Destroy(gameObject);
	}
}