using System;
using UnityEngine;

namespace InfSurvivor.Runtime.Manager
{
    public class Managers : MonoBehaviour
    {
        #region Singleton
        private const string MANGER_NAME = "@Managers";
        public static bool Initialized { get; private set; }
        public static bool IsDestroying { get; private set; } = false;
        private static Managers instance;
        public static Managers Instance
        {
            get
            {
                if (IsDestroying)
                {
                    return null;
                }
                if (instance == null)
                {
                    Managers manager = FindFirstObjectByType<Managers>();
                    if (manager == null)
                    {
                        GameObject go = new GameObject();
                        instance = go.AddComponent<Managers>();
                    }
                    
                    if (instance == null)
                    {
                        instance = manager;

                        DontDestroyOnLoad(manager.gameObject);
                        Init();
                    }
                }
                return instance;
            }
        }
        #endregion

        #region Contents
        private CollisionManager collision = new CollisionManager();
        private NetworkManager network = new NetworkManager();
        private IObjectService @object;

        public static CollisionManager Collision => Instance?.collision;

        public static NetworkManager Network => Instance?.network;
        public static IObjectService Object => Instance?.@object;
        #endregion

        #region Unity 이벤트 함수

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                gameObject.name = MANGER_NAME;

                DontDestroyOnLoad(gameObject);
                Init();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            Network.OnDestroy();
            instance = null;
            Initialized = false;
            IsDestroying = true;
        }

        private void Update()
        {
            Network.Update();
        }

        private void FixedUpdate()
        {
            Collision.OnTick();
        }
        #endregion

        private static void Init()
        {
            Debug.Assert(Initialized == false, "Managers가 중복 초기화되었습니다.");
            //Create(ref instance.collision, nameof(instance.collision));
            instance.collision.InitMatrix();
            instance.@object = objectFactory?.Invoke();
            Network.Init();
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            instance = null;
            Initialized = false;
            IsDestroying = false;
        }

        #region Temp (Will Be Removed)
        private static Func<IObjectService> objectFactory;
        public static void SetObjectService(Func<IObjectService> objectService)
        {
            objectFactory = objectService;
        }
        #endregion
    }
}
