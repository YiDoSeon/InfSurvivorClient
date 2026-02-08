using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using UnityEngine;

public class RemotePlayerController : PlayerController
{
    private float lastPacketTime;
    private bool lastFirePressed;

    public override void InitPos(PositionInfo posInfo)
    {
        transform.position = TargetPosition = posInfo.Pos.ToUnityVector3();
        LastVelocity = posInfo.Velocity.ToUnityVector3();
        LastFacingDir = posInfo.FacingDir.ToUnityVector2();
        lastFirePressed = posInfo.FirePressed;
        ApplyFacingDirection(LastFacingDir);
        AnimationSetFloat(ANIM_FLOAT_SPEED, LastVelocity.sqrMagnitude);
    }
    
    protected override void Update()
    {
        base.Update();
        float renderTime = Time.time - Managers.Network.GetDisplayPing();
        float elapsedTime = renderTime - lastPacketTime;
        Vector2 predictedPos = TargetPosition + LastVelocity * elapsedTime;
        transform.position = Vector3.Lerp(transform.position, predictedPos, 15f * Time.deltaTime);
    }
    
    public override void OnUpdateMoveState(S_Move move)
    {
        TargetPosition = move.PosInfo.Pos.ToUnityVector2();
        LastVelocity = move.PosInfo.Velocity.ToUnityVector2();
        LastFacingDir = move.PosInfo.FacingDir.ToUnityVector2();
        lastPacketTime = Time.time;

        ApplyFacingDirection(LastFacingDir);

        if (lastFirePressed != move.PosInfo.FirePressed)
        {
            lastFirePressed = move.PosInfo.FirePressed;
            if (lastFirePressed)
            {
                AnimationSetTrigger(ANIM_TRIGGER_SWORD_ATTACK);
            }
            AnimationSetBool(ANIM_BOOL_SWORD_ATTACKING, lastFirePressed);
        }
        
        AnimationSetFloat(ANIM_FLOAT_SPEED, LastVelocity.sqrMagnitude);

        if ((TargetPosition - (Vector2)transform.position).sqrMagnitude > 2f * 2f)
        {
            transform.position = TargetPosition;
        }
    }
}
