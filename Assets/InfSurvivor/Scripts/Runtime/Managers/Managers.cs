using Shared.Physics;
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
        private CollisionWorld collision = new CollisionWorld();
        private NetworkManager network = new NetworkManager();
        private ObjectManager @object = new ObjectManager();

        public static CollisionWorld Collision => Instance?.collision;

        public static NetworkManager Network => Instance?.network;
        public static ObjectManager Object => Instance?.@object;
        #endregion

        #region Core
        private ResourceManager resource = new ResourceManager();

        public static ResourceManager Resource => Instance?.resource;
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
            Collision?.Init();
            Network?.Init();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            instance = null;
            Initialized = false;
            IsDestroying = false;
        }
    }
}
