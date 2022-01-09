using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteShadersURP
{
    /// <summary>
    /// Handles Velocity, Ground Detection and more 2D Character Movement related things.
    /// </summary>
    public class DemoPlayer : MonoBehaviour
    {
        public static DemoPlayer c;

        //Threading Multiple Characters:
        public static float lastTimeStarted;

        [Header("Gravity:")]
        public float gravityFactor = 1f;

        [Header("Ground Detection:")]
        [Tooltip("Toggle this off for flying enemies.")] public bool checkIfGrounded = true;
        [Tooltip("A smaller value is more precise but consumes more performance.")] [Range(0.0033f, 0.1f)] public float groundCheckDelay = 0.02f;
        [Tooltip("A smaller value is more precise but consumes more performance.")] [Range(0.01f, 0.2f)] public float raycastDelay = 0.1f;
        [Tooltip("Higher Numbers make the character take longer to realize he is off ground.")] [Range(0, 10)] public int groundedTolerance = 5;

        [Header("Ceiling Detection:")]
        [Tooltip("Cancels vertical upward velocity when it hits a ceiling.")] public bool checkCeiling = true;

        [HideInInspector] public Vector2 velocity;
        [HideInInspector] public Vector2 movementPercentage;
        bool isGrounded;

        //Internal:
        [HideInInspector] public Rigidbody2D rig;
        [HideInInspector] public CapsuleCollider2D cc;
        [HideInInspector] public RaycastHit2D rayHit;

        Vector2 lastPosition;
        Vector2 lastMovement;

        //Ground-Calculation
        float lastInstantGroundCalculation;
        float lastGroundedTime;
        float lastRaycast;
        int offGroundCount;

        //Slope Related:
        int framesCountedSlopingUp;

        Transform canvas;

        void Awake()
        {
            c = this;

            rig = GetComponent<Rigidbody2D>();
            cc = GetComponent<CapsuleCollider2D>();
            canvas = GameObject.Find("Canvas").transform;
        }

        void OnEnable()
        {
            StopCoroutine(startUpdates());
            StartCoroutine(startUpdates());
        }

        public IEnumerator startUpdates()
        {
            while (Time.time - lastTimeStarted < 0.02345f)
            {
                yield return new WaitForFixedUpdate();
            }

            lastTimeStarted = Time.time;
            lastRaycast = Time.time + 0.123f;
            StartCoroutine(updateIsGrounded());
            StartCoroutine(zeroVelocity());
        }

        IEnumerator zeroVelocity() //Checks if we are on the ground or not.
        {
            while (true)
            {
                if (Mathf.Abs(velocity.x) < 0.005f)
                {
                    velocity.x = 0f;
                }

                yield return new WaitForSeconds(0.3213f);

                if (Mathf.Abs(velocity.y) < 0.005f)
                {
                    velocity.y = 0f;
                }

                yield return new WaitForSeconds(0.3213f);
            }
        }

        IEnumerator updateIsGrounded() //Checks if we are on the ground or not.
        {
            while (checkIfGrounded)
            {
                Vector2 position = transform.position;
                position.y -= cc.size.y * 0.5f - cc.size.x * 0.5f + 0.005f;

                yield return new WaitForSeconds(groundCheckDelay);

                RaycastHit2D hit = Physics2D.CircleCast(position, cc.size.x * 0.5f - 0.0001f, Vector2.down, 0.015f);

                yield return new WaitForSeconds(groundCheckDelay);

                if (hit.collider != null && velocity.y < 0.1f)
                {
                    offGroundCount = 0;
                    isGrounded = true;
                    lastGroundedTime = Time.time;
                }
                else
                {
                    isGrounded = false;
                    offGroundCount++;
                }

                yield return new WaitForSeconds(groundCheckDelay);
            }
        }
        public void checkIsGrounded()
        {
            lastInstantGroundCalculation = Time.time;

            if (velocity.y > 0)
            {
                isGrounded = false;
                return;
            }

            Vector2 position = transform.position;
            position.y -= cc.size.y * 0.5f - cc.size.x * 0.5f + 0.005f;

            RaycastHit2D hit = Physics2D.CircleCast(position, cc.size.x * 0.5f - 0.0001f, Vector2.down, 0.015f);

            if (hit.collider != null)
            {
                offGroundCount = 0;
                isGrounded = true;
                lastGroundedTime = Time.time;
            }
            else
            {
                isGrounded = false;
                offGroundCount++;
            }
        }

        private void Update()
        {
            float velocityTarget = 0;
            if (Input.GetKey(KeyCode.D))
            {
                velocityTarget += 5;
            }
            if (Input.GetKey(KeyCode.A))
            {
                velocityTarget += -5;
            }
            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
            {
                velocity.y = 13;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                velocityTarget *= 2f;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                for(int n = 1; n < canvas.childCount; n++)
                {
                    if(canvas.GetChild(n).position.x > transform.position.x + 1f)
                    {
                        transform.position = canvas.GetChild(n).position + Vector3.down * 2f;
                        velocity.y = -1;
                        break;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                for (int n = canvas.childCount - 1; n > 0; n--)
                {
                    if (canvas.GetChild(n).position.x < transform.position.x - 1f)
                    {
                        transform.position = canvas.GetChild(n).position + Vector3.down * 2f;
                        velocity.y = -1;
                        break;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                if (Time.timeScale > 0.5f)
                {
                    Time.timeScale = 0.2f;
                }
                else
                {
                    Time.timeScale = 1f;
                }
            }

            velocity.x += (velocityTarget - velocity.x) * Time.deltaTime * 7f;
        }

        void FixedUpdate()
        {
            handleGravity(Time.fixedDeltaTime);

            handleRaycast();

            Vector2 movement = velocity * Time.fixedDeltaTime;  //The distance moved this fixed update.

            //Calculating Percentage Moved Vertically (Ceiling Detection)
            if (lastMovement.y != 0)
            {
                movementPercentage.y = (rig.position.y - lastPosition.y) / lastMovement.y;

                //Checking if grounded instantly
                if (movementPercentage.y < 0.9f && Time.time > lastInstantGroundCalculation + 0.2f)
                {
                    checkIsGrounded();
                }

                //Checking if hitting a ceiling
                if (checkCeiling && velocity.y > 0f && lastMovement.y > 0 && movementPercentage.y < 0.01f)
                {
                    velocity.y = 0f;
                }
            }
            else
            {
                movementPercentage.y = 1f;
            }

            //Calculating Percentage Moved Horizontally (Wall Detection & Slope Speed)
            if (lastMovement.x != 0)
            {
                movementPercentage.x = (rig.position.x - lastPosition.x) / lastMovement.x;

                if (isGrounded && movementPercentage.x < 0.95f && movementPercentage.x > 0.3f && velocity.y < 0.1f) //Correcting Slope Speed
                {
                    movement.x /= movementPercentage.x;
                }

                if (movementPercentage.x < 0.01f && velocity.y > 0) //Set Velocity.X=0 when jumping against a wall.
                {
                    velocity.x = 0f;
                }
            }
            else
            {
                movementPercentage.x = 1f;
            }

            //Preventing Slope Sliding
            if (checkIfGrounded)
            {
                if (isGrounded && movement.y < 0)
                {
                    if (movementPercentage.x < 0.5f || rig.position.y - lastPosition.y > 0)
                    {
                        movement.y = 0f;
                    }
                    else
                    {
                        movement.y = Mathf.Max(movement.y, -Mathf.Abs(lastMovement.x));
                    }
                }
            }

            lastPosition = rig.position;
            lastMovement = movement;

            rig.MovePosition(rig.position + movement);
        }

        public void handleRaycast()
        {
            if (lastMovement.x != 0 && Time.time > raycastDelay + lastRaycast)
            {
                lastRaycast = Time.time;
                Vector2 rayOrigin = rig.position;

                if (lastMovement.x > 0)
                {
                    rayOrigin.x += cc.size.x + 0.4f;
                }
                else
                {
                    rayOrigin.x -= cc.size.x + 0.4f;
                }

                rayHit = Physics2D.Raycast(rayOrigin, Vector2.down, cc.size.y * 0.5f + 2f);
            }
        }

        public void handleGravity(float deltaTime)
        {
            if (gravityFactor != 0)
            {
                //Falling
                velocity.y -= 25f * deltaTime * gravityFactor;

                //Reducing Gravity while grounded
                float factor = 1f; if (isGrounded) factor = 0.5f;

                if (isGrounded)
                {
                    //0 Gravity when approaching Slope.
                    if (rig.position.y > lastPosition.y)
                    {
                        framesCountedSlopingUp++;
                    }
                    else
                    {
                        framesCountedSlopingUp = 0;
                    }

                    if (rayHit.collider && ((rayHit.normal.y < 0.95f && rayHit.point.y > rig.position.y - cc.size.y * 0.5f) || framesCountedSlopingUp > 40))
                    {
                        factor = 0f;
                    }

                    //0 Gravity when approaching Edge.
                    if (rayHit.collider == null)
                    {
                        factor = 0f;
                    }
                }
                else
                {   
                    framesCountedSlopingUp = 0;
                }

                //Limiting Fallspeed
                if (velocity.y < -50 * factor)
                {
                    velocity.y = -50 * factor * gravityFactor;
                }
            }
        }

        public bool isOnGround()
        {
            return offGroundCount < groundedTolerance;
        }
    }
}