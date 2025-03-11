using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// This script is on the GrassController prefab.
/// When the GrassController prefab is a child of a game object it draws a mesh with the assigned grass material at each vertex of the parent gameobject using GPU Instancing. 
/// </summary>
namespace MCTerrain
{
    public class GrassController : MonoBehaviour
    {

        #region Private Variables

        private TerrainManager _terrainManager;
        private Matrix4x4[] _matrices;
        private Chunk _chunk;

        private Mesh _parentMesh;
        private readonly List<Vector3> _objectPositions = new List<Vector3>();

        #endregion

        #region Public Variables

        // The mesh to be drawn at each vertice of the parent mesh.
        public Mesh Mesh;

        // Material to use for drawing the meshes.
        public Material Material;

        [Header("Grass size scale values")]
        [Range(1f, 5f)]
        public float ScaleMinimum = 2f;
        [Range(1f, 5f)]
        public float ScaleMaximum = 3f;

        [Header("Grass position offset amount")]
        [Range(0f, 0.5f)]
        public float PositionOffset = 0.2f;

        #endregion

        private void Start()
        {
            _terrainManager = TerrainManager.Instance;

            InitialiseGrassPositions();
            BuildGrassMatrix();
        }

        /// <summary>
        /// Sends the data to the GPU.
        /// </summary>
        private void Update()
        {
            Graphics.DrawMeshInstanced(Mesh, 0, Material, _matrices);
        }


        #region Private Methods

        /// <summary>
        /// Finds the parent game object's mesh and populates a Dictionary with the vertex postions and sets them to true if they face upwards.
        /// Grass is only drawn at vertex positions that are set to true, so it prevents grass from appearing on cliff walls and ceilings.
        /// </summary>
        private void InitialiseGrassPositions()
        {
            GameObject parent = this.transform.parent.gameObject;
            MeshFilter parentMeshFilter = (MeshFilter)parent.GetComponent("MeshFilter");
            _parentMesh = parentMeshFilter.mesh;

            _chunk = _terrainManager.GetChunk(parent.transform.position);

            // If the parent game object has a mesh attached
            if (_parentMesh)
            {
                for (int i = 0; i < _parentMesh.vertexCount; i++)
                {
                    // Find the local vertex position
                    Vector3 posLocal = _parentMesh.vertices[i];
                    Vector3 normal = _parentMesh.normals[i];

                    // Find the workd space vertex position
                    Vector3 pos = parent.transform.TransformPoint(_parentMesh.vertices[i]);

                    if (normal.y > 0.85f &&
                        (!_chunk.NeedsWaterTile || (_chunk.NeedsWaterTile && pos.y > _terrainManager.WaterLevel)))
                    {
                        // Don't place grass on the left and bottom edge as they are the same as the top and right of the neighbouring chunks.
                        // This stops the grass from being too dense along the edges of the chunks.
                        if (posLocal.x > 0 && posLocal.z > 0)
                        {
                            Vector3 posRandomised = new Vector3(pos.x + Random.Range(-PositionOffset, PositionOffset), pos.y - 0.2f, pos.z + Random.Range(-PositionOffset, PositionOffset));

                            _objectPositions.Add(posRandomised);
                            _chunk.GrassPositions.Add(posRandomised, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This creates a new matrix4x4 from the dictionary of vertex positions that are set to true.
        /// </summary>
        private void BuildGrassMatrix()
        {

            int temp = 0;

            _matrices = new Matrix4x4[_objectPositions.Count];

            foreach (var grassPosition in _chunk.GrassPositions)
            {
                if (grassPosition.Value == true)
                {

                    // Build matrix.
                    Matrix4x4 mat = Matrix4x4.identity;

                    Quaternion rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
                    Vector3 scale = Vector3.one * Random.Range(ScaleMinimum, ScaleMaximum);

                    mat.SetTRS(grassPosition.Key, rotation, scale);

                    _matrices[temp] = mat;

                    temp++;

                }

                // Graphics.DrawMeshInstanced has an object matrix size limit of 1023 so we must stay under that limit.
                if (temp > 1023)
                {
                    break;
                }

            }

        }

        #endregion

    }
}