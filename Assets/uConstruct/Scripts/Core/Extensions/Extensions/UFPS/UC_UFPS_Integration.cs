using UnityEngine;
using System.Collections;

#if UCONSTRUCT_PRESET && !UC_Free

namespace uConstruct.Extensions
{
    public class UC_UFPS_Integration : Extension
    {
        public override string AssetName
        {
            get { return "UFPS"; }
        }

        public override string AssetDescription
        {
            get
            {
                return "UFPS is a professional FPS base platform for \nUnity. One of the longest-running and most \npopular titles of the Unity Asset Store, it’s \nknown for smooth controls and fluid, \nrealtime-generated camera and weapon \nmotion. Since 2012 it has been steadily \nexpanded, supported and refactored with a \nfocus on robust, generic FPS features.";
            }
        }

        public override string AssetDocumentationName
        {
            get
            {
                return "‏‏‏‏‏‏‏‏uConstruct_UFPS_Integration.pdf";
            }
        }

        public override string AssetLogoName
        {
            get
            {
                return "UFPS_Logo";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "uConstruct_UFPS";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/2943";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "Opsive";
            }
        }

    }
}

#endif
