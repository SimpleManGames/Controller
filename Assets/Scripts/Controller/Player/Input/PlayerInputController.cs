﻿using System;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField, ReadOnly]
    public PlayerInput Current;
    [HideInInspector]
    public PlayerInput previous;
    public Vector2 RightStickMultiplier = new Vector2(3, -1.5f);

    // Use this for initialization
    void Start()
    {
        Current = new PlayerInput();
    }

    // Update is called once per frame
    void Update()
    {
        previous = Current;

        // Retrieve our current WASD or Arrow Key input
        // Using GetAxisRaw removes any kind of gravity or filtering being applied to the input
        // Ensuring that we are getting either -1, 0 or 1
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        Vector2 rightStickInput = new Vector2(Input.GetAxisRaw("RightStick Horz"), Input.GetAxisRaw("RightStick Vert"));

        // pass rightStick values in place of mouse when non-zero
        mouseInput.x = rightStickInput.x != 0 ? rightStickInput.x * RightStickMultiplier.x : mouseInput.x;
        mouseInput.y = rightStickInput.y != 0 ? rightStickInput.y * RightStickMultiplier.y : mouseInput.y;

        bool neutral = rightStickInput == Vector2.zero;
        bool jumpInput = Input.GetButtonDown("Jump");
        bool runInput = Input.GetButton("Run");
        bool targetInput = Input.GetButtonDown("Target") ? !Current.TargetInput : Current.TargetInput;
        bool rollInput = Input.GetButtonDown("Roll");

        Current = new PlayerInput()
        {
            MoveInput = moveInput,
            MouseInput = mouseInput,
            RightStickInput = rightStickInput,
            RightStickNeutral = neutral,
            JumpInput = jumpInput,
            RunInput = runInput,
            TargetInput = targetInput,
            RollInput = rollInput
        };
    }
}

[Serializable]
public struct PlayerInput
{
    [Header("Input")]
    public Vector3 MoveInput;
    public Vector2 MouseInput;
    public Vector2 RightStickInput;
    public bool RightStickNeutral;
    [HideInInspector]
    public bool JumpInput;
    public bool RunInput;
    public bool TargetInput;
    [HideInInspector]
    public bool RollInput;
}