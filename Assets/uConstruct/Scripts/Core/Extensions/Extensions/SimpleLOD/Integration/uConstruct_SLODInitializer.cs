using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uConstruct;
using uConstruct.Core;

#if uConstruct_SLOD

using OrbCreationExtensions;

public class uConstruct_SLODInitializer : MonoBehaviour 
{
    private float[] compression = new float[5] { 0.25f, 0.5f, 1f, 1.5f, 2f };
    private float smallObjectsValue = 1f;
    private int useValueForNrOfSteps = 1;
    private float useValueForProtectNormals = 1f;
    private float useValueForProtectUvs = 1f;
    private float useValueForProtectBigTriangles = 1f;
    private float useValueForProtectSubMeshesAndSharpEdges = 1f;
    private bool recalcNormals = true;

    void Awake()
    {
        BaseBuildingGroup.OnBatchDoneEvent += OnBatchDone;
    }

    void OnBatchDone(GameObject go, Mesh mesh)
    {
        if(UCSettings.instance.UCBatchingLODLevels == 0) return;

        UCSettings.instance.UCBatchingLODLevels = Mathf.Clamp(UCSettings.instance.UCBatchingLODLevels, 0, 5);

        float[] useCompressions = new float[UCSettings.instance.UCBatchingLODLevels];
        for (int i = 0; i < UCSettings.instance.UCBatchingLODLevels; i++) useCompressions[i] = compression[i];
        try
        {
            StartCoroutine(go.SetUpLODLevelsWithLODSwitcherInBackground(GetDftLodScreenSizes(UCSettings.instance.UCBatchingLODLevels), useCompressions, recalcNormals, smallObjectsValue, useValueForProtectNormals, useValueForProtectUvs, useValueForProtectSubMeshesAndSharpEdges, useValueForProtectBigTriangles));
        }
        catch
        {
            return;
        }
    }

    private float[] GetDftLodScreenSizes(int aNrOflevels)
    {
        switch (aNrOflevels)
        {
            case 1:
                return new float[1] { 0.5f };
            case 2:
                return new float[2] { 0.6f, 0.3f };
            case 3:
                return new float[3] { 0.6f, 0.3f, 0.15f };
            case 4:
                return new float[4] { 0.75f, 0.5f, 0.25f, 0.13f };
            default:
                return new float[5] { 0.8f, 0.6f, 0.4f, 0.2f, 0.1f };
        }
    }

}

#endif