using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uConstruct.Sockets;
using uConstruct.Core.Blueprints;
using uConstruct.Core.Saving;

namespace uConstruct.Conditions
{

    /// <summary>
    /// This class is a built-in condition that comes with the asset.
    /// it checks for any collision while placing the object, to make sure you arent placing buildings inside buildings and so on.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CheckForCollisionCondition : BaseCondition
    {
        #region Variables
        BoxCollider _collider;
        Rigidbody _rigid;

        public List<string> allowedTags = new List<string>();
        public List<Collider> collisions = new List<Collider>();

        public override bool DisableOnPlace
        {
            get
            {
                return true;
            }
        }
        #endregion

        public override void Awake()
        {
            base.Awake();

            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            _collider = GetComponent<BoxCollider>();

            if (_collider == null)
            {
                _collider = gameObject.AddComponent<BoxCollider>();
            }

            _collider.isTrigger = true;

            // ------------------------------------

            _rigid = GetComponent<Rigidbody>();

            if (_rigid == null)
            {
                _rigid = gameObject.AddComponent<Rigidbody>();
            }

            _rigid.isKinematic = true;
        }

        public override bool CheckCondition()
        {
            RemoveNullReferences();

            return collisions.Count == 0;
        }

        void RemoveNullReferences()
        {
            Collider collider;
            var originalList = collisions;

            for (int i = 0; i < originalList.Count; i++)
            {
                collider = originalList[i];

                if (collider == null || !collider.enabled || allowedTags.Contains(collider.gameObject.tag))
                {
                    collisions.Remove(collider);
                }
            }
        }

        void AddCollider(Collider collider)
        {
            if (!collisions.Contains(collider) && collider.enabled && !allowedTags.Contains(collider.gameObject.tag) && !collider.transform.Equals(rootBuilding) && !collider.isTrigger) // make sure we dont collide with sockets or our own building
            {
                collisions.Add(collider);
            }
        }

        void RemoveCollider(Collider collider)
        {
            if (collisions.Contains(collider))
                collisions.Remove(collider);
        }

        void OnTriggerEnter(Collider collision)
        {
            AddCollider(collision);
        }

        void OnTriggerExit(Collider collision)
        {
            RemoveCollider(collision);
        }

        /// <summary>
        /// Pack our building data
        /// </summary>
        /// <returns>our building data</returns>
        public override BlueprintData Pack()
        {
            return new CheckForCollision_BlueprintData(this);
        }

    }

    [System.Serializable]
    public class CheckForCollision_BlueprintData : BlueprintData
    {
        public List<string> allowedTags;
        public SerializeableVector3 ceneter;
        public SerializeableVector3 size;

        public CheckForCollision_BlueprintData(CheckForCollisionCondition condition)
        {
            this.name = condition.transform.name;

            this.position = condition.transform.localPosition;
            this.rotation = condition.transform.localRotation;
            this.scale = condition.transform.localScale;

            this.allowedTags = condition.allowedTags;

            BoxCollider collider = condition.GetComponent<BoxCollider>();

            if (collider != null)
            {
                this.ceneter = collider.center;
                this.size = collider.size;
            }
        }

        public override void UnPack(GameObject target)
        {
            BaseBuilding building = target.GetComponentInParent<BaseBuilding>();

            if (building != null)
            {
                CheckForCollisionCondition condition = (CheckForCollisionCondition)building.CreateCondition(name, SocketPositionAnchor.Center, typeof(CheckForCollisionCondition));

                BoxCollider collider = condition.GetComponent<BoxCollider>();

                if (collider != null)
                {
                    collider.center = (Vector3)this.ceneter;
                    collider.size = (Vector3)this.size;
                }

                condition.transform.localPosition = (Vector3)position;
                condition.transform.localScale = (Vector3)scale;
                condition.transform.localRotation = (Quaternion)rotation;

                condition.allowedTags = this.allowedTags;
            }
        }
    }

}