using UnityEditor;
using UnityEngine;

public class AddMeshCollidersEditor : EditorWindow
{
    private GameObject parentObject;

    [MenuItem("Tools/Add Mesh Colliders to Children")]
    public static void ShowWindow()
    {
        GetWindow<AddMeshCollidersEditor>("Add Mesh Colliders");
    }

    void OnGUI()
    {
        GUILayout.Label("Add Mesh Colliders to Children", EditorStyles.boldLabel);
        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);

        if (GUILayout.Button("Add Mesh Colliders"))
        {
            if (parentObject != null)
            {
                AddCollidersToChildren(parentObject.transform);
            }
            else
            {
                Debug.LogError("No parent object selected.");
            }
        }
    }

    void AddCollidersToChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.GetComponent<MeshFilter>() && !child.GetComponent<MeshCollider>())
            {
                Undo.AddComponent<MeshCollider>(child.gameObject);
            }
            AddCollidersToChildren(child);
        }
    }
}
