using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Popup that displays clickable internet links</summary>
public class PopupCredits : PopupBase
{
	[Header("Settings")]
	public ClickableLink mikeLink;
	public ClickableLink angusLink;

	void Awake()
	{
		mikeLink.Subscribe();
		angusLink.Subscribe();
	}

	public new void Pop() => base.Pop();

	/// <summary>Button and url that it has to open</summary>
	[Serializable]
	public class ClickableLink
	{
		public Button button;
		public string url;

		public void Subscribe() => button.onClick.AddListener(() => Application.OpenURL(url));
	}
}