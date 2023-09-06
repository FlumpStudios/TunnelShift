using UnityEngine;

public class SineRotate : MonoBehaviour
{
    public Vector3 rotationSpeed;
    public float rotationAmplitude = 0.5f; // adjust this value to control the amplitude of the sine wave
    public float multiplier = 1.0f;
    private float tick = 0;

    void Update()
    {
        tick += Time.deltaTime * multiplier;
        float rotationAngle = Mathf.Sin(tick) * rotationAmplitude;
        Vector3 rotation = rotationSpeed * rotationAngle;
        transform.localRotation *= Quaternion.Euler(rotation);
    }
}
