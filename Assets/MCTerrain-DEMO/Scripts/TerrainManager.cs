using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This script is attached to the TerrianManager GameObject.
/// It follows a singleton pattern so can be accessed from anywhere by creating a reference to TerrainManager.Instance.
/// It holds the noise settings for the terrain, and the view distance (the area visible to the player in each direction that must be populated with terrain chunk objects).
/// It is responsible for instantiating the player GameObject once the initial map area has been drawn.
/// Its Update() method is responsible for keeping track of which chunks are currently visible and which new chunks need to be created.
/// </summary>

namespace MCTerrain
{
    public class TerrainManager : MonoBehaviour
    {
        #region Singleton Pattern

        private static TerrainManager _instance = null;
        public static TerrainManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (TerrainManager)FindObjectOfType(typeof(TerrainManager));
                }

                return _instance;
            }

        }

        private void Awake()
        {
            // Singleton Pattern - we only want one instance of Terrain Manager to exist, but also want to attach the script to a game object, so using Singleton Pattern rather than a static class.
            // If there has already been another instance of Terrain Manager assigned that isn't this one, delete it as we can only have one instance.
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("More that one instance of TerrainManager present. Removing additional instance.");
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }
        #endregion

        #region Inspector Variables

        [Header("Starting chunk position")]
        [Range(0, 512)]
        [Tooltip("This position is shown as a red dot on the Biome preview map in the editor when viewing the BiomeGenerator game object.")]
        public int PlayerStartX = 256;
        [Range(0, 512)]
        [Tooltip("This position is shown as a red dot on the Biome preview map in the editor when viewing the BiomeGenerator game object.")]
        public int PlayerStartZ = 256;

        [Header("Basic settings")]
        [Tooltip("The material used for the terrain.")]
        public Material Material;
        [Tooltip("The max height for the terrain's hills.")]
        public float TerrainHeight;
        [Tooltip("The Grass Prefab to use for the terrain.")]
        public GameObject GrassPrefab;
        [Tooltip("This value is used to generate all random numbers used throughout the project for the terrain, the biomes, the vegetation and rocks.")]
        public int Seed = 10;
        [HideInInspector]
        public bool ThreadingEnabled = false;
        [SerializeField]
        [Tooltip("How far the player can see. Any chunks that exist and are further away are set to hidden.")]
        private float MaxViewDistance = 50f;
        [Tooltip("This is used to place a floor at the bottom of any caves that would otherwise dig below the bottom of the chunk.")]
        public GameObject FloorPrefab;
        [Header("Terrain Water")]
        [Tooltip("This must be checked if you want the water plane to be displayed.")]
        public bool IncludeWater = false;
        [Tooltip("The Water Plane prefab.")]
        public GameObject WaterPrefab;
        [Tooltip("The global water level.")]
        [Range(0, 100)]
        public int WaterLevel = 25;

        [Header("3D Noise Settings")]
        [Tooltip("The larger the value the smoother the terrain will be.")]
        public float NoiseScale3D = 100;

        [Tooltip("The number of passes over the noise. The higher the number the more effect the Persistence and Lacunarity settings will have.")]
        [Range(1, 8)]
        public int Octaves3D = 3;

        [Tooltip("The higher the number the bumpier the terrain will be. Also, the higher the number the more effect the Lacunarity setting will have.")]
        [Range(0f, 1f)]
        public float Persistence3D = 0.581f;

        [Tooltip("The higher the number the more jagged the terrain will be. Also the higher the number the more effect you will get from the Cliffs and Caves.")]
        [Range(1f, 4f)]
        public float Lacunarity3D = 2.74f;

        [Header("Caves")]
        [Tooltip("This allows for caves to be included in the landscape. Note that the Persistance and Lacunarity settings also effect the way the structure of the caves.")]
        public bool IncludeCaves;

        [Tooltip("This alters the depth of the caves below the ground. A minimum value of 8f allows enough headroom for the player to fit into any caves that appear but fewer caves are produced.")]
        [Range(8f, 50f)]
        public float CaveDepth;

        [Tooltip("This affects the general length of caves that are created. A low value produces rocky outcrops and a higher value creates some tunnels.")]
        [Range(0, 10)]
        public int BurrowIntensity;

        [Tooltip("This setting reverses the affect of the caves to produce high rocky areas.")]
        public bool ReverseCaveEffect;

        [HideInInspector]
        public Texture2D HeightMapTexture;

        [HideInInspector]
        public Texture2D HeightMapTextureLarge;

        #endregion

        #region Private Variables
        private DevModeManager _devModeManager;

        private Camera _cam;

        private readonly Dictionary<Vector3Int, Chunk> _chunksByPosition = new();
        private readonly List<Chunk> _chunksVisibleLastFrame = new();

        private int _chunkSize;
        private int _chunksVisibleInViewDst;
        private int _chunksInInitialMap;

        private bool _initialMapGenerated = false;

        private int _currentChunkCoordX;
        private int _currentChunkCoordZ;

        private int _processorCount;
        private int _framesToWaitBeforeNextChunksAdded = 0;
        private int _frameCounter = 0;

        #endregion

        /// <summary>
        /// Initialise private variables needed to manage the terrain.
        /// </summary>
        private void Start()
        {
            _cam = Camera.main;

            _chunkSize = GameData.ChunkWidth;
            _chunksVisibleInViewDst = Mathf.RoundToInt(MaxViewDistance / _chunkSize);
            _chunksInInitialMap = (_chunksVisibleInViewDst * 2 + 1) * (_chunksVisibleInViewDst * 2 + 1);

            _currentChunkCoordX = PlayerStartX;
            _currentChunkCoordZ = PlayerStartZ;

            // Find the number of cores so only request that many chunks per frame to keep things as smooth as possible
            _processorCount = Environment.ProcessorCount;
            _devModeManager = DevModeManager.Instance;
            
        }

        /// <summary>
        /// The main update loop used to create and set the visiblity of chunks as required.
        /// It takes into account the number of processor cores and attempts to spread the creation of chunks out between frames to keep chunk creation as smooth as possible.
        /// </summary>
        private void Update()
        {
            // This only runs until the initial map has been generated. It only has to check for the value of initialMapGenerated once it is true so doesn't take much time out of the update loop.
            if (!_initialMapGenerated && _chunksVisibleLastFrame.Count == _chunksInInitialMap)
            {
                // Set all chunks to visible once the initial map of chunks has been generated.
                int counter = 0;
                foreach (Chunk chunk in _chunksVisibleLastFrame)
                {
                    if (chunk.GetChunkVisibility() == true)
                    {
                        counter++;
                    }
                }

                // Instantiate the fly camera once the initial map has been placed and position it at the correctly.
                if (counter == _chunksInInitialMap)
                {
                    _initialMapGenerated = true;
                    float terrainHeightAtStart = GetTerrainHeightFromWorldSpace((_currentChunkCoordX * _chunkSize) + (_chunkSize / 2), (_currentChunkCoordZ * _chunkSize) + (_chunkSize / 2));
                    _cam.transform.position = new Vector3((_currentChunkCoordX * _chunkSize) + (_chunkSize / 2), terrainHeightAtStart + 4f, (_currentChunkCoordZ * _chunkSize) + (_chunkSize / 2));
                    _framesToWaitBeforeNextChunksAdded = 2;
                    if (!_devModeManager.NoThreading)
                    {
                        ThreadingEnabled = true;
                    }
                }
            }

            // Used to keep count of the chunks to be added per frame - matches number of cores/threads available
            int chunksToAddThisFrame = _processorCount;
            int chunksAddedThisFrame = 0;

            // Check the avaiable viewing area for chunks
            for (int zOffset = -_chunksVisibleInViewDst; zOffset <= _chunksVisibleInViewDst; zOffset++)
            {
                for (int xOffset = -_chunksVisibleInViewDst; xOffset <= _chunksVisibleInViewDst; xOffset++)
                {
                    Vector3Int viewedChunkCoord = new((_currentChunkCoordX + xOffset) * _chunkSize, 0, (_currentChunkCoordZ + zOffset) * _chunkSize);

                    // If the chunk has already been created then make it visible
                    if (_chunksByPosition.ContainsKey(viewedChunkCoord))
                    {
                        Chunk chunk = _chunksByPosition[viewedChunkCoord];
                        chunk.SetChunkVisiblity(true);
                    }
                    else
                    {
                        
                        // If a new chunk is needed in the checked position and we have not reached our limit of chunks to build this frame
                        if (chunksAddedThisFrame < chunksToAddThisFrame)
                        {
                            // Only allow the chunks to be created if we've waited the required amount of frames to give the previously requested chunks time to have completed to keep things running smooothly
                            if (_frameCounter > _framesToWaitBeforeNextChunksAdded)
                            {
                                Chunk chunk = AddChunkToTerrain(viewedChunkCoord);
                                chunk.CreateChunkTerrainData();
                                _chunksVisibleLastFrame.Add(chunk);
                                chunk.ChunkGameObject.transform.SetParent(transform);
                                _frameCounter = 0;
                            }

                            _frameCounter++;
                            chunksAddedThisFrame++;
                        }
                        
                    }
                }
            }
            
            foreach (Chunk chunk in _chunksVisibleLastFrame)
            {
                
                if (chunk.DataReady)
                {
                    chunk.BuildMesh();
                    chunk.DataReady = false;

                    if (IncludeWater)
                    {
                        InstantiateWaterPlane(chunk);
                    }

                    if (!_devModeManager.HideGrass)
                    {
                        InstantiateGrass(chunk);
                    }

                    if (chunk.NeedsFloorTile)
                    {
                        InstantiateFloorPlane(chunk);
                    }

                }
                
            }
        }

        #region Public Methods

        /// <summary>
        /// Returns the chunk that has the position matching the Vector3Int value.
        /// </summary>
        /// <param name="pos">Chunk position value.</param>
        /// <returns>Chunk at the position.</returns>
        public Chunk GetChunk(Vector3Int chunkPos)
        {
            if (_chunksByPosition.ContainsKey(chunkPos))
            {
                return _chunksByPosition[chunkPos];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the chunk that has the position matching the Vector3 value.
        /// </summary>
        /// <param name="pos">Chunk position value.</param>
        /// <returns>Chunk at the position.</returns>
        public Chunk GetChunk(Vector3 chunkPos)
        {
            Vector3Int temp = new((int)chunkPos.x, (int)chunkPos.y, (int)chunkPos.z);

            if (_chunksByPosition.ContainsKey(temp))
            {
                return _chunksByPosition[temp];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the chunk that populates a point in world space.
        /// </summary>
        /// <param name="pos">Coordinate in world space.</param>
        /// <returns>Chunk that populates that point in world space.</returns>
        public Chunk GetChunkFromWorldSpace(Vector3 pos)
        {
            bool negativeX = false;
            bool negativeZ = false;

            if(pos.x < 0)
            {
                negativeX = true;
            }

            if(pos.z < 0)
            {
                negativeZ = true;
            }

            int x = Mathf.FloorToInt(pos.x);
            int z = Mathf.FloorToInt(pos.z);

            x = (x / 16) * 16;
            z = (z / 16) * 16;

            if (negativeX)
            {
                x -= 16;
            }

            if (negativeZ)
            {
                z -= 16;
            }

           return GetChunk(new Vector3Int(x, 0, z));
        }

        /// <summary>
        /// Gets the chunk that the player is currently standing on.
        /// </summary>
        /// <returns>The chunk that the player is standing on.</returns>
        public Chunk GetChunkUnderPlayer()
        {
            if (_cam != null)
            {
                foreach (Chunk chunk in _chunksVisibleLastFrame)
                {
                    if (_cam.transform.position.x >= chunk.Position.x && _cam.transform.position.x < chunk.Position.x + 16)
                    {
                        if (_cam.transform.position.z >= chunk.Position.z && _cam.transform.position.z < chunk.Position.z + 16)
                        {
                            return chunk;
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the terrain height for a point in world space.
        /// </summary>
        /// <param name="worldPosX">X position.</param>
        /// <param name="worldPosZ">Z position.</param>
        /// <returns>The height value at the specified point in world space.</returns>
        public float GetTerrainHeightFromWorldSpace(float worldPosX, float worldPosZ)
        {
            Physics.Raycast(new Vector3(worldPosX, 100, worldPosZ), Vector3.down, out RaycastHit hit);
            return hit.point.y;
        }

        /// <summary>
        /// Instantiates a water plane as a child of the chunk game object.
        /// </summary>
        /// <param name="chunk">The chunk to add water to.</param>
        public void InstantiateWaterPlane(Chunk chunk)
        {
            GameObject water = Instantiate(WaterPrefab, new Vector3(chunk.Position.x, WaterLevel, chunk.Position.z), Quaternion.identity);
            water.transform.SetParent(chunk.ChunkGameObject.transform);
        }

        /// <summary>
        /// Instantiates a grass object as a child of the chunk game object.
        /// </summary>
        /// <param name="chunk">The chunk to add grass to.</param>
        public void InstantiateGrass(Chunk chunk)
        {
            GameObject grass = Instantiate(GrassPrefab, chunk.Position, Quaternion.identity);
            grass.transform.SetParent(chunk.ChunkGameObject.transform);
        }

        /// <summary>
        /// Get the base terrain height value for each position in the chunk from the large height map texture. Each pixel in the texture represents a position in the chunk.
        /// </summary>
        /// <param name="x">X position of chunk.</param>
        /// <param name="z">Z position of chunk.</param>
        /// <returns>The base terrain height for each chunk position.</returns>
        public float GetBaseTerrainHeight(int x, int z)
        {
            Color textureColor = HeightMapTextureLarge.GetPixel(x, z);
            float textureHeight = textureColor.r;
            return (GameData.MaxTerrainHeight - GameData.BaseTerrainHeight) * textureHeight;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a chunk to the _chunksByPosition dictionary if it doesn't already exist.
        /// </summary>
        /// <param name="chunkPos">Position of chunk to add.</param>
        /// <returns>Chunk at that position.</returns>
        private Chunk AddChunkToTerrain(Vector3Int chunkPos)
        {
            if (!_chunksByPosition.ContainsKey(chunkPos))
            {
                Chunk chunk = new Chunk(chunkPos);
                _chunksByPosition.Add(chunkPos, chunk);

                return chunk;
            }
            else
            {
                return _chunksByPosition[chunkPos];
            }
        }

        /// <summary>
        /// Instantiates a water plane as a child of the chunk game object.
        /// </summary>
        /// <param name="chunk">The chunk to add water to.</param>
        private void InstantiateFloorPlane(Chunk chunk)
        {
            if (FloorPrefab)
            {
                GameObject floor = Instantiate(FloorPrefab, new Vector3(chunk.Position.x, 0, chunk.Position.z), Quaternion.identity);
                floor.transform.SetParent(chunk.ChunkGameObject.transform);
                MeshRenderer renderer = floor.GetComponentInChildren<MeshRenderer>();
                renderer.sharedMaterial = Material;
            }

        }
        #endregion

    }
}