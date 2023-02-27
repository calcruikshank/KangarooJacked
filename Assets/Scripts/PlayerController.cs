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

    float currentSpeed = 10f;


    [SerializeField] LayerMask groundLayer;
    Collider2D col;
    Rigidbody2D rb;

    [SerializeField] float jumpHeight = 10f;


    public enum State
    {
        Normal,
        Knockback
    }

    public State state;
    float bufferTimerThreshold = 1f;


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

        switch (state)
        {
            case State.Normal:
                HandleMovement();
                break;
        }
    }
    void FixedUpdate()
    {
        switch (state)
        {
            case State.Normal:
                FixedHandleMovement();
                break;
        }
    }
    void HandleBufferInput()
    {
        if (inputStack.Count > 0)
        {
            BufferInput currentBufferedInput = (BufferInput)inputStack.Peek();

            Debug.Log(Time.time - currentBufferedInput.timeOfInput);
            if (Time.time - currentBufferedInput.timeOfInput < bufferTimerThreshold)
            {
                if (currentBufferedInput.actionType == KangarooJackedData.InputActionType.JUMP)
                {
                    if (IsGrounded())
                    {
                        Jump();
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

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
    }

    private void HandleMovement()
    {
        movement = Vector3.MoveTowards(movement, leftStickValue, 20 * Time.deltaTime);
    }

    public bool IsGrounded()
    {
        var groundedRay = Physics2D.Raycast(transform.position, Vector2.down, 0.1f + col.bounds.size.y / 2, groundLayer);

        if (groundedRay.collider != null)
        {
            return true;
        }

        return false;

    }
    private void FixedHandleMovement()
    {
        //rb.AddForce(movement.normalized * moveSpeed);
        rb.velocity = new Vector3(movement.x * currentSpeed, rb.velocity.y);
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

}
