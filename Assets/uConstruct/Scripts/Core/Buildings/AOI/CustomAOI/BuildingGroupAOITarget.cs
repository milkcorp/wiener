using UnityEngine;
using System.Collections;

namespace uConstruct.Core.AOI
{
    /// <summary>
    /// The building version of AOITarget
    /// </summary>
    public class BuildingGroupAOITarget : BaseAOITarget
    {
        /// <summary>
        /// our building instance
        /// </summary>
        BaseBuildingGroup buildingGroup;

        public Vector3 _totalVectors;
        
        /// <summary>
        /// Total vectors from the group buildings
        /// </summary>
        Vector3 totalVectors
        {
            get
            {
                return _totalVectors;
            }
            set
            {
                _totalVectors = value;
            }
        }

        /// <summary>
        /// the center positon on the group
        /// </summary>
        public Vector3 correctPosition
        {
            get { return (totalVectors / buildingGroup.buildings.Count) + aoiPosition; }
        }

        /// <summary>
        /// Furthest point on the building group.
        /// </summary>
        public float maxPointOnGroup = 0;

        /// <summary>
        /// Used to define a new distance.
        /// </summary>
        float radiusAdjuster = 2f;

        /// <summary>
        /// Add the target to our list
        /// </summary>
        protected override void OnEnable()
        {
            buildingGroup = GetComponent<BaseBuildingGroup>();

            if(buildingGroup != null)
            {
                buildingGroup.OnBuildingAddedEvent += GroupBuildingAdded;
                buildingGroup.OnBuildingRemovedEvent += GroupBuildingRemoved;
            }

            base.OnEnable();
        }
        /// <summary>
        /// Remove the target from the list
        /// </summary>
        protected override void OnDisable()
        {
            if (buildingGroup != null)
            {
                buildingGroup.OnBuildingAddedEvent -= GroupBuildingAdded;
                buildingGroup.OnBuildingRemovedEvent -= GroupBuildingRemoved;
            }

            base.OnDisable();
        }

        /// <summary>
        /// Handle the AOI results
        /// </summary>
        /// <param name="finder">the finder that our results got changed of</param>
        /// <param name="_inRange">are we in range of the finder?</param>
        public override void HandleAOI(BaseAOIFinder finder, bool _inRange)
        {
            base.HandleAOI(finder, _inRange);

            if (buildingGroup == null)
            {
                buildingGroup = GetComponent<BaseBuildingGroup>();
            }

            if (buildingGroup != null)
            {
                buildingGroup.AOIGroup(_inRange, finder.aoiPosition, finder.radius);
            }
        }

        /// <summary>
        /// Called when a building was added to our group.
        /// extend the radius.
        /// </summary>
        /// <param name="building">the added building</param>
        void GroupBuildingAdded(BaseBuilding building)
        {
            totalVectors += building.transform.localPosition;

            float distance = (correctPosition - building.transform.position).magnitude;
            distance += radiusAdjuster;

            if (distance > maxPointOnGroup)
                maxPointOnGroup = distance;
        }

        /// <summary>
        /// Called when a building was removed from our group.
        /// shorten the radius.
        /// </summary>
        /// <param name="building">the removed building</param>
        void GroupBuildingRemoved(BaseBuilding building)
        {
            totalVectors -= building.transform.localPosition;

            float distance = (correctPosition - building.transform.position).magnitude;
            distance += radiusAdjuster;

            if (distance > maxPointOnGroup)
            {
                CalculateNewMaxPoint();
            }
        }

        /// <summary>
        /// Calculate a new max point for the group.
        /// </summary>
        internal void CalculateNewMaxPoint()
        {
            maxPointOnGroup = -Mathf.Infinity;

            float distance;
            BaseBuilding building;

            for(int i = 0; i < buildingGroup.buildings.Count; i++)
            {
                building = buildingGroup.buildings[i];

                distance = (correctPosition - building.transform.position).magnitude;
                distance += radiusAdjuster;

                if (distance >= maxPointOnGroup)
                {
                    maxPointOnGroup = distance;
                }
            }
        }

        /// <summary>
        /// Is the finder in our zone?
        /// </summary>
        /// <param name="finderPos">the finder pos</param>
        /// <param name="radius">the finder radius</param>
        /// <returns>Are we in range?</returns>
        public override bool InZone(Vector3 finderPos, float radius)
        {
            return UCSettings.instance.UCAOICalculationMethod == UCAOIMethod.PerBuilding || (correctPosition - finderPos).magnitude <= radius + maxPointOnGroup;
        }

        /// <summary>
        /// Draw gizmos
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(correctPosition, maxPointOnGroup);
        }

    }
}
