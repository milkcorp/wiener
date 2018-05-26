using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using uConstruct.Core.Threading;

namespace uConstruct.Core.AOI
{
    /// <summary>
    /// The AOI Class that handles all of the AOI management.
    /// </summary>
    public class AOIManager
    {
        /// <summary>
        /// AOI finders
        /// </summary>
        static List<BaseAOIFinder> finders = new List<BaseAOIFinder>();
        /// <summary>
        /// AOI targets
        /// </summary>
        static List<BaseAOITarget> targets = new List<BaseAOITarget>();

        /// <summary>
        /// This will update position on the finders and targets so the thread can read that.
        /// </summary>
        static void UpdatePositions()
        {
            BaseAOITarget target;
            for (int i = 0; i < targets.Count; i++)
            {
                target = targets[i];
                target.aoiPosition = target.transform.position; // update position...
            }

            BaseAOIFinder finder;
            for (int i = 0; i < finders.Count; i++)
            {
                finder = finders[i];
                finder.aoiPosition = finder.transform.position; // update position...
            }
        }

        /// <summary>
        /// Add a finder to the finder list
        /// </summary>
        /// <param name="value">the finder you want to add</param>
        public static void AddFinder(BaseAOIFinder value)
        {
            if (!finders.Contains(value))
            {
                finders.Add(value);
            }
        }
        /// <summary>
        /// Remove a finder from the finder list
        /// </summary>
        /// <param name="value">the finder you want to remove</param>
        public static void RemoveFinder(BaseAOIFinder value)
        {
            if (finders.Contains(value))
            {
                finders.Remove(value);
            }
        }

        /// <summary>
        /// Add a target to the target list
        /// </summary>
        /// <param name="value">the target you want to add</param>
        public static void AddTarget(BaseAOITarget value)
        {
            if (!targets.Contains(value))
            {
                value.aoiPosition = value.transform.position;
                targets.Add(value);
            }
        }
        /// <summary>
        /// Remove a target from the target list
        /// </summary>
        /// <param name="value">the target you want to remove</param>
        public static void RemoveTarget(BaseAOITarget value)
        {
            if (targets.Contains(value))
            {
                targets.Remove(value);
            }
        }
        /// <summary>
        /// Update the AOI of the finder.
        /// </summary>
        /// <param name="finder">the finder you want to update the AOI zone of.</param>
        public static void UpdateAOI(BaseAOIFinder finder)
        {
            ThreadTask<BaseAOIFinder> task = new ThreadTask<BaseAOIFinder>(ComputeAOI, finder);

            UpdatePositions();

            #if !UNITY_WEBGL
            ThreadManager.RunOnUConstructThread(task);
            #else
            ThreadManager.RunOnUnityThread(task);
            #endif
        }

        /// <summary>
        /// Compute AOI for a certain finder
        /// </summary>
        /// <param name="finder">the finder you want to compute AOI for.</param>
        static void ComputeAOI(BaseAOIFinder finder)
        {
            BaseAOITarget target;

            for (int i = 0; i < targets.Count; i++)
            {
                target = targets[i];

                if (target.useMultiThreadZoneSearch)
                {
                    HandleAOI(target, finder, false);
                }
                else
                {
                    ThreadManager.RunOnUnityThread(new ThreadTask<BaseAOITarget, BaseAOIFinder>((BaseAOITarget _target, BaseAOIFinder _finder) =>
                    {
                        HandleAOI(_target, _finder, true);
                    }, target, finder));
                }
            }
        }

        /// <summary>
        /// Handle the AOI.
        /// </summary>
        /// <param name="target">our target</param>
        /// <param name="finder">our finder</param>
        /// <param name="isUnityThread">is this executed from unity's thread.</param>
        private static void HandleAOI(BaseAOITarget target, BaseAOIFinder finder, bool isUnityThread)
        {
            bool inDistance = target.InZone(finder.aoiPosition, finder.radius);

            if (!target.calculateDuplicates || target.inRange != inDistance) // if we arent duplicating
            {
                if (isUnityThread)
                {
                    target.HandleAOI(finder, inDistance);
                }
                else
                {
                    ThreadManager.RunOnUnityThread(new ThreadTask<BaseAOITarget, BaseAOIFinder, bool>((BaseAOITarget _target, BaseAOIFinder _finder, bool _inDistance) =>
                    {
                        _target.HandleAOI(_finder, _inDistance);
                    }, target, finder, inDistance));
                }
            }
        }
    }
}

