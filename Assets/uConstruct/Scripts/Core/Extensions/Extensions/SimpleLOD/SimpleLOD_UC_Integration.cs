using UnityEngine;
using System.Collections;

#if UCONSTRUCT_PRESET && !UC_Free

namespace uConstruct.Extensions
{
    public class SimpleLOD_UC_Integration : Extension
    {
        public override string AssetName
        {
            get
            {
                return "SimpleLOD";
            }
        }

        public override string AssetDescription
        {
            get
            {
                return "Recognize this problem? \nYou downloaded a model. It looks great, but \nis way too heavy for it's purpose. \n\nSimpleLOD fixes this problem in a very easy \nway: By merging skinned and non-skinned \nmeshes, by baking atlases and by generating \nthe LOD meshes for you. Your GameObject is \noperational with just a few mouse clicks.";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "Orbcreation";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/25366";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "uConstruct_SLOD";
            }
        }

        public override string AssetLogoName
        {
            get
            {
                return "SLOD_UC_Logo";
            }
        }

        [MethodHelper()]
        public void CreateInitializerInScene()
        {
#if uConstruct_SLOD
            var initializer = GameObject.FindObjectOfType<uConstruct_SLODInitializer>();

            if(initializer == null)
            {
                GameObject go = new GameObject("uConstruct SimpleLOD Initializer");
                go.AddComponent<uConstruct_SLODInitializer>();
            }
#endif
        }
    }
}

#endif