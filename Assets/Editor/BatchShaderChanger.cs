using UnityEngine;
using UnityEditor;

public class BatchShaderChanger : EditorWindow
{
    private Shader newShader;

    [MenuItem("Tools/Batch Shader Changer")]
    public static void ShowWindow()
    {
        GetWindow<BatchShaderChanger>("Batch Shader Changer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Shader Changer", EditorStyles.boldLabel);

        newShader = EditorGUILayout.ObjectField("New Shader", newShader, typeof(Shader), false) as Shader;

        if (GUILayout.Button("Change Shaders"))
        {
            ChangeShaders();
        }
    }

    private void ChangeShaders()
    {
        if (newShader == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a shader", "OK");
            return;
        }

        Object[] materials = Selection.objects;

        foreach (Object obj in materials)
        {
            if (obj is Material)
            {
                Material material = (Material)obj;
                material.shader = newShader;
                EditorUtility.SetDirty(material);
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Success", "Shaders changed successfully!", "OK");
    }
}
