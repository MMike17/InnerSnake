using TMPro;
using UnityEngine;

/// <summary>Popup that displays a message</summary>
public class PopupMessage : PopupBase
{
	[Header("Scene references")]
	public TMP_Text message;

	public void Pop(string message)
	{
		base.Pop();
		this.message.text = message;
	}
}