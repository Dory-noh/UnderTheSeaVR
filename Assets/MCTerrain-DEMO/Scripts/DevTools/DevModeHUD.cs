using UnityEngine;

/// <summary>
/// Allows for dev mode information to be displayed, eg Frames Per Second.
/// </summary>

namespace MCTerrain
{
	public class DevModeHUD : MonoBehaviour
	{
		#region Private Variables

		private float _deltaTime = 0.0f;
		private string _text;
		private GUIStyle _style;
		private int _width;
		private int _height;
		private float _boxHeight;
		private bool _gotFPSTexture;
		private bool _gotTextTexture;
		private DevModeManager _devModeManager;

		#endregion

		private void Start()
		{
			_devModeManager = DevModeManager.Instance;

			_width = 460;
			_height = Screen.height;

			_style = new GUIStyle
			{
				alignment = TextAnchor.UpperLeft,
				fontSize = _height * 2 / 80
			};

			_style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);

			GUIContent boxContent = new(_text);

			_boxHeight = _style.CalcHeight(boxContent, _width);

			_style.normal.background = MakeTex(_width, (int)_boxHeight, new Color(1f, 1f, 0.8f, 0.25f));

			_gotFPSTexture = false;
			_gotTextTexture = false;
		}

		private void Update()
		{
			_deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

			if (Input.GetKeyDown(KeyCode.F1))
			{
				_devModeManager.DisplayFPS = !_devModeManager.DisplayFPS;
			}

			if (Input.GetKeyDown(KeyCode.F2))
			{
				_devModeManager.DisplayChunkInfo = !_devModeManager.DisplayChunkInfo;
			}

		}

		/// <summary>
		/// Displays the required text depending on the Dev Mode Controller settings.
		/// </summary>
		private void OnGUI()
		{
			_text = "";

			bool needFPSDisplay = false;
			bool needTextDisplay = false;

			if (DevModeManager.Instance.DisplayFPS)
			{
				float msec = _deltaTime * 1000.0f;
				float fps = 1.0f / _deltaTime;
				_text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
				needFPSDisplay = true;
			}
			else
			{
				_gotFPSTexture = false;
			}

			if (DevModeManager.Instance.DisplayChunkInfo)
			{
				Chunk chunk = TerrainManager.Instance.GetChunkUnderPlayer();
				if (chunk != null)
				{
					_text += string.Format("\nChunk position = {0}", chunk.Position);
					_text += string.Format("\nBiome Map pixel position = {0}, {1}", chunk.Position.x / 16, chunk.Position.z / 16);
					needTextDisplay = true;
				}
			}
			else
			{
				_gotTextTexture = false;
			}

			if (needFPSDisplay)
			{
				if (!_gotFPSTexture)
				{
					GUIContent boxContent = new(_text);
					_boxHeight = _style.CalcHeight(boxContent, _width);
					_style.normal.background = MakeTex(_width, (int)_boxHeight, new Color(1f, 1f, 0.8f, 0.25f));
					_gotFPSTexture = true;

				}
				GUI.Box(new Rect(0, 0, _width, _boxHeight), _text, _style);
			}

			if (needTextDisplay)
			{
				if (!_gotTextTexture)
				{
					GUIContent boxContent = new(_text);
					_boxHeight = _style.CalcHeight(boxContent, _width);
					_style.normal.background = MakeTex(_width, (int)_boxHeight, new Color(1f, 1f, 0.8f, 0.25f));
					_gotTextTexture = true;

				}
				GUI.Box(new Rect(0, 0, _width, _boxHeight), _text, _style);
			}

		}

		/// <summary>
		/// Creates a background texture for the HUD text area.
		/// </summary>
		/// <param name="width">Width of the test area.</param>
		/// <param name="height">Height of the text area.</param>
		/// <param name="col">Colour for the texture.</param>
		/// <returns>The generated texture.</returns>
		private Texture2D MakeTex(int width, int height, Color col)
		{
			Color32[] pix = new Color32[width * height];
			for (int i = 0; i < pix.Length; ++i)
			{
				pix[i] = col;
			}
			Texture2D result = new(width, height);
			result.SetPixels32(pix);
			result.Apply();
			return result;
		}
	}
}