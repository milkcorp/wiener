using UnityEngine;
using System.Collections;

using System.Linq;

namespace uConstruct.Sockets
{
    /// <summary>
    /// Snap points are points on your building which are used for dynamically choosing an anchor for placing the building based on distances.
    /// </summary>
    public class BaseSnapPoint : MonoBehaviour
    {
        public BaseBuilding building;

        public BuildingType receiveType;

        /// <summary>
        /// Get the resulted anchored position
        /// <param name="origin">the origin of the anchor</param>
        /// <returns>the resulting anchor.</returns>
        /// </summary>
        public virtual Vector3 AnchoredPosition(Vector3 renderCenter, Vector3 renderSize, Vector3 origin)
        {
            /*
            renderSize.x = renderSize.x * (origin.x > renderCenter.x ? 1 : origin.x == renderCenter.x ? 0 : -1);
            renderSize.y = renderSize.y * (origin.y > renderCenter.y ? -1 : origin.y == renderCenter.y ? 0 : 1);
            renderSize.z = renderSize.z * (origin.z > renderCenter.z ? 1 : origin.z == renderCenter.z ? 0 : -1);

            return transform.position - new Vector3((renderCenter.x - (renderCenter.x - (renderSize.x))), 0, (renderCenter.z - (renderCenter.z - (renderSize.z))));
             */

            renderSize.x = renderSize.x * (transform.position.x > renderCenter.x ? 1 : transform.position.x == renderCenter.x ? 0 : -1);
            renderSize.y = renderSize.y * (transform.position.y > renderCenter.y ? 1 : transform.position.y == renderCenter.y ? 0 : -1);
            renderSize.z = renderSize.z * (transform.position.z > renderCenter.z ? 1 : transform.position.z == renderCenter.z ? 0 : -1);

            return transform.position - new Vector3(renderSize.x / 2, 0, renderSize.z / 2);
        }

        /// <summary>
        /// Return our distance from the target.
        /// <param name="target">our target</param>
        /// <returns>distance to our target</returns>
        /// </summary>
        public virtual float ReturnDistance(Vector3 pos)
        {
            return Vector3.Distance(this.transform.position, pos);
        }

        /// <summary>
        /// Initialize snap point.
        /// </summary>
        protected virtual void Awake()
        {
            building = GetComponentInParent<BaseBuilding>();
        }

        /// <summary>
        /// Snap this point and stash it.
        /// </summary>
        /// <param name="owner">who do we belong to?</param>
        public virtual void Snap(Transform owner)
        {
            owner.transform.position = AnchoredPosition(owner.transform.GetRendererCenter(), owner.transform.GetRendererSize(), owner.transform.position);
        }

        /// <summary>
        /// Draw gizmos
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.1f);
        }

        /// <summary>
        /// Return the closest point to the target from the points.
        /// </summary>
        /// <param name="points">our snap points.</param>
        /// <returns>closest snap point to the target.</returns>
        public static BaseSnapPoint ReturnClosest(BaseSnapPoint[] points, Vector3 pointInfluence, BuildingType type)
        {
            BaseSnapPoint closestPoint = null;
            float closestRange = 999;

            BaseSnapPoint point;
            float distance = 0.0f;

            for (int i = 0; i < points.Length; i++)
            {
                point = points[i];

                distance = point.ReturnDistance(pointInfluence);
                if (distance <= closestRange && FlagsHelper.IsBitSet<BuildingType>(point.receiveType, type))
                {
                    closestPoint = point;
                    closestRange = distance;
                }
            }

            return closestPoint;
        }
    }
}
