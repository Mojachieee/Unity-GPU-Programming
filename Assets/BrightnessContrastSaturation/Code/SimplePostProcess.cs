using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class SimplePostProcess : MonoBehaviour {


	public  Material m_EffectMat;

	/// <summary>
	/// OnRenderImage is called after all rendering is complete to render image.
	/// </summary>
	/// <param name="src">The source RenderTexture.</param>
	/// <param name="dest">The destination RenderTexture.</param>
	void OnRenderImage(RenderTexture src, RenderTexture dest) {
		if (m_EffectMat == null) {
			Graphics.Blit(src, dest);
		} else {
			Graphics.Blit(src, dest, m_EffectMat);
		}
	}



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
