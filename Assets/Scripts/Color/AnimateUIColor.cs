using UnityEngine;
using UnityEngine.UI;

/// <summary>Animates a UI image depending on time</summary>
public class AnimateUIColor : AnimateColor
{
	[Header("Scene references")]
	public Image image;

	protected override void ApplyColor()
	{
		Color.RGBToHSV(image.color, out float h, out float s, out float v);
		image.color = Color.HSVToRGB(percent, s, v);
	}
}