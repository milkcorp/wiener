using UnityEngine;
using System.Collections;
using uConstruct;
using uConstruct.Conditions;
using uConstruct.Sockets;
using System.Linq;

using System.Collections.Generic;

namespace uConstruct.Core.Physics
{
    /// <summary>
    /// This is a base class for a UCPhysicsObject.
    /// Every class that inherites this class will be counted in the physics system.
    /// </summary>
    public abstract class UCPhysicsObject : MonoBehaviour
    {
        public static Color GizmosColor = new Color(0, 0, 0, 0.2f);

        [SerializeField]
        Vector3 _center = Vector3.zero;
        public Vector3 center
        {
            get { return _center; }
            set
            {
                _center = value;
                UpdateBounds(center, size);
            }
        }

        [SerializeField]
        public Vector3 _size = Vector3.one;
        public Vector3 size
        {
            get { return _size; }
            set
            {
                _size = value;
                UpdateBounds(center, size);
            }
        }

        [SerializeField]
        private Bounds Bounds;

        [SerializeField]
        public bool _usePhysics = true;
        public bool usePhysics
        {
            get { return _usePhysics; }
            set { _usePhysics = value; }
        }

        UCPhysicsHit hit = new UCPhysicsHit();

        bool inList = false;

        /// <summary>
        /// Update object's bounds
        /// </summary>
        /// <param name="center">The center of the bounds, worldspace</param>
        /// <param name="size">The size of the bounds, worldspace</param>
        void UpdateBounds(Vector3 center, Vector3 size)
        {
            Vector3 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            Vector3 max = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

            Vector3[] points = new Vector3[8];
            Vector3 extents = size / 2;

            points[0] = transform.TransformPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z));
            points[1] = transform.TransformPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z));
            points[2] = transform.TransformPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z));
            points[3] = transform.TransformPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z));
            points[4] = transform.TransformPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z));
            points[5] = transform.TransformPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z));
            points[6] = transform.TransformPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z));
            points[7] = transform.TransformPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z));

            Vector3 point;
            for(int i = 0; i < points.Length; i++)
            {
                point = points[i];

                if (point.x < min.x) min.x = point.x;
                if (point.x > max.x) max.x = point.x;
                if (point.y < min.y) min.y = point.y;
                if (point.y > max.y) max.y = point.y;
                if (point.z < min.z) min.z = point.z;
                if (point.z > max.z) max.z = point.z;
            }

            Vector3 boundsSize = new Vector3(max.x - min.x, max.y - min.y, max.z - min.z);
            Bounds = new UnityEngine.Bounds(new Vector3(min.x + boundsSize.x / 2f, min.y + boundsSize.y / 2f, min.z + boundsSize.z / 2f), new Vector3(max.x - min.x, max.y - min.y, max.z - min.z));
        }

        /// <summary>
        /// Add to physics simulation and update bounds
        /// </summary>
        void Start() // run this and also onEnable cause sometimes on enable isnt called.
        {
            UpdateBounds(center, size);

            if (!inList)
            {
                UCPhysics.AddPhysicsObject(this);
                inList = true;
            }
        }

        /// <summary>
        /// Add object to physics simulation
        /// </summary>
        void OnEnable()
        {
            if (inList == false)
            {
                UCPhysics.AddPhysicsObject(this);
                inList = true;
            }
        }

        /// <summary>
        /// Remove physics object from physics simulation
        /// </summary>
        void OnDisable()
        {
            UCPhysics.RemovePhysicsObject(this);
            inList = false;
        }

        /// <summary>
        /// Draw gizmos
        /// </summary>
        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            DrawShape(transform.localToWorldMatrix, UnityEditor.Selection.transforms.Contains(transform));
#endif
        }

        /// <summary>
        /// Draw the shape of the bounds
        /// </summary>
        /// <param name="matrix">the matrix of the bounds</param>
        /// <param name="selected">is the shape selected in heirachy</param>
        void DrawShape(Matrix4x4 matrix, bool selected)
        {
            if (!enabled || !usePhysics) return;

            Gizmos.color = new Color(GizmosColor.r, GizmosColor.g, GizmosColor.b, 1f);
            Gizmos.matrix = matrix;

            UpdateBounds(center, this.size);

            if (selected)
            {
                Gizmos.DrawCube(center, size);
            }
            else
            {
                Gizmos.DrawWireCube(center, size);
            }
        }

        /// <summary>
        /// Raycast the physics object
        /// </summary>
        /// <param name="origin">ray origin</param>
        /// <param name="direction">ray direction</param>
        /// <param name="_hit">hit data</param>
        /// <param name="distance">max distance</param>
        /// <param name="mask">layerMask</param>
        /// <returns>Did we hit something?</returns>
        public bool Raycast(Ray ray, out UCPhysicsHit _hit, LayerMask mask)
        {
            _hit = this.hit;

            if (!usePhysics)
            {
                return false;
            }

            UpdateBounds(center, size); // update bounds to make sure that if the object moves, the new bounds will be applied.
            if (Bounds.IntersectRay(ray, out _hit.distance))
            {
                _hit.transform = this.transform;
                _hit.point = ray.GetPoint(_hit.distance);

                return VerifyUnityCollisions(_hit, ray);
            }

            return false;
        }

        bool VerifyUnityCollisions(UCPhysicsHit hit, Ray ray)
        {
            BaseBuilding _tempBuilding;
            BaseSocket _socket = hit.transform.GetComponent<BaseSocket>();
            IUTCPhysicsIgnored ignoreInterface;

            if (_socket == null) return true;

            RaycastHit[] hits = UnityEngine.Physics.RaycastAll(ray.origin, (_socket.transform.position - ray.origin), hit.distance).OrderBy(x => x.distance).ToArray();
            RaycastHit _hit;

            for (int i = 0; i < hits.Length; i++)
            {
                _hit = hits[i];
                ignoreInterface = _hit.transform.GetComponent<IUTCPhysicsIgnored>();

                if (ignoreInterface != null && ignoreInterface.ignore) continue;

                _tempBuilding = _hit.transform.GetComponentInParent<BaseBuilding>();

                if (_tempBuilding == null) return true;

                if (_tempBuilding == _socket.building)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

    }

}