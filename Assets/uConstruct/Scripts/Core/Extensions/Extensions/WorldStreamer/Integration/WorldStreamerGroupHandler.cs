#if uConstruct_WorldStreamer

using UnityEngine;
using System.Collections;
using uConstruct.Core.Saving;
using uConstruct.Core.AOI;

namespace uConstruct.Extensions.WSExtension
{
    public class WorldStreamerGroupHandler : BaseAOITarget
    {
        WorldMover wm;
        BaseBuildingGroup bg;

        public override bool useMultiThreadZoneSearch
        {
            get
            {
                return false;
            }
        }

        public Vector3 realPos
        {
            get
            {
                return wm == null ? aoiPosition : wm.currentMove + aoiPosition;
            }
        }

        void Start()
        {
            wm = GameObject.FindObjectOfType<WorldMover>();
            bg = GetComponent<BaseBuildingGroup>();

            if(wm != null)
                transform.position -= wm.currentMove;
        }

        void Enable(BaseBuilding target, bool value)
        {
            target.gameObject.SetActive(value);
            target.batchBuilding = value;
        }

        public override void HandleAOI(BaseAOIFinder finder, bool _inRange)
        {
        }

        public override bool InZone(Vector3 finderPos, float radius)
        {
            BaseBuilding building;
            Vector3 buildingPos;

            finderPos += (wm == null ? Vector3.zero : wm.currentMove);

            for (int i = 0; i < bg.buildings.Count; i++)
            {
                building = bg.buildings[i];

                if (building != null)
                {
                    buildingPos = building.transform.position + (wm == null ? Vector3.zero : wm.currentMove);

                    Enable(building, Vector3.Distance(buildingPos, finderPos) < 60);
                }
            }

            return false;
        }
    }
}
#endif