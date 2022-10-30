using UnityEngine;
using UnityEngine.UI;

/// <summary>Base class for configurable Popups that can be poped at any time</summary>
public class PopupBase : MonoBehaviour
{
	public Animator anim;
	public Button closeButton;

	void Awake()
	{
		if (closeButton != null)
		{
			closeButton.onClick.AddListener(() =>
			{
				anim.Play("Hide");
				SoundsManager.PlaySound("Click");
			});
		}
	}

	protected void Pop()
	{
		anim.Play("Show");
	}
}