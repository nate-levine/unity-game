using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class Physics : MonoBehaviour
{
    public float mass;
    public float gravityScale;
    public float terminalVelocity;
    public float frictionScale;
    public float dragScale;

    [SerializeField] Vector3 m_velocity;
    [SerializeField] Vector3 m_acceleration;
    private List<Vector3> m_forces;

    void Start()
    {
        m_forces = new List<Vector3>();
    }

    /*It's for this reason that FixedUpdate should be used when applying forces, torques, or other physics-related functions - 
    because you know it will be executed exactly in sync with the physics engine itself.

    Whereas Update() can vary out of step with the physics engine, either faster or slower, depending on how much of a load the
    graphics are putting on the rendering engine at any given time, which - if used for physics - would give correspondingly variant
    physical effects!*/
    void FixedUpdate()
    {
        // forces
        if (!GetComponent<PlayerController>().IsColliding())
        {
            // gravity
            if (Vector3.Dot(-transform.up, GetVelocity()) < terminalVelocity)
                ApplyForce(-transform.up * gravityScale * mass * Time.deltaTime);
            // drag
            ApplyForce(transform.right * Vector3.Dot(transform.right, -GetVelocity()) * dragScale);
        }
        if (GetComponent<PlayerController>().IsColliding())
        {
            // friction
            ApplyForce(transform.right * Vector3.Dot(transform.right, -GetVelocity()) * frictionScale);
        }

        // sum forces
        if (m_forces.Count > 0)
        {
            // sum accelerations
            foreach (Vector3 force in m_forces)
            {
                m_acceleration += force / mass;
            }
            // apply to velocity
            SetVelocity(GetVelocity() + (m_acceleration * Time.deltaTime));
            // reset forces for next frame
            m_forces.Clear();
            m_acceleration = Vector3.zero;
        }
    }
    // getters and setters
    public Vector3 GetVelocity() { return m_velocity; }
    public void SetVelocity(Vector3 velocity) { m_velocity = velocity; }
    public void ApplyForce(Vector3 force){ m_forces.Add(force); }
}
