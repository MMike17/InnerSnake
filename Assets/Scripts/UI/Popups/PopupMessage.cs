using TMPro;
using UnityEngine;

/// <summary>Popup that displays a message</summary>
public class PopupMessage : PopupBase
{
	[Header("Scene references")]
	public TMP_Text message;

	new void Awake() => base.Awake();

	public void Pop(string message, bool specialAnim)
	{
		base.Pop(specialAnim);
		this.message.text = message;
	}
}