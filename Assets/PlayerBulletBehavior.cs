using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletBehavior : MonoBehaviour
{
    public GameObject OnDeathEffect;
    public int bulletPower = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject collider)
    {

        if (collider.CompareTag("PlayerBullet") || collider.CompareTag("PlayerMesh")) { return; }

        if (collider.CompareTag("Enemy"))
        {
            var enemy = collider.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                enemy.Attack(bulletPower);
            }
            if (OnDeathEffect != null)
            {
                Instantiate(OnDeathEffect, gameObject.transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
