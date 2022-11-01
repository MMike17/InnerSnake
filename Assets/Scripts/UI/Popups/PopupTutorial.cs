using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Popup that displays tutorials</summary>
public class PopupTutorial : PopupBase
{
	[Header("Settings")]
	public List<Panel> tutorialContent;

	[Header("Scene references")]
	public GameObject singlePanel;
	public Image singleDisplay;
	[Space]
	public GameObject doublePanel;
	public Image doubleDisplayLeft;
	public Image doubleDisplayRight;
	[Space]
	public TMP_Text message;
	public Button previous;
	public Button next;

	Action OnDone;
	int tutorialIndex;

	void Awake()
	{
		tutorialIndex = 0;

		previous.onClick.AddListener(() =>
		{
			SoundsManager.PlaySound("Click");

			if (tutorialIndex > 0)
			{
				tutorialIndex--;
				SetData();
			}
		});
		next.onClick.AddListener(() =>
		{
			SoundsManager.PlaySound("Click");

			if (tutorialIndex < tutorialContent.Count - 1)
			{
				tutorialIndex++;
				SetData();
			}
			else
			{
				anim.Play("Hide");
				this.DelayAction(OnDone, 1);
			}
		});
	}

	void SetData()
	{
		Panel selected = tutorialContent[tutorialIndex];

		singlePanel.SetActive(selected.rightImage == null);
		doublePanel.SetActive(selected.rightImage != null);

		if (selected.rightImage == null)
			singleDisplay.sprite = selected.leftImage;
		else
		{
			doubleDisplayLeft.sprite = selected.leftImage;
			doubleDisplayRight.sprite = selected.rightImage;
		}

		message.text = selected.message;
	}

	public void Pop(Action onDone)
	{
		base.Pop(false);
		OnDone = onDone;
	}

	/// <summary>Represents data for a tutorial panel</summary>
	[Serializable]
	public class Panel
	{
		public Sprite leftImage;
		public Sprite rightImage;
		[TextArea]
		public string message;
	}
}