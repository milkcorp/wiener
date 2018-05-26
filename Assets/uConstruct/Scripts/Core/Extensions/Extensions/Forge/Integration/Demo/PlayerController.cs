#if uConstruct_ForgeNetworking

using UnityEngine;
using System.Collections;

using BeardedManStudios.Forge;
using BeardedManStudios.Network;

namespace uConstruct.Extensions.ForgeExtension
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        GameObject playerCamera;

        [SerializeField]
        MonoBehaviour[] enableOnLocalOnly = new MonoBehaviour[0];

        SimpleNetworkedMonoBehavior entity;

        void Start()
        {
            if (entity == null)
                entity = GetComponent<SimpleNetworkedMonoBehavior>();

            foreach (MonoBehaviour component in enableOnLocalOnly)
            {
                component.enabled = false;
            }

            playerCamera.gameObject.SetActive(false);

            if(entity.IsOwner)
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