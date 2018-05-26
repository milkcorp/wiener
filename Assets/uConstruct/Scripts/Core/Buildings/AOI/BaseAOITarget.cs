using UnityEngine;
using System.Collections;

namespace uConstruct.Core.AOI
{
    /// <summary>
    /// Base AOITarget class.
    /// </summary>
    public abstract class BaseAOITarget : MonoBehaviour
    {
        /// <summary>
        /// Are we in range of any of the finders
        /// </summary>
        public bool inRange = true;

        /// <summary>
        /// Calculate duplicates checks.
        /// </summary>
        public virtual bool calculateDuplicates
        {
            get
            {
                return UCSettings.instance.UCAOICalculationMethod != UCAOIMethod.PerBuilding;
            }
        }

        /// <summary>
        /// Position that is updated by the AOIManager and used by a different thread
        /// </summary>
        public Vector3 aoiPosition;

        /// <summary>
        /// Will the system use multi-threading to choose whether this target is in range of a finder or not.
        /// </summary>
        public virtual bool useMultiThreadZoneSearch
        {
            get { return true; }
        }

        /// <summary>
        /// Add the target to the targets list
        /// </summary>
        protected virtual void OnEnable()
        {
            AOIManager.AddTarget(this);
        }
        /// <summary>
        /// Remove the target from the targets list
        /// </summary>
        protected virtual void OnDisable()
        {
            AOIManager.RemoveTarget(this);
        }

        /// <summary>
        /// Handle AOI Change
        /// </summary>
        /// <param name="finder">The finder that we got in range/ out of range of.</param>
        /// <param name="_inRange">Are we in range of the finder or out of range of the finder?</param>
        public virtual void HandleAOI(BaseAOIFinder finder, bool _inRange)
        {
            //add your code when inheriting...
            if (!calculateDuplicates || inRange == _inRange) return; // make sure we arent receiving same data.

            this.inRange = _inRange;
        }

        /// <summary>
        /// Are we in zone of this target?
        /// </summary>
        /// <param name="finderPos">Our finder position</param>
        /// <param name="radius">Finder's radius</param>
        /// <returns>Are we in range ?</returns>
        public virtual bool InZone(Vector3 finderPos, float radius)
        {
            return true;
        }

    }
}
