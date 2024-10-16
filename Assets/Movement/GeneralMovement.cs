using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public abstract class GeneralMovement : MonoBehaviour
{
    [SerializeField]
    public float movementSpeed = 2f;

    [SerializeField]
    public float gravityScale = 5f;

    [SerializeField]
    public float jumpPower = 10f;

    [SerializeField]
    public float otherJumpPower = 5f;

    [SerializeField]
    public int maxJumps = 2;

    [SerializeField]
    public float maxHealth = 100f;

    [SerializeField]
    public float damage = 10f;

    [SerializeField]
    public int maxLives = 5;

    public float maxFallingSpeed = -20f;

    public float maxFallingSpeedReset = -19.8f;


    public FollowLineDrawer followLineDrawer;

    public GameObject jumpDust;
    public GameObject otherJumpDust;


    public float groundCheckRadius = 0.2f;
    public float wallCheckRadius = 0.7f;

    public bool canInteract = true;
    public Transform groundCheckCollider;
    public Transform overheadCheckCollider;
    public LayerMask groundLayer;
    public Transform wallCheckCollider;
    public LayerMask wallLayer;
    public float health;

    private int jumpsLeft;
    private bool facingRight = true;
    private float horizontalMovement;
    private bool wantJump = false;
    private bool wantAttack = false;
    private bool isGrounded;
    private bool isWalled;
    private int lives;

    private float lastJumpTime = 0f; // New variable for tracking last jump time
    private float jumpCooldown = 0.2f; // Cooldown time (1 second)

    private float lastDamagedTime = 0f;
    private float damagedCooldown = 1f;



    Rigidbody2D rb;
    Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        lives = maxLives;
        health = maxHealth;
        jumpsLeft = maxJumps;
        rb.gravityScale = gravityScale;
    }
    void Start()
    {
        Application.targetFrameRate = 30;
        if (followLineDrawer != null)
        {
            transform.position = followLineDrawer.GetMiddlePoint().Value;
        }
    }

    private void Update()
    {
        if (!canInteract) { return; }

        horizontalMovement = getHorizontalMovement() * movementSpeed;

        wantJump = getJump();
        jumpIfWantedAndPossible();
        wantAttack = getAttack();
        attack();

        // Animation
        animator.SetFloat("xVelocity", Mathf.Abs(horizontalMovement));
    }

    void FixedUpdate()
    {
        GroundCheck();
        WallCheck();
        Move();

        if (rb.velocity.y < maxFallingSpeed) { rb.velocity = new Vector2(rb.velocity.x, maxFallingSpeedReset); }

        // Use a jump when not in gound
        if (!isGrounded && !isWalled && jumpsLeft == maxJumps) { jumpsLeft--; }
    }

    private void Move()
    {
        if (rb == null) { Debug.LogError("No rigidbody to move!"); }
        else
        {

            // Dont run into a wall!
            if (isWalled) {
                try
                {
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(wallCheckCollider.position, wallCheckRadius - 0.1f, groundLayer);

                    if (colliders.Length > 0)
                    {
                        bool wallIsLeft = colliders[0].ClosestPoint(wallCheckCollider.position).x < wallCheckCollider.position.x;

                        if ((wallIsLeft && horizontalMovement < 0) || (!wallIsLeft && horizontalMovement > 0))
                        {
                            // Stick to wall if falling
                            if (rb.velocity.y < 0.1) {
                                rb.velocity = new Vector2(0f, 0f);
                                rb.gravityScale = 0f;
                            }
                            else { rb.gravityScale = gravityScale; }

                            // Avoid weird animation
                            if (facingRight && horizontalMovement < 0)
                            {
                                facingRight = false;
                            }
                            if (!facingRight && horizontalMovement > 0)
                            {
                                facingRight = true;
                            }

                            return;
                        }
                    }

                }
                catch { }
            }
            
            rb.gravityScale = gravityScale;

            
            

            rb.velocity = new Vector2(horizontalMovement, rb.velocity.y);

            if (facingRight && horizontalMovement < 0)
            {
                transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
                facingRight = false;
            }
            if (!facingRight && horizontalMovement > 0)
            {
                transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                facingRight = true;
            }
        }
    }

    private void jumpIfWantedAndPossible()
    {
        // Check if the player wants to jump, has jumps left, and if the cooldown has elapsed
        if (jumpsLeft > 0 && wantJump && Time.time >= lastJumpTime + jumpCooldown)
        {

            rb.velocity = new Vector2(horizontalMovement, 0f);

            float thisJumpPower = jumpsLeft == maxJumps ? jumpPower : otherJumpPower;

            // TODO Jump away from wall
            /*
            if (isWalled)
            {
                try {
                    Collider2D collider = Physics2D.OverlapCircleAll(wallCheckCollider.position, wallCheckRadius, wallLayer)[0];

                    float sideJumpPower = collider.ClosestPoint(wallCheckCollider.position).x < wallCheckCollider.position.x ? -jumpPower / 2 : jumpPower / 2;

                    rb.AddForce(new Vector2(sideJumpPower, 0f));
                }
                catch { }
                
            }
            */



            rb.AddForce(new Vector2(0f, thisJumpPower));

            if(jumpsLeft == maxJumps)
            {
                GameObject thisJumpDust = Instantiate(jumpDust);
                thisJumpDust.transform.position = groundCheckCollider.transform.position;
            }
            else
            {
                GameObject thisOtherJumpDust = Instantiate(otherJumpDust);
                thisOtherJumpDust.transform.position = groundCheckCollider.transform.position;
            }

            jumpsLeft -= 1;
            lastJumpTime = Time.time; // Update the time of the last jump

            // Animation
            animator.SetTrigger("jump");
        }
    }

    private void attack() { }

    void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        // Check if the GroundCheckObject is colliding with other 2D Colliders in the "Ground" Layer
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckCollider.position, groundCheckRadius, groundLayer);
        if (colliders.Length > 0 && Time.time >= lastJumpTime + jumpCooldown)
        {
            isGrounded = true;
            if (!wasGrounded)
            {
                // AudioManager.instance.PlaySFX("landing");
            }
            jumpsLeft = maxJumps;

            // Check if any of the colliders are moving platforms and parent it to this transform
            foreach (var c in colliders)
            {
                if (c.tag == "MovingPlatform")
                    transform.parent = c.transform;
            }

            if (!wasGrounded) { animator.SetTrigger("groundHit"); }
            
        }
        else
        {
            // Un-parent the transform
            transform.parent = null;
        }


        // Update animator (if any) about the jumping state
        // animator.SetBool("Jump", !isGrounded);
    }

    void WallCheck()
    {
        bool wasWalled = isWalled;
        isWalled = false;

        // Check if the GroundCheckObject is colliding with other 2D Colliders in the "Ground" Layer
        Collider2D[] colliders = Physics2D.OverlapCircleAll(wallCheckCollider.position, wallCheckRadius, groundLayer);
        if (colliders.Length > 0 && !isGrounded)
        {

            isWalled = true;
            if (!wasWalled)
            {
                // AudioManager.instance.PlaySFX("landing");
            }
            jumpsLeft = maxJumps;
            lastJumpTime -= jumpCooldown;

            // Check if any of the colliders are moving platforms and parent it to this transform
            foreach (var c in colliders)
            {
                if (c.tag == "MovingPlatform")
                    transform.parent = c.transform;
            }
        }
        else
        {
            // Un-parent the transform
            transform.parent = null;
        }

        animator.SetBool("isWalled", isWalled && horizontalMovement != 0);
        animator.SetBool("isGrounded", isGrounded);


        // Update animator (if any) about the jumping state
        // animator.SetBool("Jump", !isGrounded);
    }

    public void getDamaged(float damage)
    {
        if (Time.time > lastDamagedTime + damagedCooldown) {
            health -= damage;
            lastDamagedTime = Time.time;
            if (health <= 0)
            {
                die();
            }
        }
    }

    public void getHealed(float curation)
    {
        health = health + curation > maxHealth ? maxHealth : health + curation;
    }

    public void die()
    {
        Debug.Log("Player died");
        lives -= 1;
        if(lives < 0)
        {
            loose();
        }
        else{ resetLive(); }
    }
    public void resetLive()
    {
        
    }
    public void loose() { Debug.Log("Player lost!!"); }

    public abstract float getHorizontalMovement();
    public abstract bool getJump();
    public abstract bool getAttack();
}
