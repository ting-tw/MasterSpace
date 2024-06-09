using TMPro;
using UnityEngine;

public class LocallyStoredInput : MonoBehaviour
{
    private TMP_InputField inputField;

    private string InputKey;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        InputKey = "LocallyStoredInput-" + gameObject.name;

        if (PlayerPrefs.HasKey(InputKey))
        {
            inputField.text = PlayerPrefs.GetString(InputKey);
        }

        inputField.onValueChanged.AddListener(SaveInput);
    }

    private void SaveInput(string newInput)
    {
        PlayerPrefs.SetString(InputKey, newInput);
        PlayerPrefs.Save();
    }
}
