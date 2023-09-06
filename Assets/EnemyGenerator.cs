using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class EnemyGenerator : MonoBehaviour
{
    // Start is called before the first frame update

    public static float enemySpawnProbabilityMultiplier = 2;
    public List<Enemy> Enemies;

    public static int EnemySpawnPerSection = 1;


    private List<GameObject> TrackPieces;
    void Start()
    {
        int spanwCount = 0;
        while(spanwCount < EnemySpawnPerSection)
        {
            spanwCount++;

            TrackPieces = new List<GameObject>();
            var childTransforms = transform.GetComponentsInChildren<Transform>();
            foreach (Transform g in childTransforms)
            {
                GameObject obj = g.gameObject;
                
                // only care about platform pieces with a ridgid body
                if (obj.CompareTag("Platform") && obj.TryGetComponent<Rigidbody>(out _)) 
                {
                    TrackPieces.Add(g.gameObject);
                }
            }

            if (!TrackPieces.Any())
            {
                return;
            }

            var currentLevel = LevelManager.GetCurrentLevel(); 
                        
            var availableEnemies = Enemies.Where(x => currentLevel >= x.MinSpawnLevel && (currentLevel < x.MaxSpawnLevel || x.MaxSpawnLevel == 0)).ToList();

            var enemyIndexSelect = UnityEngine.Random.Range(0, availableEnemies.Count() * enemySpawnProbabilityMultiplier);

            if (enemyIndexSelect < availableEnemies.Count)
            {
                var trackIndexSelect = UnityEngine.Random.Range(0, TrackPieces.Count);
                var trackTransform = TrackPieces[trackIndexSelect].transform;

                var enemyToCreate = availableEnemies[(int)enemyIndexSelect];

                GameObject newEnemy = null;
                Vector3 newPos = Vector3.zero;
                if (enemyToCreate.IsWorldSpace)
                {
                    newPos = new(
                        UnityEngine.Random.Range(enemyToCreate.x_spawnRange.Min, enemyToCreate.x_spawnRange.Max),
                        UnityEngine.Random.Range(enemyToCreate.y_spawnRange.Min, enemyToCreate.y_spawnRange.Max),
                        UnityEngine.Random.Range(enemyToCreate.z_spawnRange.Min, enemyToCreate.z_spawnRange.Max));

                    newEnemy = Instantiate(enemyToCreate.StaticMesh, new Vector3(newPos.x, newPos.y, transform.position.z + newPos.z), Quaternion.identity);
                }
                else
                {
                    newEnemy = Instantiate(enemyToCreate.StaticMesh, trackTransform);
                    newPos = new(
                        UnityEngine.Random.Range(enemyToCreate.x_spawnRange.Min, enemyToCreate.x_spawnRange.Max),
                        UnityEngine.Random.Range(enemyToCreate.y_spawnRange.Min, enemyToCreate.y_spawnRange.Max),
                        UnityEngine.Random.Range(enemyToCreate.z_spawnRange.Min, enemyToCreate.z_spawnRange.Max));

                    newEnemy.transform.localScale = new Vector3(newEnemy.transform.localScale.x / trackTransform.localScale.x, newEnemy.transform.localScale.y / trackTransform.localScale.y, newEnemy.transform.localScale.z / trackTransform.localScale.z);
                    newEnemy.transform.localPosition = newPos;
                }


            }
        }        
    }
    private void ConvertToWorld(Transform local, out Vector3 worldpos, out Quaternion worldRotation, out Vector3 worldScale)
    {   
        // Convert the local position to world space
        worldpos = local.TransformPoint(Vector3.zero);

        // Convert the local rotation to world space
        worldRotation = local.rotation * Quaternion.identity;

        // Convert the local scale to world space
        worldScale = local.lossyScale;
    }

    [Serializable]
    public class Enemy
    {
        public bool IsWorldSpace;
        public GameObject StaticMesh;
        public int MinSpawnLevel;
        public int MaxSpawnLevel;
        public MinMax x_spawnRange;
        public MinMax y_spawnRange;
        public MinMax z_spawnRange;
    }

    [Serializable]
    public struct MinMax
    { 
        public float Min;
        public float Max;
    }

// Update is called once per frame
void Update()
    {        

    }
}
