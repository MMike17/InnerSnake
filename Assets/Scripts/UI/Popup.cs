using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Configurable Popup that can be poped at any time</summary>
public class Popup : MonoBehaviour
{
	public Animator anim;
	public TMP_Text message;
	public Button closeButton;

	void Awake()
	{
		closeButton.onClick.AddListener(() => anim.Play("Hide"));
	}

	public void Pop(string message)
	{
		anim.Play("Show");
		this.message.text = message;
	}
}