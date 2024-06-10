using UnityEngine;
using UnityEditor;
using TMPro;

public class ChangeTMPFont : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/Change TMP Font")]
    public static void ShowWindow()
    {
        GetWindow<ChangeTMPFont>("Change TMP Font");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select the new TMP Font Asset", EditorStyles.boldLabel);

        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New TMP Font Asset", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Change All TMP Fonts"))
        {
            ChangeAllTMPFonts();
        }
    }

    private void ChangeAllTMPFonts()
    {
        if (newFont == null)
        {
            Debug.LogError("No TMP Font Asset selected!");
            return;
        }

        TMP_Text[] tmpTexts = FindObjectsOfType<TMP_Text>();
        foreach (TMP_Text tmpText in tmpTexts)
        {
            tmpText.font = newFont;
            EditorUtility.SetDirty(tmpText);
        }

        Debug.Log("All TMP fonts have been changed to " + newFont.name);
    }
}
