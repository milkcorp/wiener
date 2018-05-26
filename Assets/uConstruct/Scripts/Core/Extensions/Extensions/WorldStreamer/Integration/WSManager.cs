#if uConstruct_WorldStreamer

using UnityEngine;
using System.Collections;

namespace uConstruct.Extensions.WSExtension
{
    public class WSManager : MonoBehaviour
    {
        WorldMover wm;

        void Awake()
        {
            wm = GameObject.FindObjectOfType<WorldMover>();

            uConstruct.BaseBuildingGroup.OnBuildingGroupCreatedEvent += (uConstruct.BaseBuildingGroup group) =>
            {
                group.gameObject.AddComponent<WorldStreamerGroupHandler>();

                if(wm != null)
                    group.gameObject.AddComponent<ObjectToMove>();
            };

            if (wm != null)
            {
                uConstruct.Core.Saving.BuildingSaveData.OnBuildingSavingEvent += (uConstruct.Core.Saving.BuildingSaveData data) =>
                    {
                        data.pos -= wm.currentMove;
                    };

                BaseBuilding.OnNetworkInstanceLoadedEvent += (BaseBuilding building) =>
                    {
                        building.transform.position += wm.currentMove;
                    };

                BaseBuilding.OnNetworkInstancePackedEvent += (BaseBuilding building) =>
                    {
                        building.transform.position -= wm.currentMove;
                    };
            }
        }
    }
}

#endif
