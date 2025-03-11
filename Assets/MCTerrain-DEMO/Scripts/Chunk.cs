using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace MCTerrain
{

    /// <summary>
    /// Holds all infomation for a chunk, and the methods to populate the terrain data and to march the cubes.
    /// </summary>
    public class Chunk
    {
        #region Public Properties

        private readonly GameObject _chunkGameObject;
        public GameObject ChunkGameObject { get { return _chunkGameObject; } }

        private Vector3Int _position;
        public Vector3Int Position { get { return _position; } }

        private readonly float[,,] _terrainMap;
        public float[,,] TerrainMap { get { return _terrainMap; } }

        private bool _needsWaterTile = false;
        public bool NeedsWaterTile { get { return _needsWaterTile; } set { _needsWaterTile = value; } }

        private bool _needsFloorTile = false;
        public bool NeedsFloorTile { get { return _needsFloorTile; } set { _needsFloorTile = value; } }

        // Chunk neighbours - these are used to calculate adding and removing terrain when the altered area crosses a chunk border.
        private readonly Vector3Int[] _neighbour = new Vector3Int[8];
        public Vector3Int NeighbourLeft { get { return _neighbour[(int)Enums.Direction.Left]; } }
        public Vector3Int NeighbourRight { get { return _neighbour[(int)Enums.Direction.Right]; } }
        public Vector3Int NeighbourTop { get { return _neighbour[(int)Enums.Direction.Top]; } }
        public Vector3Int NeighbourBottom { get { return _neighbour[(int)Enums.Direction.Bottom]; } }
        public Vector3Int NeighbourTopLeft { get { return _neighbour[(int)Enums.Direction.TopLeft]; } }
        public Vector3Int NeighbourBottomLeft { get { return _neighbour[(int)Enums.Direction.BottomLeft]; } }
        public Vector3Int NeighbourTopRight { get { return _neighbour[(int)Enums.Direction.TopRight]; } }
        public Vector3Int NeighbourBottomRight { get { return _neighbour[(int)Enums.Direction.BottomRight]; } }

        private bool _dataReady;
        public bool DataReady { get { return _dataReady; } set { _dataReady = value; } }

        private bool _hasMesh;
        public bool HasMesh { get { return _hasMesh; } }

        private bool _sceneryAdded;
        public bool SceneryAdded { get { return _sceneryAdded; } set { _sceneryAdded = value; } }

        // Scenery - the chunk holds the positions of its scenery objects. The values are populated by the Scenery Manager.
        public List<Vector2Int> TreePositions { get; set; }
        public List<Vector2Int> BushPositions { get; set; }
        public List<Vector2Int> RockPositions { get; set; }
        public List<Vector2Int> StonePositions { get; set; }
        public Dictionary<Vector3, bool> GrassPositions { get { return _grassPositions; } set { _grassPositions = value; } }
        #endregion

        #region Private Variables

        private readonly int _width = GameData.ChunkWidth;
        private readonly int _height = GameData.ChunkHeight;

        private readonly float[,] _baseTerrainHeight;

        private readonly MeshFilter _meshFilter;
        private readonly MeshCollider _meshCollider;
        private readonly MeshRenderer _meshRenderer;

        private float[,,] _densityMap;

        private readonly Dictionary<Vector3, int> _vertices;
        private readonly List<int> _triangles;

        private readonly TerrainManager _terrainManager = TerrainManager.Instance;

        private Dictionary<Vector3, bool> _grassPositions;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for the Chunk class.
        /// </summary>
        /// <param name="position">The world coordinate position of the chunk.</param>
        public Chunk(Vector3Int position)
        {

            _chunkGameObject = new GameObject
            {
                name = string.Format("Chunk {0}, {1}", position.x, position.z)
            };
            _position = position;
            _chunkGameObject.transform.position = _position;

            _neighbour[(int)Enums.Direction.Top] = _position + new Vector3Int(0, 0, 16);
            _neighbour[(int)Enums.Direction.TopRight] = _position + new Vector3Int(16, 0, 16);
            _neighbour[(int)Enums.Direction.Right] = _position + new Vector3Int(16, 0, 0);
            _neighbour[(int)Enums.Direction.BottomRight] = _position + new Vector3Int(16, 0, -16);
            _neighbour[(int)Enums.Direction.Bottom] = _position + new Vector3Int(0, 0, -16);
            _neighbour[(int)Enums.Direction.BottomLeft] = _position + new Vector3Int(-16, 0, -16);
            _neighbour[(int)Enums.Direction.Left] = _position + new Vector3Int(-16, 0, 0);
            _neighbour[(int)Enums.Direction.TopLeft] = _position + new Vector3Int(-16, 0, 16);

            _meshFilter = _chunkGameObject.AddComponent<MeshFilter>();
            _meshCollider = _chunkGameObject.AddComponent<MeshCollider>();
            _meshRenderer = _chunkGameObject.AddComponent<MeshRenderer>();
            _chunkGameObject.transform.tag = "Terrain";

            _baseTerrainHeight = new float[_width + 1, _width + 1];
            PopulateBaseTerrainHeightArray();

            _meshRenderer.material = _terrainManager.Material;

            // The data points for terrain are stored at the corners of our "cubes", so the terrainMap needs to be 1 larger
            // than the width/height of our mesh.
            _terrainMap = new float[_width + 1, _height + 1, _width + 1];
            _dataReady = false;
            _sceneryAdded = false;
            _hasMesh = false;

            _vertices = new Dictionary<Vector3, int>();
            _triangles = new List<int>();
            _grassPositions = new Dictionary<Vector3, bool>();

        }
        #endregion

        #region Public Methods

        /// <summary>
        /// This calls two methods:
        ///     MarchCubes();
        ///     BuildMesh();
        ///     
        /// It creates new mesh data for the chunk after it has been modified by the player.
        /// </summary>
        public void CreateMeshData()
        {
            MarchCubes();
            BuildMesh();
        }

        /// <summary>
        /// This clears any existing mesh data for the chunk so a new mesh can be created.
        /// Calls:
        /// _vertices.Clear();
        /// _triangles.Clear();
        /// </summary>
        public void ClearMeshData()
        {
            _vertices.Clear();
            _triangles.Clear();
        }

        /// <summary>
        /// Builds the new mesh from _verticies and _triangles that were populated by MarchCubes().
        /// </summary>
        public void BuildMesh()
        {

            Mesh mesh = new();
            Vector3[] verts = new Vector3[_vertices.Count];
            _vertices.Keys.CopyTo(verts, 0);
            mesh.SetVertices(verts);
            mesh.triangles = _triangles.ToArray();
            mesh.RecalculateNormals();
            _meshFilter.mesh = mesh;
            _meshCollider.sharedMesh = mesh;
            _hasMesh = true;

        }

        /// <summary>
        /// Sets the _terrainMap[,,] position to 0f which indicates it is below the ground.
        /// This has the effect of raising the terrain.
        /// </summary>
        /// <param name="pos">The position to update.</param>
        public void PlaceTerrain(Vector3Int pos)
        {

            pos -= _position;

            if (pos.x < 0) pos.x = 0;
            if (pos.z < 0) pos.z = 0;
            if (pos.x > 16) pos.x = 16;
            if (pos.z > 16) pos.x = 16;

            _terrainMap[pos.x, pos.y, pos.z] = 0f;

        }

        /// <summary>
        /// Sets the _terrainMap[,,] position to 1f which indicates it is above the ground.
        /// This has the effect of lowering the terrain.
        /// </summary>
        /// <param name="pos">The position to update.</param>
        public void RemoveTerrain(Vector3Int pos)
        {

            pos -= _position;

            if (pos.x < 0) pos.x = 0;
            if (pos.z < 0) pos.z = 0;
            if (pos.x > 16) pos.x = 16;
            if (pos.z > 16) pos.x = 16;

            _terrainMap[pos.x, pos.y, pos.z] = 1f;

        }

        /// <summary>
        /// Gets the value from _terrainMap at the specified point.
        /// </summary>
        /// <param name="point">The point to return the value from.</param>
        /// <returns>The terrain map value at the specified point.</returns>
        public float GetTerrainMapValue(Vector3Int point)
        {
            return _terrainMap[point.x, point.y, point.z];
        }

        /// <summary>
        /// Sets the value of _terrainMap at the specified point.
        /// </summary>
        /// <param name="point">The point to return the value from.</param>
        /// <param name="height">The height value to set.</param>
        public void SetTerrainMapValue(Vector3Int point, float height)
        {
            if (point.x < 0) point.x = 16;
            if (point.z < 0) point.z = 16;

            _terrainMap[point.x, point.y, point.z] = height;

        }

        /// <summary>
        /// Sets the visibility of the chunk game object associated with this chunk.
        /// </summary>
        /// <param name="visibility">The visibility required.</param>
        public void SetChunkVisiblity(bool visibility)
        {
            _chunkGameObject.SetActive(visibility);
        }

        /// <summary>
        /// Gets the visibility of the chunk game object associated with this chunk.
        /// </summary>
        /// <returns></returns>
        public bool GetChunkVisibility()
        {
            return _chunkGameObject.activeSelf;
        }

        /// <summary>
        /// Creates the terrain for the chunks.
        /// Calls:
        /// PopulateTerrainMap();
        /// MarchCubes();
        /// Then sets the _dataReady flag.
        /// Uses threading if it has been enabled by the TerrainController (default is to use threading).
        /// </summary>
        public void CreateChunkTerrainData()
        {

            if (_terrainManager.ThreadingEnabled)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(BuildChunkDataThreaded));
            }
            else
            {
                PopulateTerrainMap();
                MarchCubes();
                _dataReady = true;
            }

        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates the configuration index of the triangle within the cube.
        /// This is then used to find the correct value in the GameData.TriangleTable.
        /// </summary>
        /// <param name="cube">The cube we are checking.</param>
        /// <returns></returns>
        private static int GetCubeConfiguration(float[] cube)
        {

            // Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
            // A negative value is below the surface and a positive value is above.
            int configurationIndex = 0;
            for (int i = 0; i < 8; i++)
            {

                // If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
                // the surface, the bit would look like 00100000, which represents the integer value 32.
                if (cube[i] > 0)
                {
                    configurationIndex |= 1 << i;
                }

            }

            return configurationIndex;

        }

        /// <summary>
        /// Adds the vertice to the Dictionary if it doesn't already exist and stores its index number as its value against it.
        /// </summary>
        /// <param name="vert">The vertice to add.</param>
        /// <param name="vertices">The dictionary we are adding to.</param>
        /// <returns>The index number of the vertice in the dictionary.</returns>
        private static int VertForIndice(Vector3 vert, Dictionary<Vector3, int> vertices)
        {

            if (vertices.ContainsKey(vert))
            {
                return vertices[vert];
            }
            else
            {
                vertices.Add(vert, vertices.Count);
                return vertices.Count - 1;
            }

        }


        /// <summary>
        /// Generates the 3D noise for the chunk and populates the _terrainMap[,,] array from the noise data.
        /// Includes seaming of the edges of the chunks the ensure a smooth transtion between chunks if the base terrain level of the chunk is different from its neighbour.
        /// Also sets the _needsWaterTile flag.
        /// </summary>
        private void PopulateTerrainMap()
        {

            // Generate the densityMap of 3D Simplex noise for this terrain chunk.
            _densityMap = DensityMap.GenerateDensityMap(GameData.ChunkWidth + 1, GameData.ChunkHeight + 1, _terrainManager.Seed, _terrainManager.NoiseScale3D, _terrainManager.Octaves3D, _terrainManager.Persistence3D, _terrainManager.Lacunarity3D, Vector3.zero, _position);

            float positiveDensity;
            float densityValue;

            for (int x = 0; x < _width + 1; x++)
            {
                for (int y = 0; y < _height + 1; y++)
                {
                    for (int z = 0; z < _width + 1; z++)
                    {


                        densityValue = GetDensityValueAtPoint(x, y, z);

                        float terrainHeight = _terrainManager.TerrainHeight;



                        positiveDensity = terrainHeight * (densityValue + densityValue);

                        if (_terrainManager.IncludeCaves)
                        {
                            float cave = GenerateCaveDensity(x, y, z, terrainHeight);

                            if (_terrainManager.ReverseCaveEffect)
                            {
                                _terrainMap[x, y, z] = y - (positiveDensity / 2) * 1.25f - cave - _baseTerrainHeight[x, z];
                            }
                            else
                            {
                                _terrainMap[x, y, z] = y - (positiveDensity / 2) * 1.25f + cave - _baseTerrainHeight[x, z];

                                IsFloorNeeded();
                            }

                        }
                        else
                        {
                            _terrainMap[x, y, z] = y - (positiveDensity / 2) * 1.25f - _baseTerrainHeight[x, z];
                        }

                    }
                }
            }

            if (_terrainManager.IncludeWater)
            {
                // Check the terrainMap value at the water level height to see if we have any terrain there. If we do we need to flag that water is required.
                for (int x = 0; x < _width + 1; x++)
                {
                    for (int z = 0; z < _width + 1; z++)
                    {
                        if (_terrainMap[x, _terrainManager.WaterLevel, z] > 0)
                        {
                            _needsWaterTile = true;
                            break;
                        }
                    }

                    if (_needsWaterTile)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Check the terrainMap value at the bottom to see if we have any terrain there. If we do we need to flag that a floor tile is required.
        /// </summary>
        private void IsFloorNeeded()
        {          
            for (int x = 0; x < _width + 1; x++)
            {
                for (int z = 0; z < _width + 1; z++)
                {
                    if (_terrainMap[x, 0, z] > 0)
                    {
                        _needsFloorTile = true;
                        break;
                    }
                }

                if (_needsFloorTile)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Populate an float array with the base terrain height value for each position in the chunk.
        /// </summary>
        private void PopulateBaseTerrainHeightArray()
        {
            for (int x = 0; x < _width + 1; x++)
            {
                for (int z = 0; z < _width + 1; z++)
                {
                    _baseTerrainHeight[x, z] = _terrainManager.GetBaseTerrainHeight(_position.x + x, _position.z + z);
                }
            }
        }

        /// <summary>
        /// Alters the value of the point passed in to generate cave effects.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="z">Z position.</param>
        /// <param name="terrainHeight">The current height of the terrain at that posiotn.</param>
        /// <returns>The new terrain height value at that point.</returns>
        private float GenerateCaveDensity(int x, int y, int z, float terrainHeight)
        {
            int tempY = y - (int)terrainHeight;
            if (tempY < 0) tempY = 0;

            float cave = GetDensityValueAtPoint(x, tempY, z) / 0.75f;

            if (Mathf.Abs(Mathf.Floor(cave * 25)) - (_terrainManager.BurrowIntensity + 10) > 0)
            {
                // Depth of the caves
                cave = Mathf.Abs(cave * _terrainManager.CaveDepth);
            }
            else
            {
                cave = 0;
            }

            return cave;

        }

        /// <summary>
        /// Find the 3D noise density value at a specific point in the chunk.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="z">Z position.</param>
        /// <returns>The value of the 3D noise at the specified point.</returns>
        private float GetDensityValueAtPoint(int x, int y, int z)
        {
            return _densityMap[x, y, z];
        }


        /// <summary>
        /// This iterates through each cube in the chunk and calls the MarchCube() method for each one.
        /// </summary>
        private void MarchCubes()
        {

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int z = 0; z < _width; z++)
                    {
                        MarchCube(new Vector3Int(x, y, z));
                    }
                }

            }

            return;
        }

        /// <summary>
        /// Calculates the vertices and triangles for a single cube within the chunk.
        /// </summary>
        /// <param name="position">The position of the cube within the chunk.</param>
        private void MarchCube(Vector3Int position)
        {

            // Sample terrain values at each corner of the cube.
            float[] cube = new float[8];
            for (int i = 0; i < 8; i++)
            {
                cube[i] = GetTerrainMapValue(position + GameData.CornerTable[i]);
            }

            // Get the configuration index of this cube.
            int configIndex = GetCubeConfiguration(cube);

            // If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
            if (configIndex == 0 || configIndex == 255)
                return;

            // Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
            int edgeIndex = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int p = 0; p < 3; p++)
                {

                    // Get the current indice. We increment triangleIndex through each loop.
                    int indice = GameData.TriangleTable[configIndex, edgeIndex];

                    // If the current edgeIndex is -1, there are no more indices and we can exit the function.
                    if (indice == -1)
                    {
                        return;
                    }

                    // Get the vertices for the start and end of this edge.
                    Vector3 vert1 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 0]];
                    Vector3 vert2 = position + GameData.CornerTable[GameData.EdgeIndexes[indice, 1]];

                    // Get the terrain values at either end of our current edge from the cube array created above.
                    float vert1Sample = cube[GameData.EdgeIndexes[indice, 0]];
                    float vert2Sample = cube[GameData.EdgeIndexes[indice, 1]];

                    // Calculate the difference between the terrain values.
                    float difference = (0 - vert1Sample) / (vert2Sample - vert1Sample);

                    // Calculate the point along the edge that passes through.
                    Vector3 vertPosition = vert1 + ((vert2 - vert1) * difference);

                    // Add to our vertices and triangles list and incremement the edgeIndex.
                    _triangles.Add(VertForIndice(vertPosition, _vertices));

                    edgeIndex++;

                }

            }

            return;

        }

        /// <summary>
        /// Calls from within a thread:
        /// PopulateTerrainMap();
        /// MarchCubes();
        /// Only called by CreateChunkTerrainData() if threading has been enabled by the TerrainManager (default is to use threading).
        /// </summary>
        /// <param name="obj"></param>
        private void BuildChunkDataThreaded(object obj)
        {
            PopulateTerrainMap();
            MarchCubes();
            _dataReady = true;
        }

        #endregion
    }
}