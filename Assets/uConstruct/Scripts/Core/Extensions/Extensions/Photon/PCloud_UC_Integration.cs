using UnityEngine;
using System.Collections;

#if uConstruct_PhotonCloud
using uConstruct.Extensions.PCloudExtension;
#endif

#if UCONSTRUCT_PRESET && !UC_Free

namespace uConstruct.Extensions
{
    public class PCloud_UC_Integration : Extension
    {
        public override string AssetName
        {
            get
            {
                return "Photon Cloud";
            }
        }
        public override string AssetNameSpace
        {
            get
            {
                return "uConstruct_PhotonCloud";
            }
        }
        public override string PublisherName
        {
            get
            {
                return "Exit Games";
            }
        }
        public override string AssetDocumentationName
        {
            get
            {
                return "uConstruct_PCloud_Integration.docx.pdf";
            }
        }
        public override string AssetDescription
        {
            get
            {
                return "The ease-of-use of Unity's Networking plus \nthe performance and reliability of the \nPhoton Cloud.";
            }
        }
        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/1786";
            }
        }
        public override string AssetLogoName
        {
            get
            {
                return "PCloud_UC_Logo";
            }
        }

        [MethodHelper]
        public void CreateManager()
        {
#if uConstruct_PhotonCloud
            PhotonCloudEntitiesManager manager = GameObject.FindObjectOfType<PhotonCloudEntitiesManager>();

            if(manager == null)
            {
                GameObject go = new GameObject("Photon Cloud Manager");
                go.AddComponent<PhotonCloudEntitiesManager>();
            }
#endif
        }
    }
}

#endif