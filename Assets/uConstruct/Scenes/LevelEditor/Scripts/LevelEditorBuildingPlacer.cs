using UnityEngine;
using System.Collections;

using uConstruct;

namespace uConstruct.Demo
{
    public class LevelEditorBuildingPlacer : BuildingPlacer
    {
        [SerializeField]
        LevelEditorController controller;

        public override void PlaceBuilding()
        {
            base.PlaceBuilding();
            controller.ReadNewLength();
        }

        public override void DestroyBuilding(BaseBuilding building, RaycastHit hit)
        {
            base.DestroyBuilding(building, hit);
            controller.ReadNewLength();
        }

    }
}