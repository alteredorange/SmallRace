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
	
	public float m_forwardAcl = 100.0f;
	public float m_backwardAcl = 25.0f;
	float m_currThrust = 0.0f;
	
	public float m_turnStrength = 10f;
	float m_currTurn = 0.0f;
	
	public GameObject m_leftAirBrake;
	public GameObject m_rightAirBrake;
	
	Rigidbody rb;
	//  NetworkView NetView;
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
	

	void Movement()
	{
	
		RaycastHit hit;
		for (int i = 0; i < m_hoverPoints.Length; i++) {
			var hoverPoint = m_hoverPoints [i];
			if (Physics.Raycast (hoverPoint.transform.position,
			                    -Vector3.up, out hit,
			                    m_hoverHeight,
			                    m_layerMask))
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
	

	
}
