using UnityEngine;
using System.Collections;
using System.Linq;

using uConstruct.Core.Saving;
using uConstruct.Core.Blueprints;

namespace uConstruct.Conditions
{

    /// <summary>
    /// A basic built-in condition that checks if the building has ground. if it doesnt it will add gravity to the object and remove him from the group ( at the end, destroy it ).
    /// </summary>
    public class CheckForGroundCondition : BaseCondition
    {
        private BaseBuildingGroup buildingGroup;

        public float destroyDelay = 10.0f;

        bool calledAlready;

        public override bool DisableOnPlace
        {
            get
            {
                return false;
            }
        }

        public override bool CheckCondition()
        {
            return true;
        }

        public override void Awake()
        {
            base.Awake();

            rootBuilding.OnPlacedOnChangedEvent += (BaseBuilding oldBuilding, BaseBuilding newBuilding) =>
                {
                    if(newBuilding != null)
                    {
                        newBuilding.OnDeattachEvent += () => InitiateCondition(newBuilding);
                    }
                    if(oldBuilding != null)
                    {
                        oldBuilding.OnDeattachEvent -= () => InitiateCondition(oldBuilding);
                    }
                };
        }

        void InitiateCondition(BaseBuilding building)
        {
            if (rootBuilding.placedOn == building)
            {
                AddGravity();
            }
        }

        void AddGravity()
        {
            if (rootBuilding == null || !this.enabled) return;

            MeshCollider[] mColliders = rootBuilding.GetComponentsInChildren<MeshCollider>();

            // Make any mesh colliders convex to allow rigidbody.
            for (int i = 0; i < mColliders.Length; i++)
            {
                mColliders[i].convex = true;
            }

            rootBuilding.enabled = false;
            rootBuilding.EnableRenderings(true);

            var rigid = rootBuilding.GetComponent<Rigidbody>();

            if (rigid == null)
                rigid = rootBuilding.gameObject.AddComponent<Rigidbody>();

            rigid.mass = 5;
            rigid.drag = 0.1f;
            rigid.AddForce(transform.forward * 5);

            if (rootBuilding.hasGroup)
            {
                rootBuilding.buildingGroup.RemoveBuilding(rootBuilding);
            }

            rootBuilding.DeAttachBuilding();
            Destroy(rootBuilding.gameObject, destroyDelay);

            this.enabled = false;
        }

        public override void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(new Ray(transform.position, -transform.up * 20));
        }

        /// <summary>
        /// Pack our building data
        /// </summary>
        /// <returns>our building data</returns>
        public override BlueprintData Pack()
        {
            return new CheckForGround_BlueprintData(this);
        }
    }

    [System.Serializable]
    public class CheckForGround_BlueprintData : BlueprintData
    {
        public float destroyDelay;

        public CheckForGround_BlueprintData(CheckForGroundCondition condition)
        {
            this.name = condition.transform.name;

            this.position = condition.transform.localPosition;
            this.rotation = condition.transform.localRotation;
            this.scale = condition.transform.localScale;

            this.destroyDelay = condition.destroyDelay;
        }

        public override void UnPack(GameObject target)
        {
            BaseBuilding building = target.GetComponentInParent<BaseBuilding>();

            if (building != null)
            {
                CheckForGroundCondition condition = (CheckForGroundCondition)building.CreateCondition(name, SocketPositionAnchor.Center, typeof(CheckForGroundCondition));

                condition.transform.localPosition = (Vector3)position;
                condition.transform.localScale = (Vector3)scale;
                condition.transform.localRotation = (Quaternion)rotation;

                condition.destroyDelay = this.destroyDelay;
            }
        }
    }

}