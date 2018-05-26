using UnityEngine;
using System.Collections;

#if UCONSTRUCT_PRESET && !UC_Free

#if uConstruct_ForgeNetworking
using uConstruct.Extensions.ForgeExtension;
#endif

namespace uConstruct.Extensions
{
    public class ForgeNetworking_UC_Integration : Extension
    {
        public override string AssetName
        {
            get
            {
                return "Forge Networking";
            }
        }

        public override string AssetDescription
        {
            get
            {
                return "Ever wanted to make a multiplayer game? \nYou've never seen a networking library quite \nlike this before! Find out why so many people \nare leaving the other network solutions to \nfinally be free to build the multiplayer games \nthey want without limitations! Come join the \ncommunity for the fastest growing \nnetworking solution on the Asset Store!";
            }
        }

        public override string AssetLogoName
        {
            get
            {
                return "ForgeNetworking_UC_Logo";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "uConstruct_ForgeNetworking";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "Bearded Man Studios";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/38344";
            }
        }

        public override string AssetDocumentationName
        {
            get
            {
                return "uConstruct_ForgeNetworking_Integration.docx.pdf";
            }
        }

        [MethodHelper]
        public void CreateNetworkManagerInScene()
        {
#if uConstruct_ForgeNetworking
            ForgeEntitiesManager manager = GameObject.FindObjectOfType<ForgeEntitiesManager>();

            if(manager == null)
            {
                GameObject go = new GameObject("uConstruct Forge Manager");
                manager = go.AddComponent<ForgeEntitiesManager>();

                manager.serverIsAuthority = true;
                manager.interpolateFloatingValues = false;
                manager.lerpPosition = false;
                manager.lerpRotation = false;
                manager.lerpScale = false;
                manager.teleportToInitialPositions = false;
                manager.isReliable = true;
            }
#endif
        }
    }
}

#endif
