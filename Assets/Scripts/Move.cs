using UnityEngine;
using KartGame.KartSystems;

public class Move : MonoBehaviour
{
    public float MoveSpeed;
    public float SideSpeed;

    public int delay = 0;

    public bool IsRelativeToShip = false;

    public bool UseRigidBody = true;
    // Start is called before the first frame update

    private Rigidbody _rb;
    void Start()
    {   
        
    }

    float tick = 0;
    bool hasRan = false;
    private void Update()
    {
        tick += Time.deltaTime * 60;

        if (tick > delay)
        {
            if (UseRigidBody)
            {
                if (!hasRan)
                {
                    RunRigidBody();
                    hasRan = true;
                }
            }
            else
            {
                RunTransformUpdate();
            }
        }
    }

    void RunTransformUpdate()
    {        
        var speed = MoveSpeed * Time.deltaTime * transform.forward;
        if (IsRelativeToShip)
        {
            speed += ArcadeKart.ShipVelocity;
        }
        speed += SideSpeed * Time.deltaTime * transform.right;
        transform.position += speed;
    }

    void RunRigidBody()
    {
        _rb = GetComponent<Rigidbody>();
        var newSpeed = _rb.transform.forward * MoveSpeed;
        newSpeed += (_rb.transform.right * SideSpeed);

        if (IsRelativeToShip)
        {
            newSpeed += ArcadeKart.ShipVelocity;
        }

        _rb.velocity = newSpeed;
    }

}
