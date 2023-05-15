using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public float jumpDamper;
    public float maximumSlopeAngle;
    public LayerMask chunkLayerMask;

    [SerializeField] bool _isColliding;
    [SerializeField] bool _canJump;
    
    private Rigidbody2D m_rigidBody;
    private Physics m_physics;

    private void Start()
    {
        _isColliding = false;
        _canJump = false;

        m_rigidBody = GetComponent<Rigidbody2D>();
        m_physics = GetComponent<Physics>();

        // orient
        transform.up = new Vector3(0.0f, 1.0f, 0.0f);
        transform.right = -Vector3.Normalize(Vector3.Cross(new Vector3(1.0f, 0.0f, 0.0f), transform.up));
    }

    void FixedUpdate()
    {
        // move
        m_rigidBody.MovePosition(transform.position + (m_physics.GetVelocity() * Time.deltaTime));

        // input
        // move left
        if (Input.GetKey(KeyCode.A)) m_physics.ApplyForce(-transform.right * speed);
        // move right
        if (Input.GetKey(KeyCode.D)) m_physics.ApplyForce(transform.right * speed);
        // jump
        if (Input.GetKey(KeyCode.Space) && IsColliding() && _canJump)
        {
            // reset vertical velocity right before jump to maintain a consistent jump height
            m_physics.SetVelocity(transform.right * Vector3.Dot(transform.right, m_physics.GetVelocity()));
            // apply jump force
            m_physics.ApplyForce(transform.up * jumpForce);
            _canJump = false;
        }
        // jump damper
        if (Input.GetKeyUp(KeyCode.Space) && Vector3.Dot(transform.up, m_physics.GetVelocity()) > 0.0f)
        {
            m_physics.SetVelocity((transform.up * Vector3.Dot(transform.up, m_physics.GetVelocity()) * (1.0f - jumpDamper)) + (transform.right * Vector3.Dot(transform.right, m_physics.GetVelocity()) * 1.0f));
        }

        // this is for an edge case where there is suddenly no collider under the player. Such as when a big chunk of the world is removed all at once
        if (!Physics2D.CircleCast(transform.position, 0.55f, transform.up, 0.0f, chunkLayerMask))
        {
            _canJump = false;
            _isColliding = false;
        }

        // orient
        transform.up = Vector3.Normalize(new Vector3(transform.position.x, transform.position.y, 0.0f));
        transform.right = -Vector3.Normalize(Vector3.Cross(new Vector3(0.0f, 0.0f, 1.0f), transform.up));
        transform.LookAt(new Vector3(transform.position.x, transform.position.y, 0.0f), transform.up);
    }
    // check is player is colliding with anything
    public bool IsColliding() { return _isColliding; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null)
        {
            // if at least one of the normals are angled enough to the ground, count the collision
            foreach (ContactPoint2D contactPoint in collision.contacts)
            {
                if (Vector3.Dot(transform.up, contactPoint.normal) >= Mathf.Cos(maximumSlopeAngle * Mathf.PI / 180.0f))
                {
                    m_physics.SetVelocity(transform.right * Vector3.Dot(transform.right, m_physics.GetVelocity()));
                }
            }
        }
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision != null)
        {
            // if at least one of the normals are angled enough to the ground, count the collision
            int validGroundings = 0;
            foreach (ContactPoint2D contactPoint in collision.contacts)
            {
                if (Vector3.Dot(transform.up, contactPoint.normal) >= Mathf.Cos(maximumSlopeAngle * Mathf.PI / 180.0f))
                {
                    validGroundings++;
                }
            }
            if (validGroundings > 0)
            {
                // cancel normal velocity due to gravity
                if (!_isColliding && !_canJump)
                    m_physics.SetVelocity(transform.right * Vector3.Dot(transform.right, m_physics.GetVelocity()));

                _isColliding = true;

                // alow jumping
                if (!_canJump)
                    _canJump = true;
            }
            else
            {
                _canJump = false;
                _isColliding = false;
            }
        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision != null)
        {
            _canJump = false;
            _isColliding = false;
        }
    }
}
