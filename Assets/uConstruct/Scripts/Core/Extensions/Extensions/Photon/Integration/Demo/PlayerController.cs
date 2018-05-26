#if uConstruct_PhotonCloud

using UnityEngine;
using System.Collections;

using UnityEngine.Networking;

namespace uConstruct.Extensions.PCloudExtension
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        GameObject playerCamera;

        [SerializeField]
        MonoBehaviour[] enableOnLocalOnly = new MonoBehaviour[0];

        PhotonView entity;

        void Start()
        {
            if (entity == null)
                entity = GetComponent<PhotonView>();

            foreach (MonoBehaviour component in enableOnLocalOnly)
            {
                component.enabled = false;
            }

            playerCamera.gameObject.SetActive(false);

            if(entity.isMine)
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