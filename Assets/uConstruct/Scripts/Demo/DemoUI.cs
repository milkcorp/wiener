using UnityEngine;
using UnityEngine.UI;

using System.Collections;


namespace uConstruct.Demo
{

    /// <summary>
    /// A simple demo class that handles the building placer controls
    /// </summary>
    public class DemoUI : MonoBehaviour
    {
        /// <summary>
        /// Our instance.
        /// </summary>
        public static DemoUI instance;

        /// <summary>
        /// Our ui text variable.
        /// </summary>
        [SerializeField]
        Text controls;

        [SerializeField]
        Text inspectedTarget;

        /// <summary>
        /// Our current controls added count.
        /// </summary>
        int controlsCount;
        
        /// <summary>
        /// Initialize Instance.
        /// </summary>
        void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Add a new control to the controls text
        /// </summary>
        /// <param name="name">what that control presents.</param>
        public void AddControl(string name)
        {
            if (controls == null) return;

            controlsCount++;

            controls.text += controlsCount + ". " + name;
            controls.text += "\n";
        }

        /// <summary>
        /// Reset the controls list
        /// </summary>
        public void ResetControl()
        {
            if (controls == null) return;

            controlsCount = 0;
            controls.text = "";
        }

        /// <summary>
        /// Inspect a certain transform.
        /// </summary>
        /// <param name="text">the name of the target.</param>
        public void Inspect(string text)
        {
            if (inspectedTarget == null) return;

            inspectedTarget.text = text;
        }
    }
}