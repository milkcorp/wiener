using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using uConstruct.Core.Templates;
using uConstruct.Core.Blueprints;

namespace uConstruct.Conditions
{

    [System.Serializable]
    /// <summary>
    /// A base condition that should be inherited from when creating conditions.
    /// </summary>
    public partial class BaseCondition : MonoBehaviour, uConstruct.Core.Physics.IUTCPhysicsIgnored, IBlueprintItem
    {
        /// <summary>
        /// The building of this condition
        /// </summary>
        [HideInInspector]
        public BaseBuilding rootBuilding;

        /// <summary>
        /// Will this condition be disabled when placing the building
        /// </summary>
        [HideInInspector]
        public virtual bool DisableOnPlace
        {
            get { return false; }
        }

        /// <summary>
        /// Ignore physics on this condition ?
        /// </summary>
        public virtual bool ignore
        {
            get { return true; }
        }

        /// <summary>
        /// Called when the building is being placed, checks for the condition.
        /// </summary>
        /// <returns>Is the condition applied?</returns>
        public virtual bool CheckCondition()
        {
            return true;
        }

        /// <summary>
        /// Called when gizmos is drawing, can be used to debug your condition.
        /// </summary>
        public virtual void OnDrawGizmos()
        {
        }

        /// <summary>
        /// Called on awake to make sure rootParent isnt null
        /// </summary>
        public virtual void Awake()
        {
            if(rootBuilding == null)
            {
                rootBuilding = transform.GetComponentInParent<BaseBuilding>();
            }
        }

        /// <summary>
        /// Pack our building data
        /// </summary>
        /// <returns>our building data</returns>
        public virtual BlueprintData Pack()
        {
            return null;
        }

        /// <summary>
        /// Our priority.
        /// </summary>
        public virtual int priority
        {
            get
            {
                return 3;
            }
        }

    }

    /// <summary>
    /// A partial class for conditions that handles templates
    /// </summary>
    public partial class BaseCondition : MonoBehaviour, ITemplateObject
    {
        public Transform GetTransform()
        {
            return transform;
        }
    }

}