using UnityEngine;
using System.Collections;

#if UCONSTRUCT_PRESET && !UC_Free

namespace uConstruct.Extensions
{
    public class BoltIntegration : Extension
    {
        public override string AssetName
        {
            get
            {
                return "Photon Bolt";
            }
        }

        public override string AssetDescription
        {
            get
            {
                return "Build multiplayer games in Unity without having to \nknow the details of networking or write\nany complex networking code.";
            }
        }

        public override string AssetLogoName
        {
            get
            {
                return "Bolt_uConstruct_Extension";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "uConstruct_PhotonBolt";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "Exit Games";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/41330";
            }
        }

        public override string AssetDocumentationName
        {
            get
            {
                return "uConstruct_PhotonBolt_Integration.docx.pdf";
            }
        }

    }
}
#endif