using UnityEngine;
using System.Collections;

#if UCONSTRUCT_PRESET && !UC_Free

namespace uConstruct.Extensions
{
    public class UniStorm_UC_Integration : Extension
    {
        public override string AssetName
        {
            get
            {
                return "UniStorm";
            }
        }

        public override string AssetDescription
        {
            get
            {
                return "UniStorm is an incredibly powerful dynamic \nday and night weather system that creates \nAAA quality dynamically generated weather, \nlighting, and skies all at a blazing fast frame \nrate. UniStorm features over 250 \ncustomizable components allowing users to \ncreate any environment imaginable. ";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "Black Horizon Studios";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/2714";
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
                return "UniStorm_UC_Logo";
            }
        }

    }
}

#endif