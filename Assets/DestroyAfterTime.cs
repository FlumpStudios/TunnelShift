using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float DestroyDelay;
    private float tick;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tick = Time.deltaTime * 60;
        if (tick > DestroyDelay)
        {
            Destroy(gameObject);
        }
    }
}
