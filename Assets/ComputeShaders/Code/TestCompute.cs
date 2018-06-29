using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCompute : MonoBehaviour {

	public ComputeShader m_Shader;
	public Material m_DebugMat;

	private int m_KernelID = -1;
	private RenderTexture m_Texture;

	private void Awake() {
		InitTexture();
		if (m_Shader != null) {
			m_KernelID = m_Shader.FindKernel("CSMain");
			m_Shader.SetTexture(m_KernelID, "Result", m_Texture);

			int xGroupCount, yGroupCount, zGroupCount;

			ComputeThreadGroupHelper groupHelper = new ComputeThreadGroupHelper(
				m_Shader, m_KernelID);

			groupHelper.GetGroupCountForElementCount(m_Texture.width, m_Texture.height, 0,
				out xGroupCount, out yGroupCount, out zGroupCount);
			m_Shader.Dispatch(m_KernelID, xGroupCount, yGroupCount, zGroupCount);
		}
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{
		m_Shader.SetFloat("_Time", Time.time);

		int xGroupCount, yGroupCount, zGroupCount;
		ComputeThreadGroupHelper groupHelper = new ComputeThreadGroupHelper(
				m_Shader, m_KernelID);

			groupHelper.GetGroupCountForElementCount(m_Texture.width, m_Texture.height, 0,
				out xGroupCount, out yGroupCount, out zGroupCount);
		m_Shader.Dispatch(m_KernelID, xGroupCount, yGroupCount, zGroupCount);
	}

	private void InitTexture() {
		CleanupTexture();
		m_Texture = new RenderTexture(64, 64, 0);
		m_Texture.enableRandomWrite = true;
		m_Texture.filterMode = FilterMode.Point;
		m_Texture.Create();

		if (m_DebugMat != null) {
			m_DebugMat.SetTexture("_MainTex", m_Texture);
		}
	}

	private void CleanupTexture() {
		if (m_Texture != null) {
			m_Texture.Release();
			m_Texture = null;
		}
	}

	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed.
	/// </summary>
	void OnDestroy()
	{
		CleanupTexture();
		m_DebugMat.SetTexture("_MainTex", null);

	}
}
