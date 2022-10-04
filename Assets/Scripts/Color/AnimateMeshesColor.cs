using UnityEngine;

/// <summary>Animates an mesh's color depending on time</summary>
public class AnimateMeshesColor : AnimateColor
{
	public const string EMISSION_COLOR_KEY = "_EmissionColor";

	[Header("Scene references")]
	public Renderer[] renderers;

	protected override void ApplyColor()
	{
		Color.RGBToHSV(renderers[0].material.color, out float h, out float s, out float v);
		Color result = Color.HSVToRGB(percent, s, v);
		result.a = renderers[0].material.color.a;

		foreach (Renderer rend in renderers)
		{
			rend.material.color = result;

			if (rend.material.HasProperty(EMISSION_COLOR_KEY))
			{
				Color.RGBToHSV(rend.material.GetColor(EMISSION_COLOR_KEY), out h, out s, out v);
				Color emissionColor = Color.HSVToRGB(percent, s, v);
				emissionColor.a = rend.material.GetColor(EMISSION_COLOR_KEY).a;
				rend.material.SetColor(EMISSION_COLOR_KEY, emissionColor);
			}
		}
	}
}