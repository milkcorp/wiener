using UnityEngine;
using System.Collections;

#if UCONSTRUCT_PRESET && !UC_Free

using uConstruct;

#if uConstruct_ICEExtension
using ICE.Creatures;
#endif

namespace uConstruct.Extensions
{
    public class ICE_UC_Integration : Extension
    {
        public override string AssetName
        {
            get
            {
                return "ICE Creature Control ( WIP )";
            }
        }

        public override string AssetDescription
        {
            get
            {
                return "ICECreatureControl is an incredible piece of \nsoftware to breathe life into your virtual \nCharacters - within minutes and without \ntyping one single line of code!";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "ICE";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/35364";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "uConstruct_ICEExtension";
            }
        }

        public override string AssetLogoName
        {
            get
            {
                return "ICE_UC_Logo";
            }
        }

        public override string AssetDocumentationName
        {
            get
            {
                return "";
            }
        }
    
        [MethodHelper()]
        public void TriggerBuildingsAvoidanceOnScene()
        {
#if uConstruct_ICEExtension

            ICECreatureRegister register = GameObject.FindObjectOfType<ICECreatureRegister>();

            if(register == null)
            {
                Debug.LogError("ICECreatureRegister can not be found!");
                return;
            }

            if(!register.ObstacleLayers.Contains(LayersData.BuildingLayers[LayersData.instance.defaultBuildingLayer]))
            {
                register.ObstacleLayers.Add(LayersData.BuildingLayers[LayersData.instance.defaultBuildingLayer]);
                Debug.Log("Added default building layer to obstacles");
            }
            else
            {
                register.ObstacleLayers.Remove(LayersData.BuildingLayers[LayersData.instance.defaultBuildingLayer]);
                Debug.Log("Removed default building layer from obstacles");
            }

#endif
        }

    }
}

#endif