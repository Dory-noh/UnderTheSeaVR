using UnityEngine;
using OpenSimplex;

/// <summary>
/// Generates the 3D Noise Density Map.
/// </summary>

namespace MCTerrain
{
    public class DensityMap
    {
        /// <summary>
        /// Generates the 3D noise density map for a chunk.
        /// </summary>
        /// <param name="mapWidth">Width of density map to create.</param>
        /// <param name="mapHeight">Height of density map to create.</param>
        /// <param name="seed">Random seed value.</param>
        /// <param name="scale">The larger the value the smoother the noise will be.</param>
        /// <param name="octaves">The number of passes over the noise. The higher the number the more detail the noise will have.</param>
        /// <param name="peristance">The higher the number the bumpier the noise will be. Also, the higher the number the more effect the Lacunarity setting will have.</param>
        /// <param name="lacunarity">The higher the number the more jagged the noise will be. Also, the higher the number the more effect the Lacunarity setting will have.</param>
        /// <param name="offset">Moves the returned area of noise by the supplied offset.</param>
        /// <param name="chunkPosition">Returns the noise for the required chunk position</param>
        /// <returns></returns>
        public static float[,,] GenerateDensityMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float peristance, float lacunarity, Vector3 offset, Vector3Int chunkPosition)
        {
            float[,,] densityMap = new float[mapWidth, mapHeight, mapWidth];

            OpenSimplex2F openSimplex2F = new(10000);

            System.Random prng = new(seed);
            Vector3[] octaveOffsets = new Vector3[octaves];

            float maxPossibleHeight = 0;
            float amplitude = 1;

            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                float offsetZ = prng.Next(-100000, 100000) + offset.z;

                octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);

                maxPossibleHeight += amplitude;
                amplitude *= peristance;
            }

            if (scale <= 0)
            {
                scale = 0.0001f;
            }

            float maxNoiseDensity = float.MinValue;
            float minNoiseDensity = float.MaxValue;

            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;


            // Gather noise data
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {

                        amplitude = 1;
                        float frequency = 1;
                        float noiseDensity = 0;

                        for (int i = 0; i < octaves; i++)
                        {

                            double sampleX = (x + chunkPosition.x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                            double sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;
                            double sampleZ = (z + chunkPosition.z - halfWidth + octaveOffsets[i].z) / scale * frequency;

                            double simplexValue = openSimplex2F.Noise3_XYBeforeZ(sampleX, sampleZ, sampleY);

                            noiseDensity += (float)simplexValue * amplitude;

                            amplitude *= peristance;
                            frequency *= lacunarity;
                        }

                        if (noiseDensity > maxNoiseDensity)
                        {
                            maxNoiseDensity = noiseDensity;
                        }
                        else if (noiseDensity < minNoiseDensity)
                        {
                            minNoiseDensity = noiseDensity;
                        }

                        densityMap[x, y, z] = noiseDensity;

                    }
                }
            }

            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapWidth; z++)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {

                        densityMap[x, y, z] = Mathf.InverseLerp(-maxPossibleHeight, maxPossibleHeight, densityMap[x, y, z]);

                    }
                }

            }

            return densityMap;

        }

    }
}