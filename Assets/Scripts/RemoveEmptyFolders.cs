﻿using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RemoveEmptyFolders
{
    /// <summary>
    /// Use this flag to simulate a run, before really deleting any folders.
    /// </summary>
    private static bool dryRun = false;

    [MenuItem("Tools/Remove empty folders")]
    private static void RemoveEmptyFoldersMenuItem()
    {
        var index = Application.dataPath.IndexOf("/Assets");
        var projectSubfolders = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);

        // Create a list of all the empty subfolders under Assets.
        var emptyFolders = projectSubfolders.Where(path => IsEmptyRecursive(path)).ToArray();

        foreach (var folder in emptyFolders)
        {
            // Verify that the folder exists (may have been already removed).
            if (Directory.Exists(folder))
            {
                Debug.Log("Deleting : " + folder);

                if (!dryRun)
                {
                    // Remove dir (recursively)
                    Directory.Delete(folder, true);

                    // Sync AssetDatabase with the delete operation.
                    AssetDatabase.DeleteAsset(folder.Substring(index + 1));
                }
            }
        }

        // Refresh the asset database once we're done.
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// A helper method for determining if a folder is empty or not.
    /// </summary>
    private static bool IsEmptyRecursive(string path)
    {
        // A folder is empty if it (and all its subdirs) have no files (ignore .meta files)
        return Directory.GetFiles(path).Select(file => !file.EndsWith(".meta")).Count() == 0
            && Directory.GetDirectories(path, string.Empty, SearchOption.AllDirectories).All(IsEmptyRecursive);
    }
}