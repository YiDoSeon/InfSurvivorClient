using System.Collections.Generic;
using System.Linq;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using Shared.Physics.Collider;
using Shared.Utils;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private LocalPlayerController player;

    // TODO: player를 직접 참조하지 않고 이벤트 등록 방식으로 변경
    public void SetPlayer(LocalPlayerController player)
    {
        this.player = player;
    }

    private void CheckMeleeAttack()
    {
        if (player is null)
        {
            return;
        }
        player.MeleeAttackCollider.Position = player.TargetMovePosition.ToCVector2() + player.Dir4.ToCVector2() * 0.8f;
        List<ColliderBase> colliders = Managers.Collision.GetOverlappedColliders(
            player.MeleeAttackCollider,
            targetMask: Shared.Utils.Extensions.CombineMask(Shared.Packet.CollisionLayer.Monster));
        foreach (ColliderBase collider in colliders)
        {
            if (collider.Owner is EnemyController enemy)
            {
                enemy.OnDamaged(player);
                C_MeleeAttack meleeAttackPacket = new C_MeleeAttack();
                Managers.Network.Send(meleeAttackPacket);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (player is null)
        {
            return;
        }
        Gizmos.DrawWireSphere(player.MeleeAttackCollider.Center.ToUnityVector3(), player.MeleeAttackCollider.Radius);
    }
#endif

}
