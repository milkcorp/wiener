using UnityEngine;
using System.Collections;

#if UCONSTRUCT_PRESET && !UC_Free

namespace uConstruct.Extensions
{
    public class InventoryPro_UC_Integration : Extension
    {
        public override string AssetName
        {
            get { return "Inventory Pro"; }
        }

        public override string AssetDescription
        {
            get
            {
                return "Inventory Pro is a highly flexible and easy to \nuse inventory, that can be used for all game \ntypes. "; 
            }
        }

        public override string AssetDocumentationName
        {
            get
            {
                return "uConstruct_InventoryPRO_Integration.docx.pdf";
            }
        }

        public override string AssetLogoName
        {
            get
            {
                return "InventoryPro_UC_Logo";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "uConstruct_InventoryPRO";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/31226";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "Devdog";
            }
        }

    }
}

#endif
