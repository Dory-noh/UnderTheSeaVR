using UnityEngine;
using MCTerrain;
using System.IO;

namespace MapGeneration
{
    public abstract class Generator : MonoBehaviour
    {
        protected int Seed;

        protected int Width = 512;
        protected int Height = 512;

        private HeightData saveHeightData = new HeightData();
        private HeightData loadedHeightData = new HeightData();

        [Header("Height Map")]
        [SerializeField]
        [Range(1, 8)]
        protected int TerrainOctaves = 4;
        [SerializeField]
        [Range(1, 8)]
        protected double TerrainFrequency = 2;
       
        protected MapData HeightData;

        private bool generateNewHeightData = false;

        protected Tile[,] Tiles;

        // Texture output gameobject
        protected MeshRenderer BiomeMapRenderer;

        protected Texture2D HeightTexture;

        private void Awake()
        {
            PopulateSaveHeightData();
            LoadFromJson();
            CompareHeightData();
        }

        void Start()
        {
            SaveToJson();
            Instantiate();
            Generate();
        }

        public abstract void Initialize();
        protected abstract void GetData();
        protected abstract Tile GetTop(Tile tile);
        protected abstract Tile GetBottom(Tile tile);
        protected abstract Tile GetLeft(Tile tile);
        protected abstract Tile GetRight(Tile tile);

        public virtual void Instantiate()
        {

            Seed = TerrainManager.Instance.Seed;

            Initialize();
        }

        public virtual void Generate()
        {
            GetData();
            LoadTiles();

            // Generate the initial textures to store height data
            HeightTexture = TextureGenerator.GetHeightMapTexture(Width, Height, Tiles);
            TerrainManager.Instance.HeightMapTexture = HeightTexture;

            string textureDataFilePath = Application.persistentDataPath + "/TextureData.bin";

            // If the biome height data has not changed since it was last saved then load in the large height texture rather than recreating it.
            if (!generateNewHeightData && File.Exists(textureDataFilePath))
            {
                Texture2D tex2D = new Texture2D(Width * 16, Width * 16);
                TerrainManager.Instance.HeightMapTextureLarge = LoadTextureData(tex2D, textureDataFilePath);
            }
            else
            {
                TerrainManager.Instance.HeightMapTextureLarge = ScaleTexture(HeightTexture, Width * 16, Width * 16);

                // Now save the texture data to disk so that if no settings are changed we don't have to recreate the large texture by resizing, we can just load the data in back in again.
                SaveTextureData(textureDataFilePath);
            }
        }

        private void PopulateSaveHeightData()
        {
            saveHeightData.Seed = TerrainManager.Instance.Seed;
            saveHeightData.TerrainFrequency = TerrainFrequency;
            saveHeightData.TerrainOctaves = TerrainOctaves;
        }

        private void SaveToJson()
        {
            string heightData = JsonUtility.ToJson(saveHeightData);
            string filePath = Application.persistentDataPath + "/HeightData.json";
            File.WriteAllText(filePath, heightData);

        }

        private void LoadFromJson()
        {
            string filePath = Application.persistentDataPath + "/HeightData.json";

            if (File.Exists(filePath))
            {
                string heightData = File.ReadAllText(filePath);
                loadedHeightData = JsonUtility.FromJson<HeightData>(heightData);
            }

        }

        private void CompareHeightData()
        {
            if (saveHeightData.Seed != loadedHeightData.Seed               
                || saveHeightData.TerrainFrequency != loadedHeightData.TerrainFrequency
                || saveHeightData.TerrainOctaves != loadedHeightData.TerrainOctaves)
            {
                generateNewHeightData = true;
            }

        }


        private Texture2D LoadTextureData(Texture2D tex, string filePath)
        {
            byte[] texLoadData = File.ReadAllBytes(filePath);
            tex.LoadRawTextureData(texLoadData);

            return tex;
        }
        private void SaveTextureData(string filePath)
        {
            byte[] texData = TerrainManager.Instance.HeightMapTextureLarge.GetRawTextureData();
            File.WriteAllBytes(filePath, texData);
        }

        private static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            Color[] rpixels = result.GetPixels(0);
            float incX = (1.0f / (float)targetWidth);
            float incY = (1.0f / (float)targetHeight);
            for (int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
            }
            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }

        // Build a Tile array from our data
        private void LoadTiles()
        {
            Tiles = new Tile[Width, Height];

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    Tile t = new Tile();
                    t.X = x;
                    t.Y = y;

                    //set heightmap value
                    float heightValue = HeightData.Data[x, y];
                    heightValue = (heightValue - HeightData.Min) / (HeightData.Max - HeightData.Min);
                    t.HeightValue = heightValue;

                    Tiles[x, y] = t;
                }
            }
        }

    }
}
