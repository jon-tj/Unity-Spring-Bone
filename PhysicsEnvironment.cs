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
}
