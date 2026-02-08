using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using UnityEngine;

public class RemotePlayerController : PlayerController
{
    private Vector2 targetPosition;
    private Vector2 lastVelocity;
    private Vector2 lastFacingDir;
    private float lastPacketTime;
    private bool lastFirePressed;

    public override void InitPos(PositionInfo posInfo)
    {
        transform.position = targetPosition = posInfo.Pos.ToUnityVector3();
        lastVelocity = posInfo.Velocity.ToUnityVector3();
        lastFacingDir = posInfo.FacingDir.ToUnityVector2();
        lastFirePressed = posInfo.FirePressed;
        ApplyFacingDirection(lastFacingDir);
        AnimationSetFloat(ANIM_FLOAT_SPEED, lastVelocity.sqrMagnitude);
    }
    protected override void OnUpdate()
    {
        float renderTime = Time.time - Managers.Network.GetDisplayPing();
        float elapsedTime = renderTime - lastPacketTime;
        Vector2 predictedPos = targetPosition + lastVelocity * elapsedTime;
        transform.position = Vector3.Lerp(transform.position, predictedPos, 15f * Time.deltaTime);
    }
    
    public override void OnUpdateMoveState(S_Move move)
    {
        targetPosition = move.PosInfo.Pos.ToUnityVector2();
        lastVelocity = move.PosInfo.Velocity.ToUnityVector2();
        lastFacingDir = move.PosInfo.FacingDir.ToUnityVector2();
        lastPacketTime = Time.time;

        ApplyFacingDirection(lastFacingDir);

        if (lastFirePressed != move.PosInfo.FirePressed)
        {
            lastFirePressed = move.PosInfo.FirePressed;
            if (lastFirePressed)
            {
                AnimationSetTrigger(ANIM_TRIGGER_SWORD_ATTACK);
            }
            AnimationSetBool(ANIM_BOOL_SWORD_ATTACKING, lastFirePressed);
        }
        
        AnimationSetFloat(ANIM_FLOAT_SPEED, lastVelocity.sqrMagnitude);

        if ((targetPosition - (Vector2)transform.position).sqrMagnitude > 2f * 2f)
        {
            Debug.Log("???");
            transform.position = targetPosition;
        }
    }
}
