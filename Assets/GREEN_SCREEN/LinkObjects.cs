using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Script used for Green Screen scene</summary>
public class LinkObjects : MonoBehaviour
{
	[Header("Settings")]
	public float minSize;
	public float maxSize;
	[Range(0, 1)]
	public float startColorOffset;
	[Range(0, 1)]
	public float endColorOffset;
	[Space]
	public PlayerAnim selectedAnim;
	[Range(0, 1)]
	public float playerAnimPercent;

	[Header("Scene references")]
	public Transform meshRoot;
	public Animator playerAnim;
	public List<SnakePiece> pieces;

	public enum PlayerAnim
	{
		Idle,
		Eat
	}

	void Update()
	{
		Time.timeScale = 0;

		playerAnim.enabled = true;
		playerAnim.Play(selectedAnim.ToString(), 0, playerAnimPercent);

		SetupTrail();
	}

	public int ComparePieces(SnakePiece first, SnakePiece second)
	{
		// 1 => first is closer
		// -1 => second is closer

		if (first == null)
		{
			if (second == null)
				return 0;
			else
				return -1;
		}
		else
		{
			if (second == null)
				return 1;
			else
			{
				float firstDistance = Vector3.Distance(playerAnim.transform.position, first.transform.position);
				float secondDistance = Vector3.Distance(playerAnim.transform.position, second.transform.position);
				return Math.Sign(firstDistance - secondDistance);
			}
		}
	}

	// [ContextMenu("Setup trail")]
	void SetupTrail()
	{
		pieces.Sort(ComparePieces);

		for (int i = 0; i < pieces.Count; i++)
		{
			float percent = ((float)i + 1) / pieces.Count;

			if (i == 0)
				pieces[i].UpdateLine(meshRoot.position, 0, false);
			else
				pieces[i].UpdateLine(pieces[i - 1].backPoint, pieces[i - 1].targetLineWidth, false);

			pieces[i].transform.localScale = Vector3.one * Mathf.Lerp(maxSize, minSize, percent);
			pieces[i].line.enabled = true;

			float colorPercent = Mathf.Lerp(startColorOffset, endColorOffset, percent);
			float h, s, v;

			foreach (MeshRenderer rend in pieces[i].colorAnim.renderers)
			{
				Color.RGBToHSV(pieces[i].colorAnim.renderers[0].material.color, out h, out s, out v);
				Color result = Color.HSVToRGB(colorPercent, s, v);
				result.a = pieces[i].colorAnim.renderers[0].material.color.a;
				rend.material.color = result;

				if (rend.material.HasProperty(AnimateMeshesColor.EMISSION_COLOR_KEY))
				{
					Color.RGBToHSV(rend.material.GetColor(AnimateMeshesColor.EMISSION_COLOR_KEY), out h, out s, out v);
					Color emissionColor = Color.HSVToRGB(colorPercent, s, v);
					emissionColor.a = rend.material.GetColor(AnimateMeshesColor.EMISSION_COLOR_KEY).a;
					rend.material.SetColor(AnimateMeshesColor.EMISSION_COLOR_KEY, emissionColor);
				}
			}

			Color.RGBToHSV(pieces[i].lineAnim.line.colorGradient.colorKeys[0].color, out h, out s, out v);
			Color targetColor = Color.HSVToRGB(Mathf.Lerp(startColorOffset, endColorOffset, percent - (0.5f / pieces.Count)), s, v);

			targetColor.a = 1;
			pieces[i].lineAnim.line.startColor = targetColor;
			pieces[i].lineAnim.line.endColor = targetColor;
		}
	}
}