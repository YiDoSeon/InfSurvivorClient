using UnityEngine;

namespace InfSurvivor.Runtime
{
    public class Managers : MonoBehaviour
    {
        #region Singleton
        public static bool Initialized { get; private set; }
        private static Managers instance;
        public static Managers Instance
        {
            get
            {
                Init();
                return instance;
            }
        }
        #endregion

        private static void Init()
        {
            if (instance == null && Initialized == false)
            {
                Initialized = true;

                GameObject go = GameObject.Find("@Managers");
                if (go == null)
                {
                    go = new GameObject("@Managers");
                    go.AddComponent<Managers>();
                }

                DontDestroyOnLoad(go);

                instance = go.GetComponent<Managers>();
            }
        }

        private void Update()
        {

        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            instance = null;
            Initialized = false;
        }
        
    }
}
