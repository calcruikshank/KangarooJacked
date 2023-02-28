using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Stack inputStack = new Stack();

    Vector2 rightStickValue = new Vector2();
    Vector2 leftStickValue = new Vector2();

    Vector2 movement;
    Vector2 lastMoveDir;
    float maxSpeed = 15;

    float dashSpeed = 50;
    float currentDashSpeed;
    Vector2 dashDir = new Vector2();


    [SerializeField] LayerMask groundLayer;
    Collider2D col;
    Rigidbody2D rb;

    float jumpHeight = 20f;

    int numOfExtraJumps = 1;
    int currentNumOfExtraJumps = 0;

    bool groundHasNotBeenLeftAfterJumping = false;
    bool grounded;
    bool colliderIsTouchingGround;

    public enum State
    {
        Normal,
        Knockback,
        Dashing
    }

    public State state;
    float bufferTimerThreshold = .2f;


    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        col = this.GetComponent<Collider2D>();
        state = State.Normal;
    }

    // Update is called once per frame
    void Update()
    {
        HandleBufferInput();
        CheckForGround();
        GetLastMoveDirection();

        switch (state)
        {
            case State.Normal:
                HandleMovement();
                break;
            case State.Dashing:
                HandleDash();
                break;
        }
    }

    private void GetLastMoveDirection()
    {
        if (leftStickValue.magnitude != 0)
        {
            lastMoveDir = leftStickValue;
        }
    }

    private void CheckForGround()
    {
        var groundedRay = Physics2D.Raycast(transform.position, Vector2.down, 0.1f + col.bounds.size.y / 2, groundLayer);

        if (groundedRay.collider != null)
        {
            colliderIsTouchingGround = true;
        }
        if (groundedRay.collider == null)
        {
            colliderIsTouchingGround = false;
            groundHasNotBeenLeftAfterJumping = false;
            grounded = false;
        }

        if (colliderIsTouchingGround && !groundHasNotBeenLeftAfterJumping)
        {
            grounded = true;
            currentNumOfExtraJumps = 0;
        }

        if (groundHasNotBeenLeftAfterJumping)
        {
            grounded = false;
        }
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Normal:
                FixedHandleMovement();
                break;
            case State.Dashing:
                FixedHandleDash();
                break;
        }
    }


    void HandleBufferInput()
    {
        if (inputStack.Count > 0)
        {
            BufferInput currentBufferedInput = (BufferInput)inputStack.Peek();

            if (Time.time - currentBufferedInput.timeOfInput < bufferTimerThreshold)
            {
                if (currentBufferedInput.actionType == KangarooJackedData.InputActionType.JUMP)
                {
                    if (grounded)
                    {
                        Jump();
                        inputStack.Pop();
                    }
                    if (!grounded && !groundHasNotBeenLeftAfterJumping && currentNumOfExtraJumps < numOfExtraJumps)
                    {
                        currentNumOfExtraJumps++;
                        rb.velocity = new Vector2(movement.x * maxSpeed, 0);
                        Jump();
                        inputStack.Pop();
                    }
                }
                if (currentBufferedInput.actionType == KangarooJackedData.InputActionType.DASH)
                {
                    if (state == State.Normal)
                    {
                        if (grounded)
                        {
                            Dash(new Vector2(currentBufferedInput.directionOfAction.x, 0));
                        }
                        else
                        {
                            Dash(currentBufferedInput.directionOfAction);
                        }
                        inputStack.Pop();
                    }
                }
            }
            if (Time.time - currentBufferedInput.timeOfInput >= bufferTimerThreshold)
            {
                inputStack.Pop();
            }
        }
    }

    private void Dash(Vector2 directionOfDash)
    {
        dashDir = directionOfDash.normalized;
        currentDashSpeed = dashSpeed;
        SetStateToDashing();
    }
    private void HandleDash()
    {
        float powerDashSpeedMulti = 6f;
        currentDashSpeed -= currentDashSpeed * powerDashSpeedMulti * Time.deltaTime;

        if (currentDashSpeed < 5f)
        {
            SetStateToNormal();
        }
    }
    private void FixedHandleDash()
    {
        rb.velocity = new Vector3(dashDir.x * currentDashSpeed, dashDir.y * currentDashSpeed);
    }


    private void Jump()
    {
        groundHasNotBeenLeftAfterJumping = true;
        grounded = false;
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
        SetStateToNormal();
    }


    private void HandleMovement()
    {
        movement = leftStickValue; 
    }
    private void FixedHandleMovement()
    {
        //rb.AddForce(movement.normalized * moveSpeed);


        //instead of using move towards i want to keep the momentum of the rb if the current velocity is higher than the input velocity this way it will only ever add force in the direction of input
        if (grounded)
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, new Vector2(movement.x * maxSpeed, rb.velocity.y), 20);
        }

        if (!grounded)
        {
            if (rb.velocity.x < leftStickValue.x * maxSpeed && leftStickValue.x > 0)
            {
                rb.velocity = Vector2.MoveTowards(rb.velocity, new Vector2(movement.x * maxSpeed, rb.velocity.y), 2);
            }
            if (rb.velocity.x > leftStickValue.x * maxSpeed && leftStickValue.x < 0)
            {
                rb.velocity = Vector2.MoveTowards(rb.velocity, new Vector2(movement.x * maxSpeed, rb.velocity.y), 2);
            }
        }
    }

    void OnLook(InputValue inputValue)
    {
        rightStickValue = inputValue.Get<Vector2>();
    }

    void OnMove(InputValue inputValue)
    {
        leftStickValue = inputValue.Get<Vector2>();
    }

    void OnJump()
    {
        BufferInput jumpBuffer = new BufferInput(KangarooJackedData.InputActionType.JUMP, leftStickValue, Time.time);
        inputStack.Push(jumpBuffer);
    }

    void OnDash()
    {
        BufferInput dashBuffer = new BufferInput(KangarooJackedData.InputActionType.DASH, lastMoveDir.normalized, Time.time);
        inputStack.Push(dashBuffer);
    }

    private void SetStateToDashing()
    {
        state = State.Dashing;
    }
    private void SetStateToNormal()
    {
        state = State.Normal;
    }
}
