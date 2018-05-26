#if uConstruct_UNet

using UnityEngine;
using System.Collections;

using UnityEngine.Networking;

namespace uConstruct.Extensions.UNetExtension
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        GameObject playerCamera;

        [SerializeField]
        MonoBehaviour[] enableOnLocalOnly = new MonoBehaviour[0];

        NetworkIdentity entity;

        void Start()
        {
            if (entity == null)
                entity = GetComponent<NetworkIdentity>();

            foreach (MonoBehaviour component in enableOnLocalOnly)
            {
                component.enabled = false;
            }

            playerCamera.gameObject.SetActive(false);

            if(entity.hasAuthority)
            {
                InitiateLocalSetup();
            }
        }

        public void InitiateLocalSetup()
        {
            foreach (MonoBehaviour component in enableOnLocalOnly)
            {
                component.enabled = true;
            }

            playerCamera.gameObject.SetActive(true);
        }
    }
}

#endif