using UnityEngine;
using System.Collections;

public class CCar : MonoBehaviour {

    public CPlayer m_Player = null;
    public Vector3 CameraOffset;
    public float maxSpeed = 10f;
    public float Accel = 0.5f;

    Rigidbody rb;
    NetworkView NetView;
    Transform m_Camera;
    Transform m_Mesh;
    float meshStartY;
    ParticleSystem[] m_Particles;

    bool firstData = true;
    float m_Speed;

    float InputX = 0;
    float InputY = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        NetView = GetComponent<NetworkView>();
        m_Camera = Camera.main.transform;
        m_Mesh = transform.FindChild("Mesh");
        meshStartY = m_Mesh.localPosition.y;
        m_Particles = GetComponentsInChildren<ParticleSystem>();
    }


    void CameraFollow()
    {
        Vector3 temp = Vector3.zero;
        m_Camera.position = Vector3.SmoothDamp(m_Camera.position, rb.position - transform.TransformDirection(CameraOffset), ref temp, 0.05f);
        m_Camera.LookAt(transform);
    }
    void Movement()
    {
        rb.velocity = transform.forward * m_Speed;
        m_Speed = Mathf.Clamp(m_Speed + InputY * Time.deltaTime * Accel, -maxSpeed/2f, maxSpeed);
        if (InputY == 0)
            m_Speed *= 0.99f;
        transform.Rotate(0, InputX * 65 * Time.deltaTime, 0);
    }

    void Update()
    {
        if (m_Player == null)
            return;

        m_Mesh.localPosition = Vector3.up * (Mathf.Sin(Time.timeSinceLevelLoad * 2) * 0.2f + meshStartY);

        if (InputY > 0)
        {
            foreach (ParticleSystem part in m_Particles)
                part.Emit((int)(200 * Time.deltaTime));
        }

        if (NetView.isMine)
        {
            InputX = Input.GetAxis("Horizontal");
            InputY = Input.GetAxisRaw("Vertical");
            if (InputY == -1)
                InputY = -2;
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
        stream.Serialize(ref forward);
        
        if (stream.isReading)
        {
            m_Player = CGameManager.ins.m_Players[ID];
            rb.position = Pos;
            rb.velocity = Vel;
            rb.rotation = Rot;
            InputY = forward;
        }
    }

}
