using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using uConstruct.Core.Saving;
using uConstruct.Core.Blueprints;

namespace uConstruct.Conditions
{

    /// <summary>
    /// A basic condition that comes with the asset,
    /// checks if there is an building that you specify in the editor infront of the condition in the distance specified.
    /// </summary>
    public class CheckForBuildingCondition : BaseCondition
    {
        public BuildingType buildings;
        public float distance = 1;
        public DetectionType detectionMethod = DetectionType.Raycast;

        public override bool CheckCondition()
        {
            return detectionMethod == DetectionType.Raycast ? CheckRay() : CheckSphere();
        }

        bool CheckSphere()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, distance);
            Collider hit;
            bool isHit;

            for (int i = 0; i < hits.Length; i++)
            {
                hit = hits[i];

                isHit = CheckHit(hit.transform, hit.transform.position);

                if (isHit)
                    return isHit;
            }

            return false;
        }

        bool CheckHit(Transform hit, Vector3 point)
        {
            BaseBuilding building = hit.GetComponentInParent<BaseBuilding>();

            /*
            BaseBuildingBatcher groupBatcher = hit.GetComponent<BaseBuildingBatcher>();
            if (groupBatcher != null)
            {
                building = groupBatcher.GetBatchedBuilding(point);
            }
             */

            if (building == null) return false;

            return FlagsHelper.IsBitSet(buildings, building.buildingType);
        }

        bool CheckRay()
        {
            RaycastHit[] hits = Physics.RaycastAll(new Ray(transform.position, transform.forward), distance, LayersData.BuildingMask).OrderBy(x => x.distance).ToArray();
            RaycastHit hit;

            for (int i = 0; i < hits.Length; )
            {
                hit = hits[i];

                return CheckHit(hit.transform, hit.point);
            }

            return false;
        }

        public override void OnDrawGizmos()
        {
            Gizmos.color = CheckCondition() ? Color.green:Color.red;

            if (detectionMethod == DetectionType.Raycast)
                Gizmos.DrawRay(transform.position, transform.forward * distance);
            else
                Gizmos.DrawWireSphere(transform.position, distance);
        }

        /// <summary>
        /// Pack our building data
        /// </summary>
        /// <returns>our building data</returns>
        public override BlueprintData Pack()
        {
            return new CheckForBuilding_BlueprintData(this);
        }
    }

    public enum DetectionType
    {
        Raycast,
        Sphere
    }

    [System.Serializable]
    public class CheckForBuilding_BlueprintData : BlueprintData
    {
        public BuildingType buildings;
        public float distance;
        public DetectionType detectionMethod;

        public CheckForBuilding_BlueprintData(CheckForBuildingCondition condition)
        {
            this.name = condition.transform.name;

            this.position = condition.transform.localPosition;
            this.rotation = condition.transform.rotation;
            this.scale = condition.transform.localScale;

            this.buildings = condition.buildings;
            this.distance = condition.distance;
            this.detectionMethod = condition.detectionMethod;
        }

        public override void UnPack(GameObject target)
        {
            BaseBuilding building = target.GetComponentInParent<BaseBuilding>();

            if (building != null)
            {
                CheckForBuildingCondition condition = (CheckForBuildingCondition)building.CreateCondition(name, SocketPositionAnchor.Center, typeof(CheckForBuildingCondition));
                condition.transform.localPosition = (Vector3)position;
                condition.transform.localScale = (Vector3)scale;
                condition.transform.localRotation = (Quaternion)rotation;

                condition.buildings = this.buildings;
                condition.distance = this.distance;
                condition.detectionMethod = this.detectionMethod;
            }
        }

    }

}
