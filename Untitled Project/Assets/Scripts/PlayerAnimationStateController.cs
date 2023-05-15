using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationStateController : MonoBehaviour
{

    public GameObject wheelRotationBone;
    public float wheelSpeed;
    public GameObject leftHandRotationBone;
    public GameObject rightHandRotationBone;
    public float wheelHandRatio;

    private enum Facing
    {
        Left,
        Right,
    }

    private Animator animator;
    private Facing facing;
    private Vector3 previousPosition;
    private Vector3 currentPosition;

    private void Start()
    {
        animator = GetComponent<Animator>();

        facing = Facing.Right;
        previousPosition = transform.position;
        currentPosition = transform.position;
    }

    void FixedUpdate()
    {
        // rotate wheel and hands dynamically
        currentPosition = wheelRotationBone.transform.position;
        Vector3 velocity = currentPosition - previousPosition;
        Vector3 normal = Vector3.Normalize(transform.position);
        Vector3 tangent = Vector3.Cross(new Vector3(0.0f, 0.0f, -1.0f), Vector3.Normalize(transform.position));
        Vector3 normalVelocity = normal * Vector3.Dot(normal, velocity);
        Vector3 tangentVelocity = tangent * Mathf.Abs(Vector3.Dot(tangent, velocity));
        float tangentVelocityMagnitudeAbsolute = Vector3.Magnitude(tangentVelocity);

        wheelRotationBone.transform.Rotate(new Vector3(0.0f, wheelSpeed * tangentVelocityMagnitudeAbsolute * Time.deltaTime, 0.0f));
        leftHandRotationBone.transform.Rotate(new Vector3(0.0f, wheelSpeed * tangentVelocityMagnitudeAbsolute * Time.deltaTime * wheelHandRatio, 0.0f));
        rightHandRotationBone.transform.Rotate(new Vector3(0.0f, wheelSpeed * tangentVelocityMagnitudeAbsolute * Time.deltaTime * wheelHandRatio, 0.0f));

        // flip model
        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && facing != Facing.Left)
        {
            facing = Facing.Left;
            transform.localScale = new Vector3(transform.localScale.x * -1.0f, transform.localScale.y, transform.localScale.z);
        }
        else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A) && facing != Facing.Right)
        {
            facing = Facing.Right;
            transform.localScale = new Vector3(transform.localScale.x * -1.0f, transform.localScale.y, transform.localScale.z);
        }

        // change animation state
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
        if (Input.GetKey(KeyCode.Space) && Vector3.Dot(normal, normalVelocity) > 0.0f)
        {
            animator.SetBool("didJump", true);
        }
        if (Input.GetKeyUp(KeyCode.Space) || Vector3.Dot(normal, normalVelocity) <= 0.0f)
        {
            animator.SetBool("didJump", false);
        }
        
        previousPosition = currentPosition;
    }
}
