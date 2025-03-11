using UnityEngine;

namespace MapGeneration
{
    public static class TextureGenerator
    {

        public static Texture2D GetHeightMapTexture(int width, int height, Tile[,] tiles)
        {
            var texture = new Texture2D(width, height);
            var pixels = new Color32[width * height];

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {

                    pixels[x + y * width] = Color32.Lerp(Color.black, Color.white, tiles[x, y].HeightValue);

                }
            }

            texture.SetPixels32(pixels);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            return texture;
        }

    }
}