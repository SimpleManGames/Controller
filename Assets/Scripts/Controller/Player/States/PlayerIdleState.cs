﻿using UnityEngine;

public sealed class PlayerIdleState : IState
{
    private Player player;
    private Controller controller;

    public StateMachine State { get; private set; }

    public PlayerIdleState(StateMachine state, Player player, Controller controller)
    {
        State = state;

        this.player = player;
        this.controller = controller;
    }

    public void Start()
    {
        controller.EnableSlopeLimit();
        controller.EnableClamping();

        player.moveDirection.y = 0f;
    }

    public void Update()
    {
        if (player.HandleJumpState())
            return;

        if (player.HandleFallState())
            return;

        if (player.HandleTargetState())
            return;

        if (player.HandleMoveState())
            return;

        if (player.HandleRollState())
            return;

        player.moveDirection = Vector3.MoveTowards(player.moveDirection, Vector3.zero, 25f * controller.DeltaTime);
    }

    public void Exit() { }
}