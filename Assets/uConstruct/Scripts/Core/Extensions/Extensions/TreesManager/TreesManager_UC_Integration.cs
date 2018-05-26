using UnityEngine;
using System.Collections;

#if UCONSTRUCT_PRESET && !UC_Free

namespace uConstruct.Extensions
{
    public class TreesManager_UC_Integration : Extension
    {
        public override string AssetName
        {
            get
            {
                return "Trees Manager System";
            }
        }

        public override string AssetDescription
        {
            get
            {
                return "TreesManagerSystem is a strong system that \nwill help you make your terrain trees \ninteractable.";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "EE Productions";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/43129";
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
                return "TMS_UC_Logo";
            }
        }

    }
}

#endif