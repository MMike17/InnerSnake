using UnityEngine;
using UnityEngine.UI;

/// <summary>Popup asking the user if they want to rate the game</summary>
public class PopupRating : PopupBase
{
	[Header("Scene references")]
	public Button rateButton;

	void Awake()
	{
		rateButton.onClick.AddListener(() =>
		{
			Application.OpenURL("https://mikematthews.itch.io/inner-snake");
			closeButton.onClick.Invoke();
		});
	}

	public void Pop() => base.Pop(false);
}