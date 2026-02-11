using System.Collections;
using InfSurvivor.Runtime.Manager;
using InfSurvivor.Runtime.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfSurvivor.Runtime.Scenes
{
    using static Defines;
    public class LoadingScene : BaseScene
    {
        public override Defines.SceneType SceneType => Defines.SceneType.LoadingScene;
        // TODO: 리소스 로딩 방식으로 변경
        [SerializeField] private LoadingPopup loadingPopup;
        private SceneType nextScene;

        protected override void Start()
        {
            base.Start();

            nextScene = Managers.Scene.NextSceneType;
            StartCoroutine(LoadNextScene());
        }

        public override void Clear()
        {
        }

        private IEnumerator LoadNextScene()
        {
            yield return new WaitForSeconds(1f);

            AsyncOperation operation = SceneManager.LoadSceneAsync(Managers.Scene.GetSceneName(SceneType.BattleScene));
            operation.allowSceneActivation = false;

            float fakeLoadingTime = 2f;

            while (fakeLoadingTime > 0f)
            {
                fakeLoadingTime -= Time.deltaTime;
                loadingPopup.SetProgressbarValue(1f - fakeLoadingTime/2f);
                yield return null;
            }

            while (operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.priority / 0.9f);
                Debug.Log(progress);

                if (operation.progress >= 0.9f)
                {
                    loadingPopup.SetProgressbarValue(1f);
                    break;
                }

                yield return null;
            }

            operation.allowSceneActivation = true;
            Managers.Clear();
        }
    }
}
