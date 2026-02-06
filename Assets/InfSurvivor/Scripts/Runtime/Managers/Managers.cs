using UnityEngine;

namespace InfSurvivor.Runtime.Manager
{
    public class Managers : MonoBehaviour
    {
        #region Singleton
        private const string MANGER_NAME = "@Managers";
        public static bool Initialized { get; private set; }
        private static Managers instance;
        public static Managers Instance
        {
            get
            {
                if (instance == null)
                {
                    Managers manager = FindFirstObjectByType<Managers>();
                    if (manager == null)
                    {
                        GameObject go = new GameObject();
                        go.AddComponent<Managers>();                        
                    }
                }
                return instance;
            }
        }
        #endregion

        #region Contents
        private CollisionManager collision;
        public static CollisionManager Collision => Instance.collision;
        #endregion

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                gameObject.name = MANGER_NAME;

                DontDestroyOnLoad(gameObject);
                Init();
            }
            else if(instance != this)
            {
                Destroy(gameObject);
            }
        }

        private static void Init()
        {
            Debug.Assert(Initialized == false, "Managers가 중복 초기화되었습니다.");
            Create(ref instance.collision, nameof(instance.collision));
        }

        private static void Create<T>(ref T target, string name) where T : BehaviourManagerBase
        {
            Debug.Assert(target == null, "이미 생성되었습니다.");
            GameObject go = new GameObject($"@{name}");
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
