using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using System;

/// <summary>
/// Adds menu item MCTerrain/Add Tags to the Editor window. When Add Tags menu opton is selected the required tags for the project are added.
/// Note that there is no need for the user to save the project after adding the tags.
/// </summary>
/// 
namespace MCTerrain
{
    public class AddTags : Editor
    {
        // Add the menu item
        [MenuItem("Tools/MC Terrain/Add Tags", false, -1)]

        /// <summary>
        /// Adds the tags to the project.
        /// </summary>
        public static void CreateTags()
        {

            List<string> tags = new List<string>(InternalEditorUtility.tags);

            List<string> newTags = new List<string>();

            newTags.Add("Terrain");

            foreach (string newTag in newTags)
            {
                if (string.IsNullOrEmpty(newTag)) break;

                for (int t = 0, count = tags.Count; t < count; ++t)
                {
                    if (newTag.Equals(tags[t], StringComparison.Ordinal)) break;
                }

                InternalEditorUtility.AddTag(newTag);

                tags.Add(newTag);
            }

        }

    }
}

