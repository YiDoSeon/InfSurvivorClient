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
        Vector3 offset = (Vector3)(player.CollisionOffset + player.Dir4 * 0.7f);
        float range = 0.75f;
        List<GameObject> objects = Managers.Collision.GetObjectsInRange(player.transform.position, offset, range);
        foreach (GameObject obj in objects)
        {
            Debug.Log(obj.name);            
        }
    }
}
