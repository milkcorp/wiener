using UnityEngine;
using System.Collections;

namespace uConstruct.Core.AOI
{
    /// <summary>
    /// A base aoi finder class
    /// </summary>
    public class BaseAOIFinder : MonoBehaviour
    {
        /// <summary>
        /// our searching radius
        /// </summary>
        public float radius = 20f;
        /// <summary>
        /// our old position, used for checking movement and update AOI only when needed.
        /// </summary>
        Vector3 oldPos = -Vector3.one;

        /// <summary>
        /// Position that is updated by the AOIManager and used by a different thread
        /// </summary>
        public Vector3 aoiPosition;

        /// <summary>
        /// Add the finder to the list
        /// </summary>
        public virtual void OnEnable()
        {
            AOIManager.AddFinder(this);
        }
        /// <summary>
        /// Remove the finder from the list
        /// </summary>
        public virtual void OnDisable()
        {
            AOIManager.RemoveFinder(this);
        }
        /// <summary>
        /// Update the AOI of the finder.
        /// </summary>
        public virtual void UpdateAOI()
        {
            if((transform.position - oldPos).magnitude > 2)
            {
                AOIManager.UpdateAOI(this);
                oldPos = transform.position;
            }
        }

        /// <summary>
        /// Draw gizmos
        /// </summary>
        public virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, radius);
        }

        /// <summary>
        /// Calls the update on the AOI.
        /// </summary>
        public virtual void Update()
        {
            UpdateAOI();
        }

    }
}