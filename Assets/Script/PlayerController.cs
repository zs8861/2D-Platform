using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float runSpeed;
    public float jumpSpeed;
    public float doulbJumpSpeed;
    public float climbSpeed;
    public float restoreTime;

    private Rigidbody2D myRigidbody;
    private Animator myAnim;
    private BoxCollider2D myFeet;
    private bool isGround;
    private bool canDoubleJump;
    private bool isOneWayPlatform;

    private bool isLadder;
    private bool isClimbing;

    private bool isJumping;
    private bool isFalling;
    private bool isDoubleJumping;
    private bool isDoubleFalling;

    private float playerGravity;

    private PlayerInputActions controls;
    private Vector2 move;

    void Awake()
    {
        controls = new PlayerInputActions();

        controls.GamePlay.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.GamePlay.Move.canceled += ctx => move = Vector2.zero;
        controls.GamePlay.Jump.started += ctx => Jump();
    }

    void OnEnable()
    {
        controls.GamePlay.Enable();
    }

    void OnDisable()
    {
        controls.GamePlay.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();
        myFeet = GetComponent<BoxCollider2D>();
        playerGravity = myRigidbody.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameController.isGameAlive)
        {
            CheckAirStatus();
            Flip();
            Run();
            //Jump();
            Climb();
            //Attack();
            CheckGrounded();
            CheckLadder();
            SwitchAnimation();
            OneWayPlatformCheck();
        }
    }

    void CheckGrounded()
    {
        isGround = myFeet.IsTouchingLayers(LayerMask.GetMask("Ground")) ||
                   myFeet.IsTouchingLayers(LayerMask.GetMask("MovingPlatform")) ||
                   myFeet.IsTouchingLayers(LayerMask.GetMask("OneWayPlatform"));
        isOneWayPlatform = myFeet.IsTouchingLayers(LayerMask.GetMask("OneWayPlatform"));
    }

    void CheckLadder()
    {
        isLadder = myFeet.IsTouchingLayers(LayerMask.GetMask("Ladder"));
        //Debug.Log("isLadder:" + isLadder);
    }

    void Flip()
    {
        bool plyerHasXAxisSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        if(plyerHasXAxisSpeed)
        {
            if(myRigidbody.velocity.x > 0.1f)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }

            if (myRigidbody.velocity.x < -0.1f)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    void Run()
    {
        //float moveDir = Input.GetAxis("Horizontal");
        ////Debug.Log("moveDir = " + moveDir.ToString());
        //Vector2 playerVel = new Vector2(moveDir * runSpeed, myRigidbody.velocity.y);
        //myRigidbody.velocity = playerVel;
        //bool plyerHasXAxisSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        //myAnim.SetBool("Run", plyerHasXAxisSpeed);

        Vector2 playerVelocity = new Vector2(move.x * runSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;
        bool playerHasXAxisSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        myAnim.SetBool("Run", playerHasXAxisSpeed);
    }

    void Jump()
    {
        //if (Input.GetButtonDown("Jump"))
        {
            if(isGround)
            {
                myAnim.SetBool("Jump", true);
                Vector2 jumpVel = new Vector2(0.0f, jumpSpeed);
                myRigidbody.velocity = Vector2.up * jumpVel;
                canDoubleJump = true;
            }
            else
            {
                if(canDoubleJump)
                {
                    myAnim.SetBool("DoubleJump", true);
                    Vector2 doubleJumpVel = new Vector2(0.0f, doulbJumpSpeed);
                    myRigidbody.velocity = Vector2.up * doubleJumpVel;
                    canDoubleJump = false;
                }
            }
        }
    }

    void Climb()
    {
        float moveY = Input.GetAxis("Vertical");

        if(isClimbing)
        {
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, moveY * climbSpeed);
            canDoubleJump = false;
        }

        if (isLadder)        
        {            
            if (moveY > 0.5f || moveY < -0.5f)
            {
                myAnim.SetBool("Jump", false);
                myAnim.SetBool("DoubleJump", false);
                myAnim.SetBool("Climbing", true);
                myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, moveY * climbSpeed);
                myRigidbody.gravityScale = 0.0f;
            }
            else
            {
                if (isJumping || isFalling || isDoubleJumping || isDoubleFalling)
                {
                    myAnim.SetBool("Climbing", false);
                }
                else
                {
                    myAnim.SetBool("Climbing", false);
                    myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, 0.0f);
                    
                }
            }
        }
        else
        {
            myAnim.SetBool("Climbing", false);
            myRigidbody.gravityScale = playerGravity;
        }

        if (isLadder && isGround)
        {
            myRigidbody.gravityScale = playerGravity;
        }

        //Debug.Log("myRigidbody.gravityScale:"+ myRigidbody.gravityScale);
    }

    //void Attack()
    //{
    //    if(Input.GetButtonDown("Attack"))
    //    {
    //        myAnim.SetTrigger("Attack");
    //    }
    //}

    void SwitchAnimation()
    {
        myAnim.SetBool("Idle", false);
        if (myAnim.GetBool("Jump"))
        {
            if(myRigidbody.velocity.y < 0.0f)
            {
                myAnim.SetBool("Jump", false);
                myAnim.SetBool("Fall", true);
            }
        }
        else if(isGround)
        {
            myAnim.SetBool("Fall", false);
            myAnim.SetBool("Idle", true);
        }

        if (myAnim.GetBool("DoubleJump"))
        {
            if (myRigidbody.velocity.y < 0.0f)
            {
                myAnim.SetBool("DoubleJump", false);
                myAnim.SetBool("DoubleFall", true);
            }
        }
        else if (isGround)
        {
            myAnim.SetBool("DoubleFall", false);
            myAnim.SetBool("Idle", true);
        }
    }

    void OneWayPlatformCheck()
    {
        if(isGround && gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }        

        float moveY = Input.GetAxis("Vertical");
        if (isOneWayPlatform && moveY < -0.1f)
        {
            gameObject.layer = LayerMask.NameToLayer("OneWayPlatform");
            Invoke("RestorePlayerLayer", restoreTime);
        }
    }

    void RestorePlayerLayer()
    {
        if(!isGround && gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }

    void CheckAirStatus()
    {
        isJumping = myAnim.GetBool("Jump");
        isFalling = myAnim.GetBool("Fall");
        isDoubleJumping = myAnim.GetBool("DoubleJump");
        isDoubleFalling = myAnim.GetBool("DoubleFall");
        isClimbing = myAnim.GetBool("Climbing");
        //Debug.Log("isJumping:" + isJumping);
        //Debug.Log("isFalling:" + isFalling);
        //Debug.Log("isDoubleJumping:" + isDoubleJumping);
        //Debug.Log("isDoubleFalling:" + isDoubleFalling);
        //Debug.Log("isClimbing:" + isClimbing);
    }
}
