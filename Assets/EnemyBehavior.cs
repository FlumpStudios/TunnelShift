using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public GameObject ExplosionEffect;
    public int _Health = 3;

    private int _currentHealth; 

    // Start is called before the first frame update
    void Start()
    {
        _currentHealth = _Health;
    }

    // Update is called once per frame
    void Update()
    {
               
    }

    public void Kill()
    {
        if (ExplosionEffect != null)
        { 
            Instantiate(ExplosionEffect, gameObject.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    public void Attack(int power)
    {
        _currentHealth -= power;
        if (_currentHealth < 0)
        {
            Kill();
        }
    }   
}
