using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ComputeThreadGroupHelper {
	public uint m_XThreadsPerGroup;
	public uint m_YThreadsPerGroup;
	public uint m_ZThreadsPerGroup;
	
	public ComputeThreadGroupHelper(ComputeShader shader, int kernelID) {
		shader.GetKernelThreadGroupSizes(kernelID, out m_XThreadsPerGroup,
			out m_YThreadsPerGroup, out m_ZThreadsPerGroup);
	}

	public void GetGroupCountForElementCount(int xElements, int yElements, int zElements,
		out int xGroupCount, out int yGroupCount, out int zGroupCount) {
			xGroupCount = Mathf.Max(1, (xElements + ((int) m_XThreadsPerGroup - 1)) / (int) m_XThreadsPerGroup);
			yGroupCount = Mathf.Max(1, (yElements + ((int) m_YThreadsPerGroup - 1)) / (int) m_YThreadsPerGroup);
			zGroupCount = Mathf.Max(1, (zElements + ((int) m_ZThreadsPerGroup - 1)) / (int) m_ZThreadsPerGroup);
			
	}
}
