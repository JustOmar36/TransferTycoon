using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;       // The target object to orbit around
    public float radius = 5f;       // Distance from the target
    public float speed = 10f;       // Orbit speed in degrees per second

    private float angle = 0f;       // Current angle around the target

    void Update()
    {
        if (target == null) return;

        // Increment angle over time based on speed
        angle += speed * Time.deltaTime;

        // Calculate new position using polar coordinates
        float rad = angle * Mathf.Deg2Rad;
        float x = target.position.x + Mathf.Cos(rad) * radius;
        float z = target.position.z + Mathf.Sin(rad) * radius;

        // Set the camera's position (maintaining current y height)
        transform.position = new Vector3(x, transform.position.y, z);

        // Make the camera look at the target
        transform.LookAt(target);
    }
}

/*
Usage:
- Attach this script to your camera GameObject in Unity.
- Assign the target GameObject (the object to orbit) in the Inspector.
- Set the desired radius and speed values.
- The camera will orbit horizontally around the target while keeping its current height.
*/
