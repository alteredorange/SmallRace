using UnityEngine;
using System.Collections;

public class AIFloat : MonoBehaviour {
	
	
	// Boost Power up Variables
	private float maxExtraBoost = 1.0f;
	private float extraBoost = 0f;
	private bool isBoosting = false;
	private float boostTime = 1.0f;
	

	
	public float m_hoverForce = 9.0f;
	public float m_hoverHeight = 2.0f;
	public GameObject[] m_hoverPoints;
//	public CPlayer m_Player = null;
	
	public float m_forwardAcl = 100.0f;
	public float m_backwardAcl = 25.0f;
	float m_currThrust = 0.0f;

	public float m_turnStrength = 10f;
	float m_currTurn = 0.0f;
	
	public GameObject m_leftAirBrake;
	public GameObject m_rightAirBrake;


	public int m_CheckPoints = 0;
	public int m_Laps = 0;

	Rigidbody rb;
	 NetworkView NetView;
	//  Transform m_Camera;
	Transform m_Mesh;
	float meshStartY;
	//  ParticleSystem[] m_Particles;
	
	//  bool firstData = true;
	float m_Speed;
	float m_deadZone = 0.1f;
	int m_layerMask;
	float InputX = 0;
	float InputY = 0;

	
	// Smooth
	Vector3 posGot;
	Vector3 posGotVel;
	Quaternion rotGot;
	Quaternion rotGotVel;
	



	void Awake()
	{
		rb = GetComponent<Rigidbody> ();
		m_Mesh = transform.FindChild("Mesh");

	}


	void Update()
		    {
		     
		            // Main Thrust
		              m_currThrust = 0.0f;
		            
					float aclAxis = Input.GetAxis("Vertical");
					aclAxis += (aclAxis >= 0f) ? extraBoost : -extraBoost;
					
		            if (aclAxis > m_deadZone)
		                m_currThrust = aclAxis * m_forwardAcl;
		            else if (aclAxis < -m_deadZone)
		                m_currThrust = aclAxis * m_backwardAcl;
		
		            // Turning
		            m_currTurn = 0.0f;
		            //float turnAxis = Input.GetAxis("Horizontal");
		            float turnAxis = Input.GetAxis("Horizontal");
		            if (Mathf.Abs(turnAxis) > m_deadZone)
		                m_currTurn = turnAxis;
		
		            if (m_currThrust < 0) // TODO
		                m_currTurn *= -1;

		    }
		    void FixedUpdate()
		    {
		       
		       
		            Movement();
		       //     CameraFollow();
		        }
		       
		    
	void Movement()
	{
	
		RaycastHit hit;
		for (int i = 0; i < m_hoverPoints.Length; i++) {
			var hoverPoint = m_hoverPoints [i];
			if (Physics.Raycast (hoverPoint.transform.position,
			                    -Vector3.up, out hit,
			                    m_hoverHeight))
						rb.AddForceAtPosition (Vector3.up
					* m_hoverForce
					* (1.0f - (hit.distance / m_hoverHeight)),
				                      hoverPoint.transform.position);
			else {
				if (transform.position.y > hoverPoint.transform.position.y)
					rb.AddForceAtPosition (
						hoverPoint.transform.up * m_hoverForce,
						hoverPoint.transform.position);
				else
					rb.AddForceAtPosition (
						hoverPoint.transform.up * -m_hoverForce,
						hoverPoint.transform.position);
			}


			// Forward
			        if (Mathf.Abs(m_currThrust) > 0)
			            rb.AddForce(transform.forward * m_currThrust);

			
			        // Turn
			        if (m_currTurn > 0)
			            rb.AddRelativeTorque(Vector3.up * m_currTurn * m_turnStrength);
			        else if (m_currTurn < 0)
			            rb.AddRelativeTorque(Vector3.up * m_currTurn * m_turnStrength);


		}
	}


	void OnTriggerEnter(Collider Col)
	{
			if (Col.tag == "CheckPoint" /* lazy way */)
		{
			int id = int.Parse(Col.name.Substring(Col.name.Length - 1));
			if (m_CheckPoints < id)
				m_CheckPoints++;
		}
		else if (Col.tag == "EndLap" && m_CheckPoints >= CGameManager.ins.m_CheckpointsNeeded)
		{
			m_Laps++;
			CGameManager.ins.OnLap(m_Laps);
			m_CheckPoints = 0;
			if (m_Laps >= CGameManager.ins.m_LapsNeeded)
			{
				NetworkMessageInfo a = new NetworkMessageInfo();
				if (Network.isServer)
					CGameManager.ins.Client_Finish(CGameManager.ins.m_TimePassed, 0, a);
				else
					CGameManager.ins.NetView.RPC("Client_Finish", RPCMode.Server, CGameManager.ins.m_TimePassed, -1);
			
			}
		}
		// Entered Boost Trigger?
		else if (Col.tag == "Boost" && !isBoosting)
		{
			StartCoroutine(BoostCoroutine());
		}
	}


	IEnumerator BoostCoroutine()
	{
		//print("Boost Start");
		isBoosting = true;
		extraBoost = maxExtraBoost;
		yield return new WaitForSeconds(boostTime);
		extraBoost = 0f;
		isBoosting = false;
		//print("Boost Stop");
	}



	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting) {
			Vector3 vec = transform.position;
			Quaternion qua = transform.rotation;
			stream.Serialize(ref vec);
			stream.Serialize(ref qua);
			Debug.Log ("Sending player #" + NetView.viewID);
		} else if (stream.isReading) {
			Vector3 vec = Vector3.zero;
			Quaternion qua = Quaternion.identity;
			stream.Serialize(ref vec);
			stream.Serialize(ref qua);
			rb.position = Vector3.SmoothDamp(rb.position, vec, ref posGotVel, 1 / 30);
			rb.rotation = Quaternion.Lerp(rb.rotation, qua, 0.5f);
			Debug.Log ("Reading player #" + NetView.viewID);
		}
	}
	

	
}
