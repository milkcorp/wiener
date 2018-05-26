#if uConstruct_UNet

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace uConstruct.Extensions.UNetExtension
{
    public class UNet_StartGame : MonoBehaviour
    {
        public void StartServer()
        {
            NetworkManager.singleton.StartServer();
        }
        public void StartClient()
        {
            NetworkManager.singleton.StartClient();
        }
    }
}

#endif
