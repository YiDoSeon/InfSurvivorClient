using UnityEngine;

namespace InfSurvivor.Runtime.Manager
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

        #region Contents
        private CollisionManager collision;
        public static CollisionManager Collision => Instance.collision;
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
                Create(ref instance.collision, nameof(instance.collision));
            }
        }

        private static void Create<T>(ref T target, string name) where T : BehaviourManagerBase
        {
            Debug.Assert(target == null, "이미 생성되었습니다.");
            GameObject go = new GameObject(name);
            go.transform.parent = instance.transform;
            target = go.AddComponent<T>();
            target.Init();
        }
        
        private static void Create<T>(ref T target, string name, string path) where T : BehaviourManagerBase
        {
            // TODO: Load Resource And Create
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
