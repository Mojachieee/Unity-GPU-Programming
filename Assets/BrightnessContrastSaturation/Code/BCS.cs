using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class BCS : MonoBehaviour {


	public Shader m_EffectsShader;
	private Material m_EffectMat;

	[Range(-1, 1)]
	public float m_Brightness = 0;

	[Range(0, 2)]
	public float m_Contrast = 1;

	[Range(0, 2)]
	public float m_Saturation = 1;

	/// <summary>
	/// OnRenderImage is called after all rendering is complete to render image.
	/// </summary>
	/// <param name="src">The source RenderTexture.</param>
	/// <param name="dest">The destination RenderTexture.</param>
	void OnRenderImage(RenderTexture src, RenderTexture dest) {
		if (m_EffectMat == null) {
			InitialiseMaterial();
		}
		if (m_EffectMat == null) {
			Graphics.Blit(src, dest);
		} else {

			m_EffectMat.SetFloat("_Brightness", m_Brightness);
			m_EffectMat.SetFloat("_Contrast", m_Contrast);
			m_EffectMat.SetFloat("_Saturation", m_Saturation);
			
			Graphics.Blit(src, dest, m_EffectMat);
		}
	}

	private void InitialiseMaterial() {
		if (m_EffectsShader != null) {
			m_EffectMat = new Material(m_EffectsShader);
			m_EffectMat.hideFlags = HideFlags.HideAndDontSave;
		}
	}
}
