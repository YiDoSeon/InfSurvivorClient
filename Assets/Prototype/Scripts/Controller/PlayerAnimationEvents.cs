using System.Collections.Generic;
using InfSurvivor.Runtime.Manager;
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
    }
}
