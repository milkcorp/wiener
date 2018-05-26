#if uConstruct_PhotonCloud

using UnityEngine;
using System.Collections;

namespace uConstruct.Extensions.PCloudExtension
{
    public class PlayerInstantiater : Photon.PunBehaviour
    {
        public override void OnJoinedRoom()
        {
            PhotonNetwork.Instantiate("PCloudPlayers/PhotonCloudPlayer", Vector3.zero, Quaternion.identity, 0);
        }
    }
}

#endif