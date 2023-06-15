using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsEnvironment : MonoBehaviour
{
    [Header("Environment")]
    public Vector3 globalWind;
    public float windFrequency;

    public static PhysicsEnvironment instance;
    void Start() { instance = this; }

    public static Vector3 CalculateWind() => CalculateWind(Vector3.zero);
    public static Vector3 CalculateWind(Vector3 position)
    {
        if (PhysicsEnvironment.instance != null)
        {
            Vector3 wind = PhysicsEnvironment.instance.globalWind;
            if (PhysicsEnvironment.instance.windFrequency != 0)
                wind *= 0.5f + 0.25f * (
                    Mathf.Sin(PhysicsEnvironment.instance.windFrequency * Time.time+position.x) +
                    Mathf.Cos(3.14f * PhysicsEnvironment.instance.windFrequency * Time.time+position.z));
            return wind;
        }
        else return Vector3.zero;
    }
}
