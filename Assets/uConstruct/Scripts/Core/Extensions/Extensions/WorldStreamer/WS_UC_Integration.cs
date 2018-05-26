using UnityEngine;
using System.Collections;

#if UCONSTRUCT_PRESET && !UC_Free

namespace uConstruct.Extensions
{
    public class WS_UC_Integration : Extension
    {
        public override string AssetName
        {
            get
            {
                return "World Streamer";
            }
        }

        public override string AssetDescription
        {
            get
            {
                return "World Streamer is a memory streaming \nsystem. By using it you are able to stream \nwhole your game from a disc in any axis and \nspace.";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "NatureManufacture";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/36486";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "uConstruct_WorldStreamer";
            }
        }

        public override string AssetLogoName
        {
            get
            {
                return "WorldStreamer_UC_Logo";
            }
        }

        public override string AssetDocumentationName
        {
            get
            {
                return "uConstruct_WorldStreamer_Integration.docx.pdf";
            }
        }

        [MethodHelper()]
        public void CreateInitializerInScene()
        {
#if uConstruct_WorldStreamer
            var initializer = GameObject.FindObjectOfType<uConstruct.Extensions.WSExtension.WSManager>();

            if (initializer == null)
            {
                GameObject go = new GameObject("uConstruct WorldStreamer Initializer");
                go.AddComponent<uConstruct.Extensions.WSExtension.WSManager>();
            }
#endif
        }
    }
}

#endif