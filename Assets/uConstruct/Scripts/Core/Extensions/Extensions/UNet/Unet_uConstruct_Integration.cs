using UnityEngine;
using System.Collections;

#if uConstruct_UNet
using uConstruct.Extensions.UNetExtension;
#endif

#if UCONSTRUCT_PRESET && !UC_Free

namespace uConstruct.Extensions
{
    public class Unet_uConstruct_Integration : Extension
    {
        public override string AssetName
        {
            get
            {
                return "UNet";
            }
        }
        public override string AssetNameSpace
        {
            get
            {
                return "uConstruct_UNet";
            }
        }
        public override string PublisherName
        {
            get
            {
                return "Unity Technologies";
            }
        }
        public override string AssetDocumentationName
        {
            get
            {
                return "uConstruct_UNet_Integration.docx.pdf";
            }
        }

        [MethodHelper]
        public void CreateInitializerOnMenuScene()
        {
#if uConstruct_UNet
            UNetEntitiesManager manager = GameObject.FindObjectOfType<UNetEntitiesManager>();

            if(manager == null)
            {
                GameObject go = new GameObject("UNet Manager");
                go.AddComponent<UNetEntitiesManager>();
            }
#endif
        }
    }
}

#endif