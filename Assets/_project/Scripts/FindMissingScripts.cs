#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts")]
    static void ShowWindow()
    {
        GetWindow<FindMissingScripts>("Find Missing Scripts");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in Scene"))
        {
            FindInScene();
        }

        if (GUILayout.Button("Find Missing Scripts in Project"))
        {
            FindInProject();
        }

        if (GUILayout.Button("Remove All Missing Scripts in Scene"))
        {
            RemoveInScene();
        }
    }

    static void FindInScene()
    {
        var objs = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (var obj in objs)
        {
            var components = obj.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null)
                {
                    Debug.LogError($"Missing script on: {GetFullPath(obj)}", obj);
                    count++;
                }
            }
        }

        Debug.Log($"Found {count} missing scripts in scene");
    }

    static void FindInProject()
    {
        var allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;

        foreach (var guid in allPrefabs)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                var components = prefab.GetComponentsInChildren<Component>(true);
                foreach (var component in components)
                {
                    if (component == null)
                    {
                        Debug.LogError($"Missing script in prefab: {path}", prefab);
                        count++;
                        break;
                    }
                }
            }
        }

        Debug.Log($"Found {count} prefabs with missing scripts");
    }

    static void RemoveInScene()
    {
        var objs = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (var obj in objs)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            if (removed > 0)
            {
                Debug.Log($"Removed {removed} missing scripts from: {GetFullPath(obj)}", obj);
                count += removed;
            }
        }

        Debug.Log($"Removed {count} total missing scripts from scene");
    }

    static string GetFullPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
}
#endif