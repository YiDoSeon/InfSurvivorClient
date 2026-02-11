using System;
using InfSurvivor.Runtime.Scenes;
using InfSurvivor.Runtime.Utils;
using UnityEngine.SceneManagement;

namespace InfSurvivor.Runtime.Manager
{
    using static Defines;
    public class SceneManagerEx
    {
        public BaseScene CurrentScene { get; private set; }
        public SceneType NextSceneType { get; private set; }

        public void SetCurrentScene(BaseScene scene)
        {
            CurrentScene = scene;
        }

        public void LoadScene(SceneType type)
        {
            NextSceneType = type;
            Managers.Clear();
            SceneManager.LoadSceneAsync(GetSceneName(SceneType.LoadingScene), LoadSceneMode.Single);
        }

        public string GetSceneName(SceneType type)
        {
            string name = Enum.GetName(typeof(SceneType), type);
            return name;
        }

        public void Clear()
        {
            CurrentScene?.Clear();
        }
    }
}
