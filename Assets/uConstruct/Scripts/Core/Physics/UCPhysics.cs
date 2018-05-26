using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uConstruct;
using uConstruct.Conditions;
using uConstruct.Sockets;

using StopWatch = System.Diagnostics.Stopwatch;

namespace uConstruct.Core.Physics
{

    /// <summary>
    /// This class handles all custom physics.
    /// </summary>
    public static class UCPhysics
    {
        public readonly static List<UCPhysicsObject> physicsObjects = new List<UCPhysicsObject>();
        static UCPhysicsObject currentPObject;
        static RaycastHit rayHit;
        static Ray ray;
        static UCPhysicsHitsArray hits;

        /// <summary>
        /// Add a physics object to the physics simulation
        /// </summary>
        /// <param name="pObject">the object you want to add</param>
        public static void AddPhysicsObject(UCPhysicsObject pObject)
        {
            physicsObjects.Add(pObject);
        }
            
        /// <summary>
        /// Remove a physics object from the physics simulation
        /// </summary>
        /// <param name="pObject">the object you want to remove</param>
        public static void RemovePhysicsObject(UCPhysicsObject pObject)
        {
            physicsObjects.Remove(pObject);
        }

        /// <summary>
        ///  Create a raycast
        /// </summary>
        /// <param name="origin">The origin of the raycast</param>
        /// <param name="direction">The direction of the raycast</param>
        /// <param name="distance">max distance</param>
        /// <param name="mask">mask</param>
        /// <param name="offset">raycast offset</param>
        /// <returns>The hits value</returns>
        public static UCPhysicsHitsArray RaycastAll(Vector3 origin, Vector3 direction, float distance, int mask, float offset)
        {
            UCPhysicsHit hit;
            hits = new UCPhysicsHitsArray();
            ray = new Ray(origin, direction);

            for (int i = 0; i < physicsObjects.Count; i++)
            {
                currentPObject = physicsObjects[i];

                if (Vector3.Distance(origin, currentPObject.transform.position) <= distance)
                {
                    if (currentPObject.Raycast(ray, out hit, mask))
                    {
                        if (hit.distance <= distance && hit.distance >= offset)
                        {
                            hits.AddToList(hit);
                        }
                    }
                }
            }

            if (UnityEngine.Physics.Raycast(origin, direction, out rayHit, distance))
            {
                if (rayHit.transform.GetComponent<UCPhysicsObject>() != null)
                {
                    hit = new UCPhysicsHit();
                    hit.Convert(rayHit);

                    hits.AddToList(hit);
                }
            }

            return hits;
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="ray">The ray of the raycast</param>
        /// <param name="distance">Max distance</param>
        /// <param name="mask">LayerMask</param>
        /// <returns>Returns the hits array</returns>
        public static UCPhysicsHitsArray RaycastAll(Ray ray, float distance, int mask)
        {
            return RaycastAll(ray.origin, ray.direction, distance, mask, 0);
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="ray">The ray of the raycast</param>
        /// <param name="distance">Max distance</param>
        /// <param name="mask">LayerMask</param>
        /// <param name="offset">raycast offset</param>
        /// <returns>Returns the hits array</returns>
        public static UCPhysicsHitsArray RaycastAll(Ray ray, float distance, int mask, float offset)
        {
            return RaycastAll(ray.origin, ray.direction, distance, mask, offset);
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="origin">the origin of the raycast</param>
        /// <param name="direction">the direction of the raycast</param>
        /// <param name="hit">returns the hit data</param>
        /// <param name="distance">max distance</param>
        /// <param name="mask">layerMask</param>
        /// <param name="offset">raycast offset</param>
        /// <returns>did we hit something?</returns>
        public static bool Raycast(Vector3 origin, Vector3 direction, out UCPhysicsHit hit, float distance, int mask, float offset)
        {
            UCPhysicsHitsArray hits = RaycastAll(origin, direction, distance, mask, offset);
            hit = new UCPhysicsHit();

            if (hits.Count <= 0) return false;

            hits.Sort();

            hit = hits[0];
            return true;
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="origin">the origin of the raycast</param>
        /// <param name="direction">the direction of the raycast</param>
        /// <param name="hit">returns the hit data</param>
        /// <param name="distance">max distance</param>
        /// <param name="mask">layerMask</param>
        /// <returns>did we hit something?</returns>
        public static bool Raycast(Vector3 origin, Vector3 direction, out UCPhysicsHit hit, float distance, int mask)
        {
            UCPhysicsHitsArray hits = RaycastAll(origin, direction, distance, mask, 0);
            hit = new UCPhysicsHit();

            if (hits.Count <= 0) return false;

            hits.Sort();

            hit = hits[0];
            return true;
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="origin">The origin of the ray</param>
        /// <param name="direction">The direction of the ray</param>
        /// <param name="hit">The hit data of the ray</param>
        /// <param name="distance">max distance</param>
        /// <returns>did we hit something?</returns>
        public static bool Raycast(Vector3 origin, Vector3 direction, out UCPhysicsHit hit, float distance)
        {
            return Raycast(origin, direction, out hit, distance, -1);
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="ray">the ray of the raycast</param>
        /// <param name="hit">the hit data</param>
        /// <param name="distance">max distance</param>
        /// <returns></returns>
        public static bool Raycast(Ray ray, out UCPhysicsHit hit, float distance, bool TakeUnityPhysicsIntoAccount, Transform target)
        {
            return Raycast(ray.origin, ray.direction, out hit, distance, -1);
        }
        /// <summary>
        /// Creates a raycast
        /// </summary>
        /// <param name="ray">the ray of the raycast</param>
        /// <param name="hit">returns the hit data</param>
        /// <param name="distance">max distance</param>
        /// <param name="mask">layerMask</param>
        /// <returns>did we hit something?</returns>
        public static bool Raycast(Ray ray, out UCPhysicsHit hit, float distance, int mask)
        {
            return Raycast(ray.origin, ray.direction, out hit, distance, mask);
        }

    }

    /// <summary>
    /// An custom array that holds all ray results in an array
    /// </summary>
    public class UCPhysicsHitsArray
    {
        private List<UCPhysicsHit> _data;

        public UCPhysicsHitsArray()
        {
            _data = new List<UCPhysicsHit>();
        }

        public UCPhysicsHit this[int index]
        {
            get { return _data[index]; }
        }
        public void AddToList(UCPhysicsHit hit)
        {
            _data.Add(hit);
        }
        public void Sort()
        {
            _data.Sort(delegate(UCPhysicsHit a, UCPhysicsHit b)
            {
                return a.distance.CompareTo(b.distance);
            });
        }
        public int Count
        {
            get { return _data.Count; }
        }
    }

    /// <summary>
    /// A class that holds the data for the hit data
    /// </summary>
    public class UCPhysicsHit
    {
        public Transform transform;
        public Vector3 point;
        public Vector3 normal = -Vector3.one;
        public float distance;

        /// <summary>
        /// Convert a raycastHit to UCPhysicsHit 
        /// </summary>
        /// <param name="hit"></param>
        public void Convert(RaycastHit hit)
        {
            this.transform = hit.transform;
            this.point = hit.point;
            this.normal = hit.normal;
            this.distance = hit.distance;
        }
    }
    /// <summary>
    /// Ignore all physics on this script.
    /// </summary>
    interface IUTCPhysicsIgnored
    {
        bool ignore { get; }
    }
}