using UnityEngine;

/// <summary>Animates an line's color depending on time</summary>
public class AnimateLineColor : AnimateColor
{
	[Header("Scene references")]
	public LineRenderer line;

	protected override void ApplyColor()
	{
		Color.RGBToHSV(line.colorGradient.colorKeys[0].color, out float h, out float s, out float v);
		Color targetColor = Color.HSVToRGB(percent, s, v);

		line.startColor = targetColor;
		targetColor.a = 0;
		line.endColor = targetColor;
	}
}