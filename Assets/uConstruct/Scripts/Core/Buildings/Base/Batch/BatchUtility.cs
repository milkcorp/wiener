using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace uConstruct
{
    /// <summary>
    /// This class handles the batching mechanic of uConstruct. Can be used with other system as well if needed.
    /// </summary>
    public class BatchUtility
    {
        /// <summary>
        /// Return combined batched meshes (support submeshes).
        /// </summary>
        /// <param name="batch">What MeshFilters to batch</param>
        /// <returns>the batched meshes</returns>
        public static BatchData CompileInitialBatchData(MeshFilter[] batch, bool value)
        {
            BatchData batchData = new BatchData();
            CombineInstance combineInstance = new CombineInstance();
            MeshFilter current;

            BatchClass batchInstance;

            Material[] materials;

            Vector3 tempPos;

            for (int i = 0; i < batch.Length; i++)
            {
                current = batch[i];
                materials = null;

                if(current != null)
                {
                    tempPos = current.transform.position; // assign this to avoid world space (using this method cause -> worldToLocal changes size, we dont want that.)

                    current.transform.position = current.transform.position - current.transform.root.position;

                    combineInstance.transform = current.transform.localToWorldMatrix;
                    combineInstance.mesh = current.mesh;

                    current.transform.position = tempPos;

                    materials = HandleRenders(current, value);

                    batchInstance = batchData.Batchable(materials, current.mesh.vertexCount);
                    if (batchInstance != null)
                    {
                        batchInstance.AddFilter(current, combineInstance);
                    }
                    else
                    {
                        batchData.Add(new BatchClass(materials)).AddFilter(current, combineInstance);
                    }
                }
            }

            return batchData;
        }

        /// <summary>
        /// Update our batch data
        /// </summary>
        /// <param name="filters">what filters you want to add / remove</param>
        /// <param name="Add">Are we adding an instance ? or removing it ?</param>
        /// <param name="batchData">returns the edited batch data</param>
        public static void UpdateBatchData(MeshFilter[] filters, bool Add, ref BatchData batchData)
        {
            BatchClass currentBatch;
            MeshFilter currentFilter;

            if (!Add)
            {
                for (int i = 0; i < filters.Length; i++)
                {
                    currentFilter = filters[i];

                    for (int b = 0; b < batchData.Count; b++)
                    {
                        currentBatch = batchData[b];

                        for (int c = 0; c < currentBatch.Filters.Count; c++)
                        {
                            if (currentBatch.Filters[c] == currentFilter)
                            {
                                currentBatch.RemoveFilter(c);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                MeshRenderer currentRenderer;
                CombineInstance currentCombineMesh = new CombineInstance();
                Vector3 tempPos;

                for(int i = 0; i < filters.Length; i++)
                {
                    if (filters[i] == null) continue;

                    currentFilter = filters[i];
                    currentRenderer = currentFilter.GetComponent<MeshRenderer>();
                    
                    if(currentRenderer != null && currentFilter != null)
                    {
                        currentBatch = batchData.Batchable(currentRenderer.materials, currentFilter.mesh.vertexCount);

                        HandleRenders(currentFilter, Add);

                        tempPos = currentFilter.transform.position; // assign this to avoid world space (using this method cause -> worldToLocal changes size, we dont want that.)

                        currentFilter.transform.position = currentFilter.transform.position - currentFilter.transform.root.position;

                        currentCombineMesh.transform = currentFilter.transform.localToWorldMatrix;
                        currentCombineMesh.mesh = currentFilter.mesh;

                        currentFilter.transform.position = tempPos;

                        if (currentBatch != null)
                        {
                            currentBatch.AddFilter(currentFilter, currentCombineMesh);
                        }
                        else
                        {
                            batchData.Add(new BatchClass(currentRenderer.materials)).AddFilter(currentFilter, currentCombineMesh);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handle the renders of the filter
        /// </summary>
        /// <param name="filter">the filter</param>
        /// <param name="value">enable or disable ?</param>
        /// <returns>the renderer material data</returns>
        static Material[] HandleRenders(MeshFilter filter, bool value)
        {
            MeshRenderer renderer;
            Material[] materials = null;

            renderer = filter.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = !value;
                materials = renderer.materials;
            }

            return materials;
        }

        /// <summary>
        /// Are we over the vertex limit ?
        /// </summary>
        /// <param name="amount">our current vertex count</param>
        /// <returns></returns>
        public static bool isVertexOverLimit(int amount)
        {
            return amount >= 64000; // vertex limit.
        }
    }

    /// <summary>
    /// Extension class for the mesh class.
    /// </summary>
    public static class BatchExtensions
    {
        public static bool IsMeshFull(this Mesh mesh)
        {
            return mesh.vertexCount >= 64000;
        }
    }

    /// <summary>
    /// Our batch data
    /// </summary>
    public class BatchData
    {
        List<BatchClass> renderesData = new List<BatchClass>();

        public BatchClass this[int index]
        {
            get { return renderesData[index]; }
        }
        public int Count
        {
            get { return renderesData.Count; }
        }
        /// <summary>
        /// Can we get any more batches? and is the data exists?
        /// </summary>
        /// <param name="data">our materials data</param>
        /// <param name="shapeVertexCount">our current shape vertex count</param>
        /// <returns>instance of the batch data</returns>
        public BatchClass Batchable(Material[] data, int shapeVertexCount)
        {
            BatchClass current;

            for (int i = 0; i < renderesData.Count; i++)
            {
                current = renderesData[i];

                if (current.Containes(data) && !BatchUtility.isVertexOverLimit(current.totalVertexAmount + shapeVertexCount))
                {
                    return current;
                }
            }

            return null;
        }
        /// <summary>
        /// Add a building to the data
        /// </summary>
        /// <param name="value">what building to add</param>
        /// <returns>instance of the building</returns>
        public BatchClass Add(BatchClass value)
        {
            renderesData.Add(value);

            return renderesData[renderesData.Count - 1];
        }
        /// <summary>
        /// Remove a building from the data
        /// </summary>
        /// <param name="value">the instance to remove.</param>
        public void Remove(BatchClass value)
        {
            if(renderesData.Contains(value))
            {
                renderesData.Remove(value);
            }
        }
    }


}
