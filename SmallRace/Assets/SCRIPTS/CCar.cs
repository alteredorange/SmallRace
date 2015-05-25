using UnityEngine;
using System.Collections;

public class CCar : MonoBehaviour {

    public CPlayer m_Player = null;
    public Vector3 CameraOffset;
    //public float maxSpeed = 10f;
    //public float Accel = 0.5f;

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
    NetworkView NetView;
    Transform m_Camera;
    Transform m_Mesh;
    float meshStartY;
    ParticleSystem[] m_Particles;

    bool firstData = true;
    float m_Speed;
    float m_deadZone = 0.1f;
    int m_layerMask;
    float InputX = 0;
    float InputY = 0;
    int m_CheckPoints = 0;
    public int m_Laps = 0;
    public bool m_CanMove = false;

    // Smooth
    Vector3 posGot;
    Vector3 posGotVel;
    Quaternion rotGot;
    Quaternion rotGotVel;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        NetView = GetComponent<NetworkView>();
        m_Camera = Camera.main.transform;
        m_Mesh = transform.FindChild("Mesh");
        meshStartY = m_Mesh.localPosition.y;
        m_Particles = GetComponentsInChildren<ParticleSystem>();

        m_layerMask = 1 << LayerMask.NameToLayer("Characters");
        m_layerMask = ~m_layerMask;
    }
    void OnDrawGizmos()
    {

        //  Hover Force
        RaycastHit hit;
        for (int i = 0; i < m_hoverPoints.Length; i++)
        {
            var hoverPoint = m_hoverPoints[i];
            if (Physics.Raycast(hoverPoint.transform.position,
                                -Vector3.up, out hit,
                                m_hoverHeight,
                                m_layerMask))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(hoverPoint.transform.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(hoverPoint.transform.position,
                               hoverPoint.transform.position - Vector3.up * m_hoverHeight);
            }
        }
    }

    void CameraFollow()
    {
        Vector3 temp = Vector3.zero;
        /*m_Camera.position = Vector3.SmoothDamp(m_Camera.position, rb.position - transform.TransformDirection(CameraOffset), ref temp, 0.05f);
        m_Camera.LookAt(transform);*/
        Vector3 vector = transform.TransformDirection(CameraOffset);
        m_Camera.position = Vector3.SmoothDamp(m_Camera.position, rb.position + vector, ref temp, 0.07f);
        m_Camera.LookAt(transform.position);
    }
    void Movement()
    {
        /*rb.velocity = transform.forward * m_Speed;
        m_Speed = Mathf.Clamp(m_Speed + InputY * Time.deltaTime * Accel, -maxSpeed/2f, maxSpeed);
        if (InputY == 0)
            m_Speed *= 0.99f;
        transform.Rotate(0, InputX * 65 * Time.deltaTime, 0);*/
        RaycastHit hit;
        for (int i = 0; i < m_hoverPoints.Length; i++)
        {
            var hoverPoint = m_hoverPoints[i];
            if (Physics.Raycast(hoverPoint.transform.position,
                                -Vector3.up, out hit,
                                m_hoverHeight,
                                m_layerMask))
                rb.AddForceAtPosition(Vector3.up
                  * m_hoverForce
                  * (1.0f - (hit.distance / m_hoverHeight)),
                                          hoverPoint.transform.position);
            else
            {
                if (transform.position.y > hoverPoint.transform.position.y)
                    rb.AddForceAtPosition(
                      hoverPoint.transform.up * m_hoverForce,
                      hoverPoint.transform.position);
                else
                    rb.AddForceAtPosition(
                      hoverPoint.transform.up * -m_hoverForce,
                      hoverPoint.transform.position);
            }
        }
        if (!m_CanMove)
            return;
        // Forward
        if (Mathf.Abs(m_currThrust) > 0)
            rb.AddForce(transform.forward * m_currThrust);

        // Turn
        if (m_currTurn > 0)
            rb.AddRelativeTorque(Vector3.up * m_currTurn * m_turnStrength);
        else if (m_currTurn < 0)
            rb.AddRelativeTorque(Vector3.up * m_currTurn * m_turnStrength);
    }

    public void OnRestart()
    {
        transform.position = CGameManager.ins.m_SpawnPoints[m_Player.m_ID].position;
        transform.rotation = Quaternion.identity;
        m_Laps = 0;
        m_CheckPoints = 0;
        m_CanMove = false;
    }

    void OnTriggerEnter(Collider Col)
    {
        if (!m_CanMove || !NetView.isMine)
            return;
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
                    CGameManager.ins.Client_Finish(CGameManager.ins.m_TimePassed, a, 0);
                else
                    CGameManager.ins.NetView.RPC("Client_Finish", RPCMode.Server, CGameManager.ins.m_TimePassed);
                m_CanMove = false;
            }
        }
    }

    void Update()
    {
        if (m_Player == null)
            return;

        //m_Mesh.localPosition = Vector3.up * (Mathf.Sin(Time.timeSinceLevelLoad * 2) * 0.2f + meshStartY);

        /*if (InputY > 0)
        {
            foreach (ParticleSystem part in m_Particles)
                part.Emit((int)(200 * Time.deltaTime));
        }*/

        if (NetView.isMine)
        {
            /*InputX = Input.GetAxis("Horizontal");
            InputY = Input.GetAxisRaw("Vertical");
            if (InputY == -1)
                InputY = -2;*/
            // Main Thrust
            if (m_CanMove)
            {
                if (CGameManager.ins.m_TimePassed == -1)
                    CGameManager.ins.m_TimePassed = 0;
                CGameManager.ins.m_TimePassed += Time.deltaTime;
            }
            else
                CGameManager.ins.m_TimePassed = -1;
            m_currThrust = 0.0f;
            float aclAxis = ETCInput.GetAxis("Vertical");
            if (aclAxis > m_deadZone)
                m_currThrust = aclAxis * m_forwardAcl;
            else if (aclAxis < -m_deadZone)
                m_currThrust = aclAxis * m_backwardAcl;

            // Turning
            m_currTurn = 0.0f;
            //float turnAxis = Input.GetAxis("Horizontal");
            float turnAxis = ETCInput.GetAxis("Horizontal");
            if (Mathf.Abs(turnAxis) > m_deadZone)
                m_currTurn = turnAxis;

            if (m_currThrust < 0) // TODO
                m_currTurn *= -1;
        }
    }
    void FixedUpdate()
    {
        if (m_Player == null)
            return;

        if (NetView.isMine)
        {
            Movement();
            CameraFollow();
        }
        else
        {
            rb.position = Vector3.SmoothDamp(rb.position, posGot, ref posGotVel, 0.1f);
            rb.rotation = Quaternion.Lerp(rb.rotation, rotGot, 10 * Time.deltaTime);
        }
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        int ID = -1;
        Vector3 Pos = Vector3.zero;
        Vector3 Vel = Vector3.zero;
        Quaternion Rot = Quaternion.identity;
        int forward = 0;

        if (stream.isWriting)
        {
            ID = m_Player.m_ID;
            Pos = rb.position;
            Vel = rb.velocity;
            Rot = transform.rotation;
            forward = (int)InputY;
        }

        stream.Serialize(ref ID);
        stream.Serialize(ref Pos);
        stream.Serialize(ref Vel);
        stream.Serialize(ref Rot);
        stream.Serialize(ref m_currThrust);
        
        if (stream.isReading)
        {
            m_Player = CGameManager.ins.m_Players[ID];
            posGot = Pos;
            rb.velocity = Vel;
            rotGot = Rot;
            m_currThrust = forward;
        }
    }

}
