using UnityEngine;
using System.Collections;

#if UCONSTRUCT_PRESET && !UC_Free

namespace uConstruct.Extensions
{
    public class Exterior_UC_Integration : Extension
    {
        public override string AssetName
        {
            get
            {
                return "Village Exteriors Kit";
            }
        }

        public override string AssetDescription
        {
            get
            {
                return "This modular kit is what you have been \nlooking for to construct all the Medieval \nFantasy Village & Town buildings you would \never need.";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "3DForge";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/38045";
            }
        }

        public override bool IsDefault
        {
            get
            {
                return true;
            }
        }

        public override string AssetLogoName
        {
            get
            {
                return "Exterior_UC_Logo";
            }
        }

        public override string AssetDocumentationName
        {
            get
            {
                return "uConstruct_3DForge_Integration.pdf";
            }
        }
    }
}

#endif