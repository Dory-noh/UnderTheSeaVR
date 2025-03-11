using System;
using UnityEngine;

/// <summary>
/// Readme SciptableObject class.
/// </summary>

namespace MCTerrain
{
	public class MCTerrainReadme : ScriptableObject
	{
		public Texture2D icon;
		public string title;
		public Section[] sections;
		public bool loadedLayout;

		[Serializable]
		public class Section
		{
			public string heading, text, linkText, url;
		}
	}
}
