using UnityEngine;
using System.Collections;

namespace uConstruct
{
    /// <summary>
    /// A class that is attached to the socket preview object to contain data about the prefab and apply changes to the prefab.
    /// </summary>
    public class PreviewBuilding : MonoBehaviour
    {
        /// <summary>
        /// Our preview prefab.
        /// </summary>
        public GameObject previewPrefab;

        /// <summary>
        /// Apply changes to the prefab from the transform (rotation and scale).
        /// </summary>
        /// <param name="prefab">our prefab</param>
        public void ApplyChangesToPrefab(GameObject prefab)
        {
            Transform oldParent = transform.parent;

            if(prefab != null)
            {
                transform.parent = null;
                prefab.transform.localScale = transform.localScale;
                prefab.transform.rotation = transform.rotation;
                transform.parent = oldParent;
            }
        }

        /// <summary>
        /// Fits the transform to fit the parent scale. (changes localScale to 1,1,1)
        /// </summary>
        public void FitToLocalSpace()
        {
            this.transform.localScale = new Vector3(1, 1, 1);
        }

    }
}
