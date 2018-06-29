using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class Bloom : MonoBehaviour {

	private enum Pass {
		Prefilter,
		Downsample,
		Upsample,
		ApplyBloom
	}

	public Shader m_BloomShader = null;
	private Material m_BloomMat = null;

	void InitiliaseMaterial() { 
		if (m_BloomShader != null) {
			m_BloomMat = new Material(m_BloomShader);
			m_BloomMat.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	[Range(1, 10)]
	public int m_BlurIterations = 4;

	[Range(0, 5)]
	public float m_Intensity = 1;
	[Range(0, 5)]
	public float m_Threshold = 1;

	/// <summary>
	/// OnRenderImage is called after all rendering is complete to render image.
	/// </summary>
	/// <param name="src">The source RenderTexture.</param>
	/// <param name="dest">The destination RenderTexture.</param>
	void OnRenderImage(RenderTexture src, RenderTexture dest) {
		if (m_BloomMat == null) {
			InitiliaseMaterial();
		}
		if (m_BloomMat == null) {
			Graphics.Blit(src, dest);
			return;
		}

		m_BloomMat.SetFloat("_Intensity", m_Intensity);
		m_BloomMat.SetFloat("_Threshold", m_Threshold);
		m_BloomMat.SetTexture("_SourceTex", src);
		RenderTexture currentSrc = src;
		List<RenderTexture> downsamples = new List<RenderTexture>(m_BlurIterations);
		downsamples.Add(RenderTexture.GetTemporary(currentSrc.width / 2, currentSrc.height / 2, 0, currentSrc.format));

		Graphics.Blit(currentSrc, downsamples[0], m_BloomMat, (int) Pass.Prefilter);
		currentSrc = downsamples[0];

		for (int i = 1; i < m_BlurIterations; i++) {
			if (currentSrc.width < 2 || currentSrc.height < 2) {
				break;
			}
			downsamples.Add(RenderTexture.GetTemporary(currentSrc.width / 2, currentSrc.height / 2, 0, currentSrc.format));
			Graphics.Blit(currentSrc, downsamples[i], m_BloomMat, (int) Pass.Downsample);
			currentSrc = downsamples[i];
		}

		for (int i = downsamples.Count - 1; i > 0; i--) {
			Graphics.Blit(downsamples[i], downsamples[i - 1], m_BloomMat, (int) Pass.Upsample);
		}
		Graphics.Blit(downsamples[0], dest, m_BloomMat, (int) Pass.ApplyBloom);

		for (int i = 0; i < downsamples.Count; i++) {
			RenderTexture.ReleaseTemporary(downsamples[i]);
		}
	}

}
