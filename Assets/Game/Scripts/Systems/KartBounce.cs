using UnityEngine;

namespace KartGame.KartSystems
{

    [RequireComponent(typeof(ArcadeKart))]
    public class KartBounce : MonoBehaviour
    {
        public bool BounceFlag { get; private set; }

        public float BounceFactor = 10f;
        public float RotationSpeed = 3f;
        public LayerMask CollisionLayer;
        public float RayDistance = 1f;
        public float PauseTime = 0.5f;
        public float HeightOffset;
        public float[] Angles;

        public bool DrawGizmos;
        public AudioClip BounceSound;

        ArcadeKart kart;
        float resumeTime;
        bool hasCollided;
        Vector3 reflectionVector;

        void Start()
        {
            kart = GetComponent<ArcadeKart>();
        }

        void Update()
        {
            // Reset the trigger flag
            if (BounceFlag)
            {
                BounceFlag = false;
            }
            Vector3 origin = transform.position;
            origin.y += HeightOffset;

            for (int i = 0; i < Angles.Length; i++)
            {
                Vector3 direction = GetDirectionFromAngle(Angles[i], Vector3.up, transform.forward);

                if (Physics.Raycast(origin, direction, out RaycastHit hit, RayDistance, CollisionLayer) && Time.time > resumeTime && !hasCollided && kart.LocalSpeed() > 0)
                {
                    // If the hit normal is pointing up, then we don't want to bounce
                    if (Vector3.Dot(hit.normal, Vector3.up) > 0.2f) 
                    { 
                        return;
                    }

                    // Calculate the incident vector of the kart colliding into whatever object
                    Vector3 incidentVector =  hit.point - origin;

                    // Calculate the reflection vector using the incident vector of the collision
                    Vector3 hitNormal = hit.normal.normalized;
                    reflectionVector = incidentVector - 2 * Vector3.Dot(incidentVector, hitNormal) * hitNormal;
                    reflectionVector.y = 0;

                    kart.Rigidbody.velocity /= 2;
                    // Apply the bounce impulse with the reflectionVector
                    kart.Rigidbody.AddForce(reflectionVector.normalized * BounceFactor, ForceMode.Impulse);

                    // Mark that the vehicle has collided and the reset time.
                    kart.SetCanMove(false);
                    BounceFlag = hasCollided = true;
                    resumeTime = Time.time + PauseTime;

                    if (BounceSound)
                    {
                        AudioUtility.CreateSFX(BounceSound, transform.position, AudioUtility.AudioGroups.Collision, 0f);
                    }
                    return;
                }
            }

            if (Time.time < resumeTime)
            {
                Vector3 targetPos         = origin + reflectionVector;
                Vector3 direction         = targetPos - origin;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                kart.transform.rotation   = Quaternion.Slerp(kart.transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            }
        }

        void LateUpdate()
        {
            if (Time.time > resumeTime && hasCollided) 
            {
                kart.SetCanMove(true);
                hasCollided = false;
            }
        }

        void OnDrawGizmos()
        {
            if (DrawGizmos)
            {
                Vector3 origin = transform.position;
                origin.y += HeightOffset;

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(origin, origin + transform.forward);
                Gizmos.color = Color.red;
                for (int i = 0; i < Angles.Length; i++)
                {
                    var direction = GetDirectionFromAngle(Angles[i], Vector3.up, transform.forward);
                    Gizmos.DrawLine(origin, origin + (direction.normalized * RayDistance));
                }
            }
        }

        Vector3 GetDirectionFromAngle(float degrees, Vector3 axis, Vector3 zerothDirection)
        {
            Quaternion rotation = Quaternion.AngleAxis(degrees, axis);
            return (rotation * zerothDirection);
        }
    }
}
