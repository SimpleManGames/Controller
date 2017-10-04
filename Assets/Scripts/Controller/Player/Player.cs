﻿using UnityEditor.Animations;
using UnityEngine;

[SelectionBase, RequireComponent(typeof(Controller)), RequireComponent(typeof(PlayerInputController))]
public class Player : Agent
{
    [HideInInspector]
    public PlayerInputController input;

    [HideInInspector]
    public Transform cameraRigTransform;
    private CameraController cameraController;

    Controller controller;
    public Animator Animator { get; private set; }
    private GameObject modelObject;

    [Header("Movement Info")]
    [ReadOnly, Tooltip("Describes the direction the player controller is trying to move. " +
        "If you want to manually move this, use the Controller's debug move")]
    public Vector3 moveDirection;
    [SerializeField, ReadOnly]
    private float moveAmount;
    public float MoveAmount { get; set; }
    [ReadOnly]
    public bool canMove;
    [ReadOnly]
    public bool lockOn;
    [ReadOnly]
    public bool rolling;
    public float rollModifier = 1;
    [ReadOnly]
    public Vector2 rollInput;

    public override void Awake()
    {
        base.Awake();
        controller = GetComponent<Controller>();
        Animator = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInputController>();
        state.CurrentState = new PlayerIdleState(state, this, controller);

        modelObject = transform.Find("boxMan").gameObject;
    }

    public override void Start()
    {
        base.Start();
        maxJumpVelocity = Mathf.Abs(Gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * minJumpHeight);

        cameraController = cameraRigTransform.GetComponent<CameraController>();
    }

    public void Update()
    {
        canMove = Animator.GetBool("canMove");
        rolling = Animator.GetBool("rolling");

        HandleMovement();

        if (canMove)
            RotateTransform();

        UpdateAnimationValues();
    }

    private void HandleMovement()
    {
        float m = Mathf.Abs(input.Current.MoveInput.x) + Mathf.Abs(input.Current.MoveInput.z);
        moveAmount = Mathf.Clamp01(m);
        Vector3 moveDirectionNoYChange = Vector3.zero;

        if (canMove)
        {
            moveDirectionNoYChange += moveDirection;
            moveDirectionNoYChange *= moveAmount;
            moveDirectionNoYChange.y = moveDirection.y;
        }

        if (modelObject.transform.localPosition != Vector3.zero)
        {
            moveDirectionNoYChange += modelObject.transform.localPosition;
            modelObject.transform.localPosition = Vector3.zero;
        }

        transform.position += moveDirectionNoYChange * controller.DeltaTime;
    }

    private void RotateTransform()
    {
        Vector3 targetDirection = moveDirection;
        targetDirection.y = 0f;

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion tr = Quaternion.LookRotation(targetDirection);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, controller.DeltaTime * rotateSpeed);
        transform.rotation = targetRotation;
    }

    private void UpdateAnimationValues()
    {
        float v = input.Current.MoveInput.z;
        float h = input.Current.MoveInput.x;

        if (input.Current.RollInput)
        {
            rollInput.y = input.Current.MoveInput.z;
            rollInput.x = input.Current.MoveInput.x;
        }


        if (lockOn)
        {
            if (rolling)
            {
                Animator.SetFloat("vertical", rollInput.y);
                Animator.SetFloat("horizontal", rollInput.x);
                return;
            }

            Animator.SetFloat("vertical", v, 0.2f, controller.DeltaTime);
            Animator.SetFloat("horizontal", h, 0.1f, controller.DeltaTime);
            return;
        }

        if (rolling)
        {
            ChildMotion motion = new ChildMotion();
            
            if (lockOn == false)
            {
                rollInput.y = 1;
                rollInput.x = 0;

                Animator.SetFloat("vertical", rollInput.y);
                Animator.SetFloat("horizontal", rollInput.x);
                return;
            }
            else
            {
                if (Mathf.Abs(rollInput.y) > 0.3f)
                    rollInput.y = 0f;
                if (Mathf.Abs(rollInput.x) > 0.3f)
                    rollInput.x = 0f;

                Animator.SetFloat("vertical", rollInput.y);
                Animator.SetFloat("horizontal", rollInput.x);
                return;
            }
        }

        if (canMove)
        {
            Animator.SetFloat("vertical", moveAmount);
        }
    }

    #region State Management

    public bool MaintainingGround()
    {
        return controller.CurrentGround.IsGrounded(true, 0.5f);
    }

    public bool AcquiringGround()
    {
        return controller.CurrentGround.IsGrounded(false, 0.01f);
    }

    public Vector3 LocalMovement()
    {
        Vector3 lookDirection = cameraRigTransform.forward;
        lookDirection.y = 0f;

        Vector3 right = Vector3.Cross(controller.Up, lookDirection);

        Vector3 local = Vector3.zero;

        if (input.Current.MoveInput.x != 0)
        {
            local += right * input.Current.MoveInput.x;
        }

        if (input.Current.MoveInput.z != 0)
        {
            local += lookDirection * input.Current.MoveInput.z;
        }

        return local.normalized;
    }

    public bool HandleJumpState()
    {
        if (input.Current.JumpInput)
        {
            state.CurrentState = new PlayerJumpState(state, this, controller);
            return true;
        }

        return false;
    }

    public bool HandleFallState()
    {
        if (!MaintainingGround())
        {
            state.CurrentState = new PlayerFallState(state, this, controller);
            return true;
        }

        return false;
    }

    public bool HandleTargetState()
    {
        if (input.Current.TargetInput)
        {
            state.CurrentState = new PlayerLockOnState(state, this, controller, null);
            return true;
        }

        return false;
    }

    public bool HandleMoveState()
    {
        if (input.Current.MoveInput != Vector3.zero)
        {
            if (input.Current.RunInput)
            {
                state.CurrentState = new PlayerRunState(state, this, controller);
                return true;
            }

            state.CurrentState = new PlayerMoveState(state, this, controller);
            return true;
        }

        return false;
    }

    public bool HandleRunState()
    {
        if (input.Current.RunInput)
        {
            state.CurrentState = new PlayerRunState(state, this, controller);
            return true;
        }

        return false;
    }

    public bool HandleIdleState()
    {
        if (input.Current.MoveInput == Vector3.zero)
        {
            state.CurrentState = new PlayerIdleState(state, this, controller);
            return true;
        }
        return false;
    }

    public bool HandleRollState()
    {
        if (input.Current.RollInput)
        {
            state.CurrentState = new PlayerRollState(state, this, controller);
            return true;
        }

        return false;
    }

    #endregion
}