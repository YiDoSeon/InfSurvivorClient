using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace InfSurvivor.Runtime
{
    /// <summary>
    /// 윈도우에서 게임창을 닫았을 때 간헐적으로 프로세스가 백그라운드에서 종료되지 않는 현상이 있음 <br/>
    /// https://discussions.unity.com/t/bug-unity-6-build-game-process-continues-running-in-background-after-closing-window/1573387/17 <br/>
    /// 위 링크를 참고하여 어플리케이션 종료시 강제 종료 처리를 적용함
    /// </summary>
    public class AppTerminator : MonoBehaviour
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int TerminateProcess(IntPtr hProcess, uint exitCode);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeOnLoad()
        {
            Application.quitting += Application_Quitting;
        }

        private static void Application_Quitting()
        {
            IntPtr handle = System.Diagnostics.Process.GetCurrentProcess().Handle;
            TerminateProcess(handle, 0);
        }
#endif
    }
}
