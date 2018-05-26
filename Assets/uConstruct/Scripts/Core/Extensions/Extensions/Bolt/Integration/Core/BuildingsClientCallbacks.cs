#if uConstruct_PhotonBolt
using UnityEngine;
using System.Collections;

namespace uConstruct.Extensions.BoltExtension
{
    [BoltGlobalBehaviour(BoltNetworkModes.Client)]
    public class BuildingsClientCallbacks : Bolt.GlobalEventListener
    {
        public override void OnEvent(CreateNetworkedBuilding evnt)
        {
            if (BoltNetwork.isServer) return;

            BoltEntitiesManager.LoadEntity(evnt);
        }

        public override void OnEvent(UpdateNetworkedBuilding evnt)
        {
            if (BoltNetwork.isServer) return;

            BoltEntitiesManager.UpdateEntity(evnt);
        }
    }
}
#endif