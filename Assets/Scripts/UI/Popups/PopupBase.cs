using UnityEngine;
using UnityEngine.UI;

/// <summary>Base class for configurable Popups that can be poped at any time</summary>
public class PopupBase : MonoBehaviour
{
	public Animator anim;
	public Button closeButton;

	bool specialAnim;

	virtual protected void Awake()
	{
		if (closeButton != null)
		{
			closeButton.onClick.AddListener(() =>
			{
				anim.Play("Hide", 0);
				SoundsManager.PlaySound("Click");

				if (specialAnim)
					anim.Play("HideSpecial", 1);
			});
		}
	}

	protected void Pop(bool specialAnim)
	{
		this.specialAnim = specialAnim;

		anim.Play("Show", 0);

		if (specialAnim)
			anim.Play("ShowSpecial", 1);
	}
}