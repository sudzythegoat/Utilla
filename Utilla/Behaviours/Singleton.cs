using UnityEngine;
using Utilla.Tools;

namespace Utilla.Behaviours
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; protected set; }

        public static bool HasInstance => Instance;

        public virtual bool SingleInstance { get; } = true;

        private T GenericComponent => gameObject.GetComponent<T>();

        public void Awake()
        {
            if (SingleInstance)
            {
                if (HasInstance && Instance != GenericComponent)
                {
                    Destroy(GenericComponent);
                }
                Instance = GenericComponent;
            }
            else if (!HasInstance)
            {
                Instance = GenericComponent;
            }

            Initialize();
        }

        public virtual void Initialize()
        {
            Logging.Info($"Initializing singleton for {typeof(T).Name}");
        }
    }
}