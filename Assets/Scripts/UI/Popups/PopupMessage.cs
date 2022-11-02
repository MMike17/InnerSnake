using TMPro;
using UnityEngine;

/// <summary>Popup that displays a message</summary>
public class PopupMessage : PopupBase
{
	[Header("Settings")]
	public float rotationSpeed;

	[Header("Scene references")]
	public TMP_Text message;
	public RectTransform specialBackground;

	void Update() => specialBackground.Rotate(0, 0, -rotationSpeed * Time.deltaTime);

	new void Awake() => base.Awake();

	public void Pop(string message, bool specialAnim)
	{
		base.Pop(specialAnim);
		this.message.text = message;
	}
}