using TMPro;
using UnityEngine;

public class UsernameManager : MonoBehaviour
{
    public TMP_InputField usernameInputField;

    private const string UsernameKey = "Username";

    void Start()
    {
        // 檢查是否已有儲存的 Username
        if (PlayerPrefs.HasKey(UsernameKey))
        {
            // 將儲存的 Username 設置到 InputField
            usernameInputField.text = PlayerPrefs.GetString(UsernameKey);
        }

        // 添加文字修改的監聽器
        usernameInputField.onValueChanged.AddListener(SaveUsername);
    }

    private void SaveUsername(string newUsername)
    {
        // 儲存新的 Username
        PlayerPrefs.SetString(UsernameKey, newUsername);
        PlayerPrefs.Save();
    }
}
