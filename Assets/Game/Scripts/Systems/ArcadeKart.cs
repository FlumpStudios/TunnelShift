using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;

namespace KartGame.KartSystems
{
    public class ArcadeKart : MonoBehaviour
    {

        public const int MaxNumberOfShotsAtOneTime = 2;
        public static int bulletSpawnCount = 0;

        public static float baseTopSpeed = 0;
        public static float baseSteer = 0;
        public static Vector3 ShipVelocity = Vector3.zero;
        public static bool FirePressed = false;

        const bool ALWAYS_ACCEL = true;
        [System.Serializable]
        public class StatPowerup
        {
            public ArcadeKart.Stats modifiers;
            public string PowerUpID;
            public float ElapsedTime;
            public float MaxTime;
        }

        [System.Serializable]
        public struct Stats
        {
            public GameObject SteeringWheel;
            
            [Header("Movement Settings")]
            [Min(0.001f), Tooltip("Top speed attainable when moving forward.")]
            public float TopSpeed;

            [Tooltip("How quickly the kart reaches top speed.")]
            public float Acceleration;

            [Min(0.001f), Tooltip("Top speed attainable when moving backward.")]
            public float ReverseSpeed;

            [Tooltip("How quickly the kart reaches top speed, when moving backward.")]
            public float ReverseAcceleration;

            [Tooltip("How quickly the kart starts accelerating from 0. A higher number means it accelerates faster sooner.")]
            [Range(0.2f, 1)]
            public float AccelerationCurve;

            [Tooltip("How quickly the kart slows down when the brake is applied.")]
            public float Braking;

            [Tooltip("How quickly the kart will reach a full stop when no inputs are made.")]
            public float CoastingDrag;

            [Range(0.0f, 1.0f)]
            [Tooltip("The amount of side-to-side friction.")]
            public float Grip;

            [Tooltip("How tightly the kart can turn left or right.")]
            public float Steer;

            [Tooltip("Additional gravity for when the kart is in the air.")]
            public float AddedGravity;

            [Tooltip("Height of jump")]
            public int JumpHeight;

            // allow for stat adding for powerups.
            public static Stats operator +(Stats a, Stats b)
            {
                return new Stats
                {
                    Acceleration = a.Acceleration + b.Acceleration,
                    AccelerationCurve = a.AccelerationCurve + b.AccelerationCurve,
                    Braking = a.Braking + b.Braking,
                    CoastingDrag = a.CoastingDrag + b.CoastingDrag,
                    AddedGravity = a.AddedGravity + b.AddedGravity,
                    Grip = a.Grip + b.Grip,
                    ReverseAcceleration = a.ReverseAcceleration + b.ReverseAcceleration,
                    ReverseSpeed = a.ReverseSpeed + b.ReverseSpeed,
                    TopSpeed = a.TopSpeed + b.TopSpeed,
                    Steer = a.Steer + b.Steer,
                    JumpHeight = a.JumpHeight + b.JumpHeight
                };
            }
        }

        public Rigidbody Rigidbody { get; private set; }
        public InputData Input { get; private set; }
        public float AirPercent { get; private set; }
        public float GroundPercent { get; private set; }



        public ArcadeKart.Stats baseStats = new ArcadeKart.Stats
        {
            TopSpeed = 10f,
            Acceleration = 5f,
            AccelerationCurve = 4f,
            Braking = 10f,
            ReverseAcceleration = 5f,
            ReverseSpeed = 5f,
            Steer = 5f,
            CoastingDrag = 4f,
            Grip = .95f,
            AddedGravity = 1f,
            JumpHeight = 100000
        };

        [Header("Vehicle Visual")]
        public List<GameObject> m_VisualWheels;

        [Header("Vehicle Physics")]
        [Tooltip("The transform that determines the position of the kart's mass.")]
        public Transform CenterOfMass;

        [Range(0.0f, 20.0f), Tooltip("Coefficient used to reorient the kart in the air. The higher the number, the faster the kart will readjust itself along the horizontal plane.")]
        public float AirborneReorientationCoefficient = 3.0f;

        [Header("Drifting")]
        [Range(0.01f, 1.0f), Tooltip("The grip value when drifting.")]
        public float DriftGrip = 0.4f;
        [Range(0.0f, 10.0f), Tooltip("Additional steer when the kart is drifting.")]
        public float DriftAdditionalSteer = 5.0f;
        [Range(1.0f, 30.0f), Tooltip("The higher the angle, the easier it is to regain full grip.")]
        public float MinAngleToFinishDrift = 10.0f;
        [Range(0.01f, 0.99f), Tooltip("Mininum speed percentage to switch back to full grip.")]
        public float MinSpeedPercentToFinishDrift = 0.5f;
        [Range(1.0f, 20.0f), Tooltip("The higher the value, the easier it is to control the drift steering.")]
        public float DriftControl = 10.0f;
        [Range(0.0f, 20.0f), Tooltip("The lower the value, the longer the drift will last without trying to control it by steering.")]
        public float DriftDampening = 10.0f;

        [Header("VFX")]
        [Tooltip("VFX that will be placed on the wheels when drifting.")]
        public ParticleSystem DriftSparkVFX;
        [Range(0.0f, 0.2f), Tooltip("Offset to displace the VFX to the side.")]
        public float DriftSparkHorizontalOffset = 0.1f;
        [Range(0.0f, 90.0f), Tooltip("Angle to rotate the VFX.")]
        public float DriftSparkRotation = 17.0f;
        [Tooltip("VFX that will be placed on the wheels when drifting.")]
        public GameObject DriftTrailPrefab;
        [Range(-0.1f, 0.1f), Tooltip("Vertical to move the trails up or down and ensure they are above the ground.")]
        public float DriftTrailVerticalOffset;
        [Tooltip("VFX that will spawn upon landing, after a jump.")]
        public GameObject JumpVFX;
        [Tooltip("VFX that is spawn on the nozzles of the kart.")]
        public GameObject NozzleVFX;
        [Tooltip("List of the kart's nozzles.")]
        public List<Transform> Nozzles;

        [Header("Suspensions")]
        [Tooltip("The maximum extension possible between the kart's body and the wheels.")]
        [Range(0.0f, 1.0f)]
        public float SuspensionHeight = 0.2f;
        [Range(10.0f, 100000.0f), Tooltip("The higher the value, the stiffer the suspension will be.")]
        public float SuspensionSpring = 20000.0f;
        [Range(0.0f, 5000.0f), Tooltip("The higher the value, the faster the kart will stabilize itself.")]
        public float SuspensionDamp = 500.0f;
        [Tooltip("Vertical offset to adjust the position of the wheels relative to the kart's body.")]
        [Range(-1.0f, 1.0f)]
        public float WheelsPositionVerticalOffset = 0.0f;

        [Header("Physical Wheels")]
        [Tooltip("The physical representations of the Kart's wheels.")]
        public WheelCollider FrontLeftWheel;
        public WheelCollider FrontRightWheel;
        public WheelCollider RearLeftWheel;
        public WheelCollider RearRightWheel;

        [Tooltip("Which layers the wheels will detect.")]
        public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

        // the input sources that can control the kart
        IInput[] m_Inputs;

        const float k_NullInput = 0.01f;
        const float k_NullSpeed = 0.01f;
        Vector3 m_VerticalReference = Vector3.up;

        // Drift params
        public bool WantsToDrift { get; private set; } = false;
        public bool IsDrifting { get; private set; } = false;
        float m_CurrentGrip = 1.0f;
        float m_DriftTurningPower = 0.0f;
        float m_PreviousGroundPercent = 1.0f;
        readonly List<(GameObject trailRoot, WheelCollider wheel, TrailRenderer trail)> m_DriftTrailInstances = new List<(GameObject, WheelCollider, TrailRenderer)>();
        readonly List<(WheelCollider wheel, float horizontalOffset, float rotation, ParticleSystem sparks)> m_DriftSparkInstances = new List<(WheelCollider, float, float, ParticleSystem)>();

        // can the kart move?
        bool m_CanMove = true;
        List<StatPowerup> m_ActivePowerupList = new List<StatPowerup>();
        ArcadeKart.Stats m_FinalStats;

        Quaternion m_LastValidRotation;
        Vector3 m_LastValidPosition;
        Vector3 m_LastCollisionNormal;
        bool m_HasCollision;
        bool m_InAir = false;

        public void AddPowerup(StatPowerup statPowerup) => m_ActivePowerupList.Add(statPowerup);
        public void SetCanMove(bool move) => m_CanMove = move;
        public float GetMaxSpeed() => Mathf.Max(m_FinalStats.TopSpeed, m_FinalStats.ReverseSpeed);

        private void ActivateDriftVFX(bool active)
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                if (active && vfx.wheel.GetGroundHit(out WheelHit hit))
                {
                    if (!vfx.sparks.isPlaying)
                        vfx.sparks.Play();
                }
                else
                {
                    if (vfx.sparks.isPlaying)
                        vfx.sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }

            }

            foreach (var trail in m_DriftTrailInstances)
                trail.Item3.emitting = active && trail.wheel.GetGroundHit(out WheelHit hit);
        }

        private void UpdateDriftVFXOrientation()
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                vfx.sparks.transform.position = vfx.wheel.transform.position - (vfx.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up) + (transform.right * vfx.horizontalOffset);
                vfx.sparks.transform.rotation = transform.rotation * Quaternion.Euler(0.0f, 0.0f, vfx.rotation);
            }

            foreach (var trail in m_DriftTrailInstances)
            {
                trail.trailRoot.transform.position = trail.wheel.transform.position - (trail.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up);
                trail.trailRoot.transform.rotation = transform.rotation;
            }
        }

        void UpdateSuspensionParams(WheelCollider wheel)
        {
            wheel.suspensionDistance = SuspensionHeight;
            wheel.center = new Vector3(0.0f, WheelsPositionVerticalOffset, 0.0f);
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = SuspensionSpring;
            spring.damper = SuspensionDamp;
            wheel.suspensionSpring = spring;
        }

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            m_Inputs = GetComponents<IInput>();

            UpdateSuspensionParams(FrontLeftWheel);
            UpdateSuspensionParams(FrontRightWheel);
            UpdateSuspensionParams(RearLeftWheel);
            UpdateSuspensionParams(RearRightWheel);

            m_CurrentGrip = baseStats.Grip;

            if (DriftSparkVFX != null)
            {
                AddSparkToWheel(RearLeftWheel, -DriftSparkHorizontalOffset, -DriftSparkRotation);
                AddSparkToWheel(RearRightWheel, DriftSparkHorizontalOffset, DriftSparkRotation);
            }

            if (DriftTrailPrefab != null)
            {
                AddTrailToWheel(RearLeftWheel);
                AddTrailToWheel(RearRightWheel);
            }

            if (NozzleVFX != null)
            {
                foreach (var nozzle in Nozzles)
                {
                    Instantiate(NozzleVFX, nozzle, false);
                }
            }
        }

        void AddTrailToWheel(WheelCollider wheel)
        {
            GameObject trailRoot = Instantiate(DriftTrailPrefab, gameObject.transform, false);
            TrailRenderer trail = trailRoot.GetComponentInChildren<TrailRenderer>();
            trail.emitting = false;
            m_DriftTrailInstances.Add((trailRoot, wheel, trail));
        }

        void AddSparkToWheel(WheelCollider wheel, float horizontalOffset, float rotation)
        {
            GameObject vfx = Instantiate(DriftSparkVFX.gameObject, wheel.transform, false);
            ParticleSystem spark = vfx.GetComponent<ParticleSystem>();
            spark.Stop();
            m_DriftSparkInstances.Add((wheel, horizontalOffset, -rotation, spark));
        }

        int jumpCount = 0;

        void FixedUpdate()
        {
            ShipVelocity = Rigidbody.velocity;
            baseStats.TopSpeed = baseTopSpeed;
            if (baseStats.Steer < 400)
            {
                baseStats.Steer = baseSteer;
            }

            UpdateSuspensionParams(FrontLeftWheel);
            UpdateSuspensionParams(FrontRightWheel);
            UpdateSuspensionParams(RearLeftWheel);
            UpdateSuspensionParams(RearRightWheel);

            GatherInputs();

            // apply our powerups to create our finalStats
            TickPowerups();

            // apply our physics properties
            Rigidbody.centerOfMass = transform.InverseTransformPoint(CenterOfMass.position);

            int groundedCount = 0;
            if (FrontLeftWheel.isGrounded && FrontLeftWheel.GetGroundHit(out WheelHit hit))
                groundedCount++;
            if (FrontRightWheel.isGrounded && FrontRightWheel.GetGroundHit(out hit))
                groundedCount++;
            if (RearLeftWheel.isGrounded && RearLeftWheel.GetGroundHit(out hit))
                groundedCount++;
            if (RearRightWheel.isGrounded && RearRightWheel.GetGroundHit(out hit))
                groundedCount++;

            // calculate how grounded and airborne we are
            GroundPercent = (float)groundedCount / 4.0f;
            AirPercent = 1 - GroundPercent;

            // apply vehicle physics
            if (m_CanMove)
            {
                MoveVehicle(Input.Jump, Input.Accelerate, Input.Brake, Input.TurnInput, Input.SwitchGravity, Input.Fire);
            }
            GroundAirbourne();

            m_PreviousGroundPercent = GroundPercent;

            UpdateDriftVFXOrientation();
        }

        void GatherInputs()
        {
            // reset input
            if (m_Inputs == null) { return; }
            Input = new InputData();
            WantsToDrift = false;

            // gather nonzero input from our sources
            for (int i = 0; i < m_Inputs.Length; i++)
            {
                Input = m_Inputs[i].GenerateInput();
                WantsToDrift = Input.Brake && Vector3.Dot(Rigidbody.velocity, transform.forward) > 0.0f;
            }
        }

        void TickPowerups()
        {
            // remove all elapsed powerups
            m_ActivePowerupList.RemoveAll((p) => { return p.ElapsedTime > p.MaxTime; });

            // zero out powerups before we add them all up
            var powerups = new Stats();

            // add up all our powerups
            for (int i = 0; i < m_ActivePowerupList.Count; i++)
            {
                var p = m_ActivePowerupList[i];

                // add elapsed time
                p.ElapsedTime += Time.fixedDeltaTime;

                // add up the powerups
                powerups += p.modifiers;
            }

            // add powerups to our final stats
            m_FinalStats = baseStats + powerups;

            // clamp values in finalstats
            m_FinalStats.Grip = Mathf.Clamp(m_FinalStats.Grip, 0, 1);
        }

        void GroundAirbourne()
        {
            // while in the air, fall faster

            Rigidbody.velocity += Physics.gravity * Time.fixedDeltaTime * m_FinalStats.AddedGravity;

        }

        public void Reset()
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = euler.z = 0f;
            transform.rotation = Quaternion.Euler(euler);
        }

        public float LocalSpeed()
        {
            if (m_CanMove)
            {
                float dot = Vector3.Dot(transform.forward, Rigidbody.velocity);
                if (Mathf.Abs(dot) > 0.1f)
                {
                    float speed = Rigidbody.velocity.magnitude;
                    return dot < 0 ? -(speed / m_FinalStats.ReverseSpeed) : (speed / m_FinalStats.TopSpeed);
                }
                return 0f;
            }
            else
            {
                // use this value to play kart sound when it is waiting the race start countdown.
                return Input.Accelerate ? 1.0f : 0.0f;
            }
        }

        void OnCollisionEnter(Collision collision) => m_HasCollision = true;
        void OnCollisionExit(Collision collision) => m_HasCollision = false;

        private int rotationToFloorSpeed = 12;
        void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Platform"))
            {
                Rigidbody.transform.rotation = Quaternion.Slerp(Rigidbody.transform.rotation, collision.transform.rotation, Time.fixedDeltaTime * rotationToFloorSpeed); // speed of rotation to new plane
                _gravityUpUnitVector = collision.transform.up;
            }

            m_HasCollision = true;
            m_LastCollisionNormal = Vector3.zero;
            float dot = -1.0f;

            foreach (var contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > dot)
                    m_LastCollisionNormal = contact.normal;
            }
        }

        private Vector3 _gravityUpUnitVector = Vector3.zero;
        private static bool isInSmash = false;

        public static int FireDelay = 10;
        int fireTick = FireDelay;

        const int MAX_WHEEL_ANGLE = 70;

        void MoveVehicle(bool jump, bool accelerate, bool brake, float turnInput, bool switchGrav, bool fire)
        {
            baseStats.SteeringWheel.transform.localEulerAngles = new Vector3(0, 0, (turnInput * MAX_WHEEL_ANGLE) * -1);

            if (ALWAYS_ACCEL)
            {
                accelerate = true;
            }


            fireTick++;
            if (fire && fireTick >= FireDelay && Rigidbody.velocity.z > 0)
            {
                bulletSpawnCount = 0;
                FirePressed = true;
                fireTick = 0;
            }
            else

            {
                FirePressed = false;
            }

            float accelInput = (accelerate ? 1.0f : 0.0f) - (brake ? 1.0f : 0.0f);

            // manual acceleration curve coefficient scalar
            float accelerationCurveCoeff = 5;
            Vector3 localVel = transform.InverseTransformVector(Rigidbody.velocity);

            bool accelDirectionIsFwd = accelInput >= 0;
            bool localVelDirectionIsFwd = localVel.z >= 0;

            // use the max speed for the direction we are going--forward or reverse.
            float maxSpeed = localVelDirectionIsFwd ? m_FinalStats.TopSpeed : m_FinalStats.ReverseSpeed;
            float accelPower = accelDirectionIsFwd ? m_FinalStats.Acceleration : m_FinalStats.ReverseAcceleration;

            float currentSpeed = Rigidbody.velocity.magnitude;
            float accelRampT = currentSpeed / maxSpeed;
            float multipliedAccelerationCurve = m_FinalStats.AccelerationCurve * accelerationCurveCoeff;
            float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

            bool isBraking = (localVelDirectionIsFwd && brake) || (!localVelDirectionIsFwd && accelerate);

            // if we are braking (moving reverse to where we are going)
            // use the braking accleration instead
            float finalAccelPower = isBraking ? m_FinalStats.Braking : accelPower;

            float finalAcceleration = finalAccelPower * accelRamp;

            // apply inputs to forward/backward

            // Quaternion turnAngle = Quaternion.AngleAxis(turningPower, transform.up);
            // Vector3 fwd = turnAngle * transform.forward;
            Vector3 movement = transform.forward * accelInput * finalAcceleration * ((m_HasCollision || GroundPercent > 0.0f) ? 1.0f : 0.0f);

            // forward movement
            bool wasOverMaxSpeed = currentSpeed >= maxSpeed;

            // if over max speed, cannot accelerate faster.
            if (wasOverMaxSpeed && !isBraking)
                movement *= 0.0f;

            Vector3 newVelocity = Rigidbody.velocity + movement * Time.fixedDeltaTime;
            newVelocity.y = Rigidbody.velocity.y;

            //  clamp max speed if we are on ground
            if (GroundPercent > 0.0f && !wasOverMaxSpeed)
            {
                newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
                isInSmash = false;
                Rigidbody.angularVelocity = Vector3.zero;
            }

            // coasting is when we aren't touching accelerate
            // coasting is when we aren't touching accelerate
            if (Mathf.Abs(accelInput) < k_NullInput && GroundPercent > 0.0f)
            {
                newVelocity = Vector3.MoveTowards(newVelocity, new Vector3(0, Rigidbody.velocity.y, 0), Time.fixedDeltaTime * m_FinalStats.CoastingDrag);
            }

            if (GroundPercent > 0.0f && !jump)
            {
                if (turnInput > -0.2 && turnInput < 0.2)
                {
                    newVelocity.x = 0;
                    newVelocity.y = 0;
                    Rigidbody.angularVelocity = Vector3.zero;
                }
            }


            Rigidbody.velocity = newVelocity;


            Rigidbody.velocity += transform.right * ((isInSmash ? 0 : turnInput) * baseStats.Steer * Time.deltaTime) * 2;

            turnInput = 0;

            Physics.gravity = (_gravityUpUnitVector * 9.8f * -1) * baseStats.AddedGravity;
            // Physics.gravity *= 2;




            if (jump)
            {
                if (jumpCount == 0)
                {
                    Rigidbody.AddForce(transform.up * baseStats.JumpHeight);
                    jumpCount++;
                }
            }

            if (FrontLeftWheel.isGrounded || FrontRightWheel.isGrounded || RearLeftWheel.isGrounded || RearRightWheel.isGrounded)
            {
                Rigidbody.angularVelocity = Vector3.zero;
                jumpCount = 0;
            }

            if (switchGrav && jumpCount > 0)
            {
                isInSmash = true;
            }

            if (isInSmash)
            {
                Rigidbody.AddForce((transform.up * 100000) * -1);
            }

            if (m_InAir)
            {
                m_InAir = false;
                Instantiate(JumpVFX, transform.position, Quaternion.identity);
            }

            bool validPosition;
            if (Physics.Raycast(transform.position + (transform.up * 0.1f), -transform.up, out RaycastHit hit, 100.0f, 1 << 9 | 1 << 10 | 1 << 11)) // Layer: ground (9) / Environment(10) / Track (11)
            {
                if (!m_HasCollision)
                {
                    Rigidbody.transform.rotation = Quaternion.Slerp(Rigidbody.transform.rotation, hit.transform.rotation, Time.fixedDeltaTime * rotationToFloorSpeed); // speed of rotation to new plane
                    _gravityUpUnitVector = hit.transform.up;
                }
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > hit.normal.y) ? m_LastCollisionNormal : hit.normal;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime * (GroundPercent > 0.0f ? 10.0f : 1.0f)));    // Blend faster if on ground
            }
            else
            {
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > 0.0f) ? m_LastCollisionNormal : Vector3.up;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime));
            }

            validPosition = GroundPercent > 0.7f && !m_HasCollision && Vector3.Dot(m_VerticalReference, Vector3.up) > 0.9f;

            // Airborne / Half on ground management
            if (GroundPercent < 0.7f)
            {
                Rigidbody.angularVelocity = new Vector3(0.0f, Rigidbody.angularVelocity.y * 0.98f, 0.0f);
                Vector3 finalOrientationDirection = Vector3.ProjectOnPlane(transform.forward, m_VerticalReference);
                finalOrientationDirection.Normalize();
                if (finalOrientationDirection.sqrMagnitude > 0.0f)
                {
                    Rigidbody.MoveRotation(Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(finalOrientationDirection, m_VerticalReference), Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime)));
                }
            }
            else if (validPosition)
            {
                m_LastValidPosition = transform.position;
                m_LastValidRotation.eulerAngles = new Vector3(0.0f, transform.rotation.y, 0.0f);
            }

            ActivateDriftVFX(IsDrifting && GroundPercent > 0.0f);
        }
    }
}
