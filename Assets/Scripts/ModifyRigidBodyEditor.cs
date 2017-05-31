//Assets/Editor/ModifyRigidBodyEditor.cs
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

class ModifyRigidBodyEditor
{
    [MenuItem("Tools/Change all RigidBody Interpolations in assets to None... :(")]
    static void ModifyRigidBodyMenu()
    {

        if (EditorUtility.DisplayDialog("Rigid Body changer", "Are you sure you want to change all of your assets RigidBody interpolations to None? There's no undo for this", "Do it...", "Don't do it, I'll wait for unity to fix this..."))
        {
            string[] allPrefabs = GetAllPrefabs();
            foreach (string prefab in allPrefabs)
            {


                UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath(prefab);
                GameObject go;
                try
                {
                    go = (GameObject)o;
                    foreach (Rigidbody c in go.GetComponentsInChildren<Rigidbody>(true))
                    {
                        c.interpolation = RigidbodyInterpolation.None;
                        Debug.Log("Prefab's " + prefab + " RigidBody interpolation set to None");
                    }
                }
                catch
                {
                    Debug.Log("Warning: Prefab " + prefab + " won't cast to GameObject");

                }
            }
        }
    }

    public static string[] GetAllPrefabs()
    {
        string[] temp = AssetDatabase.GetAllAssetPaths();
        List<string> result = new List<string>();
        foreach (string s in temp)
        {
            if (s.Contains(".prefab")) result.Add(s);
        }
        return result.ToArray();
    }
}