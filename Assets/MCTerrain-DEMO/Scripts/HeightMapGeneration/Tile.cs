using UnityEngine;

namespace MapGeneration
{

	public class Tile
	{

		public float HeightValue { get; set; }

		public int X, Y;
		public int Bitmask;
		public int BiomeBitmask;

		public Tile Left;
		public Tile Right;
		public Tile Top;
		public Tile Bottom;

		public bool Collidable;
		public bool FloodFilled;

		public Color Color = Color.black;


		public Tile()
		{
		}

	}

}