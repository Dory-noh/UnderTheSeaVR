using UnityEngine;

/// <summary>
/// Allows for various Dev Mode flags to be set for the game.
/// Controls the player object using basic dev mode movement to fly the player around the map.
/// </summary>
/// 

namespace MCTerrain
{
    public class DevModeManager : MonoBehaviour
    {
        #region Singleton Pattern

        private static DevModeManager _instance = null;
        public static DevModeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (DevModeManager)FindObjectOfType(typeof(DevModeManager));
                }

                return _instance;
            }

        }

        private void Awake()
        {
            // Singleton Pattern - we only want one instance of Dev Mode Manager to exist, but also want to attach the script to a game object, so using Singleton Pattern rather than a static class.
            // If there has already been another instance of Dev Mode Manager assigned that isn't this one, delete it as we can only have one instance.
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("More that one instance of DevModeManager present. Removing additional instance.");
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }
        #endregion

        #region Public Variables

        public bool NoThreading;
        public bool DisplayFPS;
        public bool DisplayChunkInfo;

        public bool HideGrass;

        #endregion
    } 

}