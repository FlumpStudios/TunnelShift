using UnityEngine;
using KartGame.KartSystems;

    public class PlayerBulletSpawner : MonoBehaviour
    {
        public static bool ShouldSpawnBullet = false;
    
        
        public GameObject Bullet;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (ArcadeKart.FirePressed)
            {
                if (ArcadeKart.bulletSpawnCount < ArcadeKart.MaxNumberOfShotsAtOneTime)
                {
                    Instantiate(Bullet, transform.position, Quaternion.identity);
                    ArcadeKart.bulletSpawnCount++;
                }
            }
        }
    }

