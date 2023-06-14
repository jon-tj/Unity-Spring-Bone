using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/* ABOUT

    Author: Jon Henrik
    Date: 14th of June 2023, on holiday :)

    Description: Makes bones springy and adds drag, stretch and gravity!

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

    [Header("Collision")]
    public float colliderRadius = 0.3f;
    public float colliderPos = 0.7f;
    [Tooltip("How much of the velocity is kept upon collision?")]
    public float elasticity = 0.6f;
    public SphereCollider[] colliders;

    // Private variables
    private Vector3 tip;
    private Vector3 tipVelocity;
    private Vector3 target; // The target is set to the initial rotation.

    private float totalTension;
    private float tensionVelocity;

    private Quaternion prevRotation;
    private Vector3 prevPos;
    private Vector3 prevVelocity;


    // Start is called before the first frame update
    void Start()
    {
        target = transform.forward;
        tip = transform.forward;
        prevPos = transform.position;
        prevRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        // Spring and dampen
        float angle = Vector3.Angle(tip, target) * Time.fixedDeltaTime;
        tipVelocity *= Mathf.Pow(0.8f, dampen);
        tipVelocity += (target - tip).normalized * (angle * spring);

        // Drag
        Vector3 velocity = transform.position - prevPos;
        Vector3 windPulse = Vector3.zero;
        if (PhysicsEnvironment.instance != null)
        {
            windPulse = PhysicsEnvironment.instance.globalWind;
            if (PhysicsEnvironment.instance.windFrequency != 0)
                windPulse *= 0.5f + 0.25f * (
                    Mathf.Sin(PhysicsEnvironment.instance.windFrequency * Time.time) +
                    Mathf.Cos(3.14f * PhysicsEnvironment.instance.windFrequency * Time.time));
        }
        tipVelocity -= (velocity - windPulse) * drag;

        // Gravity
        print(Time.fixedDeltaTime);
        tipVelocity.y -= gravity * Time.fixedDeltaTime;
        tipVelocity = Vector3.ProjectOnPlane(tipVelocity, tip);

        // Append the velocity vector
        float lengthOfBone = tip.magnitude;
        tip += tipVelocity;
        Vector3 tipBeforeNormalized = tip;
        tip = tip.normalized * lengthOfBone;

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


        // Perform the rotation
        tipVelocity = Quaternion.Inverse(prevRotation) * tipVelocity;
        transform.rotation = Quaternion.LookRotation(tip, Vector3.right);
        tipVelocity = transform.rotation * tipVelocity;

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

        prevRotation = transform.rotation;
        prevPos = transform.position;
        prevVelocity = velocity;
    }

    // Debug gizmos
    void OnDrawGizmos()
    {
        if (Selection.activeGameObject != transform.gameObject)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * colliderPos, colliderRadius);
    }
}
