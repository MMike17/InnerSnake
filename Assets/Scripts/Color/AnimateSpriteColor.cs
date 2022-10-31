using UnityEngine;

/// <summary>Animates a sprite's color depending on time</summary>
public class AnimateSpriteColor : AnimateColor
{
	[Header("Scene references")]
	public SpriteRenderer sprite;

	protected override void ApplyColor()
	{
		Color.RGBToHSV(sprite.color, out float h, out float s, out float v);
		Color result = Color.HSVToRGB(percent, s, v);
		result.a = sprite.color.a;
		sprite.color = result;
	}
}