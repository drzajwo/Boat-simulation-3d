using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ship : MonoBehaviour
{
    private Rigidbody _rigidBody;
    private GameObject A;
    private GameObject B;
    public Vector3 centerOfMass;

    public float waterDensity = 1f;
    public float displacedWaterVolume = 0f;
    public float shipDensity = 0.4f;
    public float shipVolume = 1000;

    private Vector3 centerBuoyancy = Vector3.zero;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        A = transform.Find("A").gameObject;
        B = transform.Find("B").gameObject;
    }

    private void FixedUpdate()
    {
        // Get current position and set center of Mass
        _rigidBody.centerOfMass = centerOfMass;
        // little wind
        // AddForceAtPositionWithLine(position, new Vector3(1f, 0f, 0f), Color.yellow);

        // Apply gravity force (0, -9.81, 0)
        AddForceAtCenterOfMassWithLine(Physics.gravity, Color.cyan);
        
        // Calculate and apply Buoyance force
        var force = CalculateBuoyancy();
        AddForceAtPositionWithLine(centerBuoyancy, force, Color.red);
    }

    private void CalculateApproxDisplacedWaterVolume()
    {
        var position = transform.position;
        var aPosition = A.transform.position;
        var bPosition = B.transform.position;
        var localScale = transform.localScale;
        
        var center = position.y;
        var x = localScale.x;
        var y = localScale.y;
        var z = localScale.z;
        // wave height at left and right part of figure
        var centerWaveHeight = Waves.instance.GetWaveHeight(position.x);
        var aWaveHeight = Waves.instance.GetWaveHeight(aPosition.x);
        var bWaveHeight = Waves.instance.GetWaveHeight(bPosition.x);

        // in other words point on y axis where water intersect with figure
        var aPos = A.transform.localPosition;
        var a = y / 2 + (aWaveHeight - center);
        var lengthToA =
            Mathf.Sqrt(
                Mathf.Pow((position.x - aPosition.x), 2) + Mathf.Pow((centerWaveHeight - aPosition.y), 2));
        a += Mathf.Tan(transform.rotation.eulerAngles.z * Mathf.PI / 180) * lengthToA;
        a = Mathf.Clamp01(a);
        aPos.y = a - 0.5f;
        A.transform.localPosition = aPos;

        var bPos = B.transform.localPosition;
        var b = y / 2 + (bWaveHeight - center);
        var lengthToB =
            Mathf.Sqrt(
                Mathf.Pow((position.x - bPosition.x), 2) + Mathf.Pow((centerWaveHeight - bPosition.y), 2));
        b -= Mathf.Tan(transform.rotation.eulerAngles.z * Mathf.PI / 180) * lengthToB;
        b = Mathf.Clamp01(b);
        bPos.y = b - 0.5f;
        B.transform.localPosition = bPos;

        // calculate volume
        var v1 = a <= b ? x * a * z : x * b * z;
        var v2 = a <= b ? x * (b - a) * z / 2 : x * (a - b) * z / 2;
        // transform to cm3
        v1 *= 1000;
        v2 *= 1000;
        displacedWaterVolume = Mathf.Min(shipVolume, (v1 + v2));
    }

    private Vector3 CalculateBuoyancy()
    {
        CalculateApproxDisplacedWaterVolume();
        CalculateBuoyancyCenter();
        var buoyancyForce = Vector3.zero;
        buoyancyForce.y = Mathf.Max(0, (displacedWaterVolume * waterDensity) / (shipVolume * shipDensity));
        // return force in m/s^2
        return buoyancyForce * 10;
    }

    private void CalculateBuoyancyCenter()
    {
        centerBuoyancy = transform.position;
        centerBuoyancy.x += (B.transform.position.y - A.transform.position.y) /5;
        var bottom = centerBuoyancy.y - transform.localScale.y / 2;
        var waveHeightAtX = Waves.instance.GetWaveHeight(centerBuoyancy.x);
        var div = (waveHeightAtX - bottom) / 2;
        centerBuoyancy.y -= (waveHeightAtX - bottom) / 2;
    }

    private void AddForceAtCenterOfMassWithLine(Vector3 forceVector, Color color)
    {
        var position = transform.position + transform.rotation * centerOfMass;
        Debug.DrawLine(position, position + forceVector / 10, color);
        _rigidBody.AddForceAtPosition(forceVector, position, ForceMode.Acceleration);
    }

    private void AddForceAtPositionWithLine(Vector3 position, Vector3 forceVector, Color color)
    {
        var radian = Mathf.Asin(Waves.instance.GetWaveHeight(position.x));
        var degrees = radian * 180 / Mathf.PI;
        forceVector.x *= degrees / 180;
        Debug.DrawLine(position, position + forceVector / 10, color);

        _rigidBody.AddForceAtPosition(forceVector, position, ForceMode.Acceleration);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + transform.rotation * centerOfMass, .1f);
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(centerBuoyancy, .05f);
        Gizmos.color = Color.red;
        if (A != null && B != null)
        {
            Gizmos.DrawSphere(A.transform.position, .05f);
            Gizmos.DrawSphere(B.transform.position, .05f);
        }
    }
}