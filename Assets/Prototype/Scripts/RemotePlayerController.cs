using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using UnityEngine;

public class RemotePlayerController : PlayerController
{
    private Vector2 lastPosition;
    private Vector2 lastVelocity;
    private Vector2 lastMoveDir;
    private float lastPacketTime;

    public override void InitPos(PositionInfo posInfo)
    {
        transform.position = lastPosition = posInfo.Pos.ToUnityVector3();
        lastMoveDir = lastVelocity = posInfo.Velocity.ToUnityVector3();
        ApplyFacingDirection(lastMoveDir);
        AnimationSetFloat(ANIM_FLOAT_SPEED, lastVelocity.sqrMagnitude);
    }
    protected override void OnUpdate()
    {
        float renderTime = Time.time - Managers.Network.GetDisplayPing();
        float elapsedTime = renderTime - lastPacketTime;
        Vector2 predictedPos = lastPosition + lastVelocity * elapsedTime;
        transform.position = Vector3.Lerp(transform.position, predictedPos, 15f * Time.deltaTime);
    }
    
    public override void OnUpdateMoveState(S_Move move)
    {
        lastPosition = move.PosInfo.Pos.ToUnityVector3();
        lastVelocity = move.PosInfo.Velocity.ToUnityVector3();
        lastPacketTime = Time.time;

        if (lastVelocity.sqrMagnitude > 0.01f)
        {
            lastMoveDir = lastVelocity;
        }

        ApplyFacingDirection(lastMoveDir);
        AnimationSetFloat(ANIM_FLOAT_SPEED, lastVelocity.sqrMagnitude);

        if ((lastPosition - (Vector2)transform.position).sqrMagnitude > 2f * 2f)
        {
            transform.position = lastPosition;
        }
    }
}
