using UnityEngine;
using System.Collections;

namespace uConstruct
{

    /// <summary>
    /// A class that is attached to the bathed collider.
    /// used to contain data about the group we are batching.
    /// </summary>
    public class BaseBuildingBatcher : MonoBehaviour, IBuilding
    {
        public BaseBuildingGroup group;

        [System.Obsolete("Collider batching is disabled, this method will not work.")]
        public void DestroyBuilding()
        {
            if (group == null) return;

            /* Collider batcher is obsolote as of the moment.
            var building = GetBatchedBuilding(hit.point);

            if (building != null)
            {
                building.Destroy(hit);
            }
             */
        }

        [System.Obsolete("Collider batching is disabled, this method will not work.")]
        /// <summary>
        /// Get our batch building from the group
        /// </summary>
        /// <param name="point">a point on the building</param>
        /// <returns>our batched building instance</returns>
        public BaseBuilding GetBatchedBuilding(Vector3 point)
        {
            return group.ReturnBatchedBuilding(point);
        }
    }

}
