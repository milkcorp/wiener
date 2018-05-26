using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uConstruct.Core.Threading;
using uConstruct.Core.Physics;
using System.Diagnostics;

namespace uConstruct.Sockets
{
    public class SOverlapThreshold
    {
        static Vector3 tempPosition;

        static readonly Dictionary<Vector3, SOverlapThreshold> overlaps = new Dictionary<Vector3, SOverlapThreshold>();
        public readonly List<BaseSocket> overlapping = new List<BaseSocket>();

        public static void DetectOverlap(Vector3 pos, BaseSocket[] targets)
        {
            if (overlaps.ContainsKey(pos)) return;

            /*
            ThreadManager.RunOnUConstructThread(new ThreadTask<Vector3>((Vector3 pos) =>
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    SOverlapThreshold threshold = new SOverlapThreshold();
                    Vector3 pObjectPos;

                    //float distance;
                    BaseSocket instance;

                    for (int i = 0; i < UCPhysics.physicsObjects.Count; i++)
                    {
                        pObjectPos = UCPhysics.physicsObjects[i].stashedPosition;

                        if (pObjectPos == pos)
                        {
                            instance = UCPhysics.physicsObjects[i] as BaseSocket;

                            if (instance != null)
                            {
                                threshold.overlapping.Add(instance);
                            }
                        }
                    }

                    overlaps.Add(pos, threshold);

                    watch.Stop();
                    UnityEngine.Debug.Log(watch.Elapsed + " " + threshold.overlapping.Count);

                }, _pos));
             

            Stopwatch watch = new Stopwatch();
            watch.Start();

            SOverlapThreshold threshold = new SOverlapThreshold();
            Vector3 pObjectPos;

            //float distance;
            BaseSocket instance;

            BaseSocket currentLoopedSocket;
            BaseSocket ownerSocket;

            for (int i = 0; i < UCPhysics.physicsObjects.Count; i++)
            {
                pObjectPos = UCPhysics.physicsObjects[i].stashedPosition;

                if (pObjectPos == pos)
                {
                    instance = UCPhysics.physicsObjects[i] as BaseSocket;

                    if (instance != null)
                    {
                        instance.BuildingSnapped(true);
                        threshold.overlapping.Add(instance);

                        for (int b = 0; b < targets.Length; b++)
                        {
                            ownerSocket = targets[b];

                            //HANDLE OVERLAPPED SOCKETS

                            if(ownerSocket.stashedPosition == instance.building.transform.position)
                            {
                                ownerSocket.BuildingSnapped(false);
                            }

                            for (int c = 0; c < instance.building.sockets.Length; c++)
                            {
                                currentLoopedSocket = instance.building.sockets[c];
                                if (ownerSocket.stashedPosition == currentLoopedSocket.stashedPosition)
                                {
                                    currentLoopedSocket.BuildingSnapped(true);
                                }
                            }
                        }
                    }
                }
            }

            overlaps.Add(pos, threshold);

            watch.Stop();
            UnityEngine.Debug.Log(watch.Elapsed + " " + threshold.overlapping.Count);
            */
        }

        static float fastDistance(Vector3 objA, Vector3 objB)
        {
            tempPosition.x = objA.x - objB.x;
            tempPosition.y = objA.y - objB.y;
            tempPosition.z = objA.z - objB.z;

            return (tempPosition.x * tempPosition.x) + (tempPosition.y * tempPosition.y) + (tempPosition.z * tempPosition.z);
        }

    }
}
