using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using UnityEngine;

namespace InfSurvivor.Runtime.Scenes
{
    public class BattleScene : BaseScene
    {
        public override Defines.SceneType SceneType => Defines.SceneType.BattleScene;

        protected override void Start()
        {
            base.Start();
            Managers.Network.EnterGame();
        }

        public override void Clear()
        {
        }
    }
}
