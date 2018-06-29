using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUFlock : MonoBehaviour {
	private struct Fish {
		public Vector3 pos;
		public float mass;
		public Vector3 vel;
		public float padding;
	}
	public ComputeShader m_FlockShader;
	public Mesh m_FishMesh;
	public Material m_FishMat;
	private Material m_FishMaterialInstance;
	public int m_FishCount = 500;
	public float m_MinMass = 50;
	public float m_MaxMass = 100;
	public float m_MaxSpeed = 5;
	public float m_MaxTurnSpeed = 5;
	public float m_NeighbouringDistance = 1;
	public float m_SeperationDistance = 0.5f;
	public Transform m_Target;
	private Fish[] m_Fish;
	private ComputeBuffer[] m_FishBuffers;
	private ComputeBuffer m_ArgsBuffer;
	private struct IndirectArgs {
		public uint IndexCount;
		public uint InstanceCount;
		public uint IndexOffset;
		public int VertexLocationOffset;
		public uint InstanceOffset;
	}
	private int m_KernelID = -1;
	private int m_CurrentSourceBuffer = 0;
	void Awake() {
		Initialise();
	}

	void OnDestroy()
	{
		Cleanup();
	}

	private void Initialise() {
		Cleanup();
		m_Fish = new Fish[m_FishCount];

		for (int i = 0; i < m_FishCount; i++) {
			m_Fish[i].pos = transform.position;
			m_Fish[i].vel = Vector3.zero;
			m_Fish[i].mass = Random.Range(m_MinMass, m_MaxMass);
		}
		m_FishBuffers = new ComputeBuffer[2];
		m_FishBuffers[0] = new ComputeBuffer(m_FishCount, 32);
		m_FishBuffers[1] = new ComputeBuffer(m_FishCount, 32);
		
		m_FishBuffers[0].SetData(m_Fish);
		m_FishBuffers[1].SetData(m_Fish);
		m_KernelID = m_FlockShader.FindKernel("CSMain");

		m_FishMaterialInstance = Instantiate<Material>(m_FishMat);
		m_FishMaterialInstance.hideFlags = HideFlags.HideAndDontSave;


		IndirectArgs[] args = new IndirectArgs[1];
		args[0].IndexCount = m_FishMesh.GetIndexCount(0);
		args[0].InstanceCount = (uint) m_FishCount;
		args[0].IndexOffset = 0;
		args[0].VertexLocationOffset = 0;
		args[0].InstanceOffset = 0;
		m_ArgsBuffer = new ComputeBuffer(1, 20, ComputeBufferType.IndirectArguments);
		m_ArgsBuffer.SetData(args);
		
	}

	void Update() {
		m_FlockShader.SetFloat("_MaxSpeed", m_MaxSpeed);
		m_FlockShader.SetFloat("_MaxTurnSpeed", m_MaxTurnSpeed);
		m_FlockShader.SetFloat("_NeighbouringDistance", m_NeighbouringDistance);
		m_FlockShader.SetFloat("_SeperationDistance", m_SeperationDistance);
		m_FlockShader.SetVector("_TargetPos", m_Target.position);
		m_FlockShader.SetInt("_FishCount", m_FishCount);
		m_FlockShader.SetFloat("_DeltaTime", Time.deltaTime);

		m_FlockShader.SetBuffer(m_KernelID, "_SourceFish", m_FishBuffers[m_CurrentSourceBuffer]);

		m_FlockShader.SetBuffer(m_KernelID, "_TargetFish", m_FishBuffers[1 - m_CurrentSourceBuffer]);
		

		ComputeThreadGroupHelper helper = new ComputeThreadGroupHelper(
			m_FlockShader, m_KernelID);
		int xGroupCount, yGroupCount, zGroupCount;
		helper.GetGroupCountForElementCount(m_FishCount, 0, 0, 
			out xGroupCount, out yGroupCount, out zGroupCount);

		m_FlockShader.Dispatch(m_KernelID, xGroupCount, yGroupCount, zGroupCount);
		m_FishMaterialInstance.SetBuffer("_SourceFish", m_FishBuffers[m_CurrentSourceBuffer]);

		Graphics.DrawMeshInstancedIndirect(m_FishMesh, 0, m_FishMaterialInstance,
			new Bounds(transform.position, Vector3.one * 500), m_ArgsBuffer);

		m_CurrentSourceBuffer = 1 - m_CurrentSourceBuffer;


	}

	private void Cleanup() {
		if (m_FishBuffers != null) {
			for (int i = 0; i < m_FishBuffers.Length; i++) {
				if (m_FishBuffers[i] != null) {
					m_FishBuffers[i].Release();
				}
			}
		}
		if (m_ArgsBuffer != null) {
			m_ArgsBuffer.Release();
		}
	}
}
