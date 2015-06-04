using UnityEngine;
using System.Collections;

public class InputSerializer : MonoBehaviour {

	int m_RacerNumber;

	public Vector3 forwardForce = Vector3.zero;
	public Vector3 turningForce = Vector3.zero;

	public int RacerNumber {
		get {
			return m_RacerNumber;
		}
	}

	public Vector3 ForwardForce {
		get {
			return forwardForce;
		}
	}

	public Vector3 TurningForce {
		get {
			return turningForce;
		}
	}

	public void Initialize(int RacerNumber)
	{
		m_RacerNumber = RacerNumber;
	}
	public void OnNetworkInstantiate (NetworkMessageInfo info)
	{
		if (Network.isClient)
			return;

		CGameManager gamemanager = GameObject.Find("GameManager").GetComponent<CGameManager>();
		if(gamemanager != null)
		{
			for(int i = 0; i < gamemanager.m_SerializedInputs.Length; ++i)
			{
				if(gamemanager.m_SerializedInputs[i] == null)
				{
					gamemanager.m_SerializedInputs[i] = this;
					break;
				}
			}
		}
	}
	public void BufferForwardForce(Vector3 force)
	{
		forwardForce = force;
	}

	public void BufferTurningForce(Vector3 force)
	{
		turningForce = force;
	}

	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		stream.Serialize(ref m_RacerNumber);
		stream.Serialize(ref forwardForce);
		stream.Serialize(ref turningForce);

		if (stream.isWriting)
			Debug.Log("Sending player #" + m_RacerNumber);
		else if(stream.isReading)
			Debug.Log("Reading player #" + m_RacerNumber);
	}
}
