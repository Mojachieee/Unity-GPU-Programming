using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowCPUFlockBehaviour : MonoBehaviour
{
	private struct Fish
	{
		public Vector3 Position;
		public Vector3 Velocity;
		public float Mass;

		public GameObject FishInstance;
	}

	public GameObject m_FishPrefab = null;
	[Range(1, 2500)]
	public int m_FishCount = 100;

	public float m_MinMass = 50.0f;
	public float m_MaxMass = 100.0f;

	public float m_MaxSpeed = 5.0f;
	public float m_MaxTurnSpeed = 5.0f;


	public float m_FishSize = 0.5f;
	public Transform m_Target = null;
	private Fish[] m_Fish;

	private void Awake()
	{
		Initialise();
	}

	[ContextMenu("Initialise")]
	private void Initialise()
	{
		m_Fish = new Fish[m_FishCount];

		for (int i = 0; i < m_FishCount; i++)
		{
			m_Fish[i].Position = transform.position;
			m_Fish[i].Velocity = Vector3.zero;
			m_Fish[i].Mass = Random.Range(m_MinMass, m_MaxMass);
			m_Fish[i].FishInstance = Instantiate<GameObject>(m_FishPrefab, transform.position, Quaternion.identity, transform);
		}
	}

	// - Returns clamped velocity toward the target
	private Vector3 SeekTarget(Vector3 elementPosition, Vector3 elementVelocity, Vector3 targetPosition)
	{
		Vector3 desiredVelocity = targetPosition - elementPosition;
		if (desiredVelocity.sqrMagnitude > 0)
		{
			desiredVelocity = desiredVelocity.normalized * m_MaxSpeed;
		}

		Vector3 turningForce = desiredVelocity - elementVelocity;

		if (turningForce.sqrMagnitude > m_MaxTurnSpeed * m_MaxTurnSpeed)
		{
			turningForce = turningForce.normalized * m_MaxTurnSpeed;
		}

		return turningForce;
	}

	// - Flock algorithm
	private Vector3 Flock(int id, Vector3 elementPosition, Vector3 elementVelocity)
	{
		Vector3 flockForce = Vector3.zero;
		Vector3 separateForce = Vector3.zero;
		Vector3 alignForce = Vector3.zero;
		Vector3 cohereForce = Vector3.zero;
		int neighbourCounter = 0;

		// - For each other fish, calculate separate, align and cohere forces and the number of neighbours
		for(int j = 0; j < m_Fish.Length; j++)
		{
			if(id == j) continue;

            Vector3 distLine = elementPosition - m_Fish[j].Position;
            float distLineLength = distLine.sqrMagnitude;

            if(distLineLength < m_FishSize * m_FishSize)
            {
                if(distLineLength > 0)
                {
					separateForce += distLine.normalized / distLine.magnitude;
                }

                alignForce += m_Fish[j].Velocity;
                cohereForce += m_Fish[j].Position;
                neighbourCounter++;
            }
		}

		// - If there was any neighbour close, average and clamp forces
		if (neighbourCounter > 0)
		{
			float forceLength = separateForce.sqrMagnitude;
			if (forceLength > 0)
			{
				separateForce /= neighbourCounter;
				if (forceLength > m_MaxTurnSpeed * m_MaxTurnSpeed)
				{
					separateForce = separateForce.normalized * m_MaxTurnSpeed;
				}
				else
				{
					separateForce = separateForce.normalized * m_MaxSpeed;
				}
			}

			alignForce = alignForce / neighbourCounter;
			float combinedVelocitiesLength = alignForce.sqrMagnitude;
			if (combinedVelocitiesLength > m_MaxSpeed * m_MaxSpeed || combinedVelocitiesLength > m_MaxTurnSpeed * m_MaxTurnSpeed)
			{
				alignForce = alignForce.normalized * Mathf.Min(m_MaxSpeed, m_MaxTurnSpeed);
			}

			cohereForce = Approach(elementPosition, cohereForce / neighbourCounter);
		}


		// - Returns total flock velocity
		flockForce += separateForce;
		flockForce += alignForce;
		flockForce += cohereForce;

		return flockForce;
	}

	private Vector3 Approach(Vector3 elementPosition, Vector3 targetPosition)
	{
		Vector3 desiredVelocity = targetPosition - elementPosition;
		float dist = 1;

		float velocityLength = desiredVelocity.sqrMagnitude;
		if (velocityLength < dist * dist)
		{
			if (velocityLength > 0)
			{
				// - Arbitrary damping for when the fish is close enough
				desiredVelocity = desiredVelocity.normalized * Mathf.Lerp(0, m_MaxSpeed, velocityLength / 3);
			}
		}
		else
		{
			desiredVelocity = desiredVelocity.normalized * m_MaxSpeed;
		}

		if (velocityLength > m_MaxTurnSpeed * m_MaxTurnSpeed)
		{
			desiredVelocity = desiredVelocity.normalized * m_MaxTurnSpeed;
		}

		return desiredVelocity;
	}

	private void Update()
	{
		// - For Each Fish
		for (int i = 0; i < m_Fish.Length; i++)
		{
			Vector3 acceleration = Vector3.zero;

			acceleration += SeekTarget(m_Fish[i].Position, m_Fish[i].Velocity, m_Target.position);
			acceleration += Flock(i, m_Fish[i].Position, m_Fish[i].Velocity);

			// - Updating and clamping fish velocity and apply the mass modifier
			acceleration /= m_Fish[i].Mass;
			m_Fish[i].Velocity += acceleration;
			if (m_Fish[i].Velocity.sqrMagnitude > m_MaxSpeed * m_MaxSpeed)
			{
				m_Fish[i].Velocity = m_Fish[i].Velocity.normalized * m_MaxSpeed;
			}

			m_Fish[i].Position += m_Fish[i].Velocity * Time.deltaTime;

			// - Update the game objects location and orientation
			m_Fish[i].FishInstance.transform.position = m_Fish[i].Position;
			if(m_Fish[i].Velocity.sqrMagnitude > 0.0001)
			{
				m_Fish[i].FishInstance.transform.rotation = Quaternion.LookRotation(m_Fish[i].Velocity);
			}
		}
	}
}
