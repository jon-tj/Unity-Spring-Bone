using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/* ABOUT

    Author: Jon Henrik
    Date: 14th of June 2023, on holiday :)

    Description: Makes bones springy and adds drag, stretch and gravity!

    Dependencies:
        ln 72 :  PhysicsEnvironment.CalculateWind()

 */

public class SpringBone : MonoBehaviour
{
    // Public variables
    [Header("Spring Properties")]
    public float spring = 0.1f;
    public float dampen = 0.3f;
    public float drag = 0.15f;
    public float stretch = 0.1f;
    public float gravity = 1f;

    [Header("Limits")]
    public float limitAngle= -1; // no limit

    [Header("Collision")]
    public float colliderRadius = 0.3f;
    public float colliderPos = 0.7f;
    [Tooltip("How much of the velocity is kept upon collision?")]
    public float elasticity = 0.6f;
    public SphereCollider[] colliders;

    // Private variables
    private Vector3 tip;
    private Vector3 tipVelocity;
    private Vector3 target;

    private float totalTension;
    private float tensionVelocity;

    private Quaternion prevRotation;
    private Vector3 prevPos;
    private Vector3 prevVelocity;

    void Start()
    {
        // Initialization

        // Target is set to the initial rotation, but could alternatively
        // be set to some public vector. Make sure the vector is normalized.
        target = transform.forward;
        tip = transform.forward;
        prevPos = transform.position;
        prevRotation = transform.rotation;
        iniRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        // Spring and dampen
        float angle = Vector3.Angle(tip, target);
        tipVelocity *= Mathf.Pow(0.8f, dampen);
        tipVelocity += (target - tip).normalized * (angle* Time.fixedDeltaTime * spring);

        // Drag
        Vector3 velocity = transform.position - prevPos;
        Vector3 wind = PhysicsEnvironment.CalculateWind(transform.position);
        tipVelocity -= (velocity - wind) * drag;

        // Gravity
        tipVelocity.y -= gravity * Time.fixedDeltaTime;
        tipVelocity = Vector3.ProjectOnPlane(tipVelocity, tip);

        // Append the velocity vector
        tip += tipVelocity;
        tip = tip.normalized;

        // Tension stretching
        Vector3 acceleration = velocity - prevVelocity;
        float tension = Vector3.Dot(tip, acceleration);

        tensionVelocity += tension * stretch;
        tensionVelocity -= totalTension * spring* Time.fixedDeltaTime*20;
        tensionVelocity *= Mathf.Pow(0.8f, dampen);
        totalTension += tensionVelocity;
        if (totalTension < -0.5f)
        {
            totalTension = -0.5f;
            tensionVelocity = 0;
        }
        if (totalTension > 0.5f)
        {
            totalTension = 0.5f;
            tensionVelocity = 0;
        }
        transform.localScale = new Vector3(1-totalTension,1-totalTension,  1 + totalTension);

        // Colliders
        Vector3 colliderPos3 = transform.position + transform.forward * colliderPos;
        for (int i=0; i<colliders.Length; i++)
        {
            Vector3 normal = colliderPos3 - colliders[i].transform.position;
            float distCenter = normal.magnitude;
            float distSurface = distCenter - colliderRadius - colliders[i].radius;
            if (distSurface < 0)
            {
                normal = normal.normalized * distSurface;
                tip = (tip*colliderPos - normal).normalized;
                tipVelocity *= -elasticity;
            }
        }

        // Angular limit
        angle = Vector3.Angle(tip, target);
        if (limitAngle >= 0 && angle > limitAngle)
        {
            tip = Vector3.ProjectOnPlane(tip, target);
            if (tip.magnitude > 0.00001f) // Cannot do cross product on zero vector
            {
                Vector3 axis = Vector3.Cross(tip, target);
                Quaternion rotation = Quaternion.AngleAxis(90 - limitAngle, axis);
                tip = rotation * tip;
                tipVelocity *= -elasticity;
            }
        }

        // Perform the rotation
        tipVelocity = Quaternion.Inverse(prevRotation) * tipVelocity;
        transform.rotation = Quaternion.LookRotation(tip, Vector3.right);
        tipVelocity = transform.rotation * tipVelocity;

        prevRotation = transform.rotation;
        prevPos = transform.position;
        prevVelocity = velocity;
    }

    // Debug gizmos
    internal Color colliderColor = new(0.53f, 0.81f, 0.5f);
    internal Quaternion iniRotation;
    void OnDrawGizmos()
    {
        if (Selection.activeGameObject != transform.gameObject)
            return;
        
        // Our sphere collider
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * colliderPos, colliderRadius);

        // Rendering the registered colliders
        Gizmos.color = colliderColor;
        for (int i = 0; i < colliders.Length; i++)
            Gizmos.DrawWireSphere(colliders[i].transform.position, colliders[i].radius);

        // Rendering the angle limit as spokes with a certain angle.
        if (iniRotation.x == 0 && iniRotation.y == 0) iniRotation = transform.rotation;
        if (limitAngle >= 0)
        {
            Vector3 ttarget = transform.forward;
            if (target != Vector3.zero) ttarget = target;
            float angleBetweenSpokes = 6.283f / 10;
            for (float i = 0; i < 6.283f; i += angleBetweenSpokes)
            {
                Vector3 spokeDir = iniRotation * new Vector3(Mathf.Sin(i), Mathf.Cos(i), 0);
                Vector3 axis = Vector3.Cross(spokeDir, ttarget);
                Quaternion rotation = Quaternion.AngleAxis(90-limitAngle, axis);
                Gizmos.DrawLine(transform.position, transform.position + rotation*spokeDir);
            }
            Handles.color = colliderColor;
            Handles.DrawWireDisc(
                transform.position+ttarget* Mathf.Cos(limitAngle * Mathf.Deg2Rad),
                ttarget,
                Mathf.Sin(limitAngle*Mathf.Deg2Rad));
        }
    }
}
