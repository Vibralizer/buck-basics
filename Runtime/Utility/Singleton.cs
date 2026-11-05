// MIT License - Copyright (c) 2025 BUCK Design LLC - https://github.com/buck-co

using UnityEngine;

namespace Buck
{
    /// <summary>
    /// Inherit from this base class to create a singleton.
    /// e.g. public class MyClassName : Singleton<MyClassName> {}
    /// Scene and prefab instances claim the singleton slot in Awake, so a lazy
    /// Instance access can never permanently shadow a real instance that simply
    /// had not loaded yet. Subclasses that declare Awake, OnDestroy, or
    /// OnApplicationQuit must override the base methods and call the base
    /// implementation, otherwise the registration logic is skipped for that type.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Check to see if we're about to be destroyed.
        static bool m_ShuttingDown = false;
        static bool m_AppIsQuitting = false;
        static object m_Lock = new object();
        static T m_Instance;

        // True while the registered instance is one the Instance getter created
        // itself (an empty "(Singleton)" GameObject with default field values), as
        // opposed to a scene or prefab instance carrying real serialized data.
        static bool m_InstanceWasAutoCreated = false;

        // Unity does not invoke RuntimeInitializeOnLoadMethod inside a generic type, so every
        // closed Singleton<T> registers its reset here and PlayModeStatics runs it at the start
        // of each Play session. Without it, a session that ends with m_AppIsQuitting set (every
        // editor Play session does) would deny Instance to the next session once its first
        // scene swap destroys an instance, and a lazy Instance access before Awake would return
        // null with the "already destroyed" warning.
        static Singleton()
            => PlayModeStatics.Register(ResetStatics);

        static void ResetStatics()
        {
            lock (m_Lock)
            {
                m_Instance = null;
                m_ShuttingDown = false;
                m_AppIsQuitting = false;
                m_InstanceWasAutoCreated = false;
            }
        }

        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_ShuttingDown)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        "' already destroyed. Returning null.");
                    return null;
                }

                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        // Search for existing instance.
                        m_Instance = (T)FindAnyObjectByType(typeof(T));

                        // Create new instance if one doesn't already exist.
                        if (m_Instance == null)
                        {
                            // Need to create a new GameObject to attach the singleton to.
                            var singletonObject = new GameObject();
                            m_Instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).ToString() + " (Singleton)";
                            m_InstanceWasAutoCreated = true;

                            // If a scene or prefab provides this singleton, the access at
                            // this log's stack ran before that instance existed (e.g. during
                            // an async scene load); the real instance takes over in Awake.
                            Debug.Log("[Singleton] Auto-created '" + singletonObject.name +
                                "' because no instance was found.", singletonObject);

                            // Make instance persistent.
                            DontDestroyOnLoad(singletonObject);
                        }
                        else
                        {
                            m_InstanceWasAutoCreated = false;
                        }
                    }

                    return m_Instance;
                }
            }
        }

        /// <summary>
        /// Scene and prefab instances register themselves the moment they wake instead
        /// of waiting for a lazy Instance access to find them. This closes the window
        /// where an Instance touch during a scene load minted a bare auto-created
        /// instance that permanently shadowed the real one: if that already happened,
        /// the real instance takes over and the auto-created stand-in is destroyed.
        /// </summary>
        protected virtual void Awake()
        {
            lock (m_Lock)
            {
                if (m_Instance == null)
                {
                    m_Instance = GetComponent<T>();
                    m_InstanceWasAutoCreated = false;
                    m_ShuttingDown = false;
                }
                else if (m_InstanceWasAutoCreated && !ReferenceEquals(m_Instance, this))
                {
                    T autoCreated = m_Instance;
                    m_Instance = GetComponent<T>();
                    m_InstanceWasAutoCreated = false;
                    Debug.LogWarning("[Singleton] '" + gameObject.name + "' is taking over as the '" +
                        typeof(T) + "' Instance from auto-created '" + autoCreated.gameObject.name +
                        "' (an Instance access ran before this instance loaded). Destroying the stand-in.",
                        gameObject);
                    Destroy(autoCreated.gameObject);
                }
                // Otherwise a real instance is already registered; this one is a duplicate
                // and, matching the historical behavior, is left alone (first one wins).
            }
        }

        protected virtual void OnApplicationQuit()
        {
            m_AppIsQuitting = true;
            m_ShuttingDown = true;
        }

        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(m_Instance, this))
            {
                if (m_AppIsQuitting)
                {
                    // If the application is quitting, don't allow recreation.
                    m_ShuttingDown = true;
                }
                else
                {
                    // If the instance is destroyed because of scene reload / swap, allow recreation.
                    m_Instance = null;
                    m_ShuttingDown = false;
                    m_InstanceWasAutoCreated = false;
                }
            }
            // If this wasn't the active instance (duplicate), don't do anything.
        }
    }
}
