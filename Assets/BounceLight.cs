using System;
using UnityEngine;

public class BounceLight : MonoBehaviour
{
    public MinMax IntensityRange;
    public float Speed;    

    private Light _light;
    private bool _bounced = false;
    void Start()
    {
        _light = GetComponent<Light>();
    }
    

    // Update is called once per frame
    void Update()
    {     

        if (_light.intensity > IntensityRange.Max)
        {
            _bounced = true;

        }

        if (_light.intensity < IntensityRange.Min)
        {
            _bounced = false;

        }

        if (_bounced)
        {
            _light.intensity -= (Speed * Time.deltaTime);
        }
        else
        {
            _light.intensity += (Speed * Time.deltaTime);
        }
    }
}

[Serializable]
public struct MinMax
{ 
    public float Min;
    public float Max;
}
