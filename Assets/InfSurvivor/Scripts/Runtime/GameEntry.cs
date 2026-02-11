using InfSurvivor.Runtime.Manager;
using UnityEngine;

namespace InfSurvivor.Runtime
{
    public class GameEntry : MonoBehaviour
    {
        private void Awake()
        {
            /*
                현재 완전히 비어있는 씬에서 어떤 설정인지는 모르지만
                빌드 후 포커싱이 해제된 경우 간헐적으로 얼마의 시간이 흐른뒤
                프로그램이 응답없음 상태로 변경됨.
                의심 되는 원인은 수직동기화, 타겟프레임, DX12 API 정도로 보이며
                지속적인 테스트를 통해 문제를 확인해야함

                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
                26.02.08 - 수직동기화를 끄고 프레임 제한 없음, DX12 (581.80)
                    => 26.02.08 15:50 현상 발생
                
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                26.02.08 - 수직동기화를 끄고 타겟프레임을 60, DX12 (581.80)
                    => 26.02.08 16:33 현상 발생

                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
                26.02.08 - 수직동기화를 끄고 프레임 제한 없음, DX11 (581.80)
            */
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }
    }
}