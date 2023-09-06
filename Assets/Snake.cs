using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject SnakeSegment;
    
    public int SnakeSize;
    
    public int SpawnDelay;

    public MinMax RotationRange = new MinMax { Min = 0, Max = 360 };
    
    private readonly List<GameObject> SnakeSegments = new List<GameObject>();

    private float rote = 0;

    public int SnakeIndex { get; set; }

    void Start()
    {
        rote = UnityEngine.Random.Range(RotationRange.Min, RotationRange.Max);

        for (int i = 0; i < SnakeSize; i++)
        {
            var createdSegment = SpawnSegment();
            createdSegment.SetActive(false);
            SnakeSegments.Add(createdSegment);
        }   
    }

    float tick = 0;
    int cursor = 0;
    // Update is called once per frame
    private void Update()
    {
        if (cursor >= SnakeSegments.Count) { Destroy(gameObject); return; }

        tick += Time.deltaTime * 60;
        
        if (tick > SpawnDelay)
        {
            SnakeSegments[cursor].SetActive(true);
            cursor++;
            tick = 0;
        }
    }

    GameObject SpawnSegment()
    {   
        var NextSegment = Instantiate(SnakeSegment, transform.position, transform.rotation);
         
        foreach (var c in NextSegment.GetComponentsInChildren<Transform>())
        {
            if (c.gameObject.CompareTag("Rotator"))
            {
                c.gameObject.transform.localRotation = Quaternion.Euler(0, 0, rote);
                // c.transform.rotation = RotatorRotation;
            }
        }

        return NextSegment;
    }
}

