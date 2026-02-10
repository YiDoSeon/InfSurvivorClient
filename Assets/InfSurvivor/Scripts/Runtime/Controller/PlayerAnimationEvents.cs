using System.Collections.Generic;
using System.Linq;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using Shared.Packet;
using Shared.Physics.Collider;
using Shared.Utils;
using UnityEngine;

namespace InfSurvivor.Runtime.Controller
{
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
            player.CheckMeleeAttack();
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

}