using UnityEngine;

using System.IO;

using System.Collections;
using System.Collections.Generic;
using uConstruct.Core.Saving;

using uConstruct.Core.Threading;

namespace uConstruct.Core.Manager
{

    /// <summary>
    /// This class needs to be initiated on the start on the game and it handles loading and saving,
    /// it has control over all unity callbacks and you can use it to add some static OnApplicationQuit callbacks.
    /// </summary>
    public class UCCallbacksManager : MonoBehaviour
    {
        /// <summary>
        /// Project name (uConstruct folder name).
        /// </summary>
        public const string ProjectName = "uConstruct";
        /// <summary>
        /// The found path to the project directory (based on the name provided on ProjectName).
        /// </summary>
        public static string ProjectPath
        {
            get
            {

#if UNITY_EDITOR

                string[] directories;

#if UNITY_WEBPLAYER
                directories = Directory.GetDirectories(@"Assets", ProjectName);
#else
                directories = Directory.GetDirectories(@"Assets", ProjectName, SearchOption.AllDirectories);
#endif

                for (int i = 0; i < directories.Length; i++)
                {
                    if (directories[i].Contains(ProjectName))
                        return directories[i] + "/";
                }

#endif

                return "";
            }
        }

        static UCCallbacksManager _instance;
        public static UCCallbacksManager instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateAndInitialize();
                }

                return _instance;
            }
        }

        List<System.Action> OnApplicationQuitActions = new List<System.Action>();

        /// <summary>
        /// Called when application quits
        /// </summary>
        void OnApplicationQuit()
        {
            ThreadManager.StopThread();
            UCSavingManager.Save();

            System.Action action;
            for (int i = 0; i < OnApplicationQuitActions.Count; i++)
            {
                action = OnApplicationQuitActions[i];

                action.Invoke();
            }
        }

        /// <summary>
        /// Assign instance and start thread.
        /// </summary>
        void Awake()
        {
            _instance = this;

            ThreadManager.StartThread();
        }

        /// <summary>
        /// Load data on start to avoid miss-order.
        /// </summary>
        void Start()
        {
            UCSavingManager.Load();
        }

        /// <summary>
        /// Update the unity thread.
        /// </summary>
        void Update()
        {
            ThreadManager.UnityThread();
        }

        /// <summary>
        /// Initialize and create an instance of the callbacks manager
        /// </summary>
        public static void CreateAndInitialize()
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<UCCallbacksManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject();
                    _instance = go.AddComponent<UCCallbacksManager>();
                    go.transform.name = "uConstructCallbacksManager";
                }
            }
        }

        /// <summary>
        /// Add an action to the application quit data
        /// </summary>
        /// <param name="action">the action</param>
        public void AddApplicationQuitAction(System.Action action)
        {
            if (!OnApplicationQuitActions.Contains(action))
                OnApplicationQuitActions.Add(action);
        }

#if UC_Free && UNITY_EDITOR
        [UnityEditor.Callbacks.PostProcessScene]
        public static void OnBuilding()
        {
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
            {
                UnityEditor.EditorApplication.Exit(0);

                throw new IOException("YOU MUST PURCHASE uConstruct in order to use it inside a build. Remove uConstruct from the project in order to gain the option to build the game again.");
            }
        }
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void OnCompiling()
        {
            if(UnityEditor.PlayerSettings.apiCompatibilityLevel != UnityEditor.ApiCompatibilityLevel.NET_2_0_Subset)
            {
                Debug.LogError("uConstruct Free not installed properly!, API compatibility level doesnt match. FIXING!");

                UnityEditor.PlayerSettings.apiCompatibilityLevel = UnityEditor.ApiCompatibilityLevel.NET_2_0_Subset;
            }
        }
#endif
    }
}