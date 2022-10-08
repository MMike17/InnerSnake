using UnityEngine;

/// <summary>Animates a particle's system color depending on time</summary>
public class AnimateParticlesColor : AnimateColor
{
	[Header("Scene references")]
	public ParticleSystem particles;

	protected override void ApplyColor()
	{
		ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();

		Color.RGBToHSV(renderer.material.color, out float h, out float s, out float v);
		Color result = Color.HSVToRGB(percent, s, v);

		renderer.material.color = result;
	}
}