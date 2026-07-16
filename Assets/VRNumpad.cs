using UnityEngine;
using TMPro;

public class VRNumpad : MonoBehaviour
{
    [Header("目标输入框")]
    public TMP_InputField targetInputField;

    // 当点击数字或小数点时调用
    public void OnKeyPress(string key)
    {
        if (targetInputField != null)
        {
            targetInputField.text += key; // 在现有文字后面加上这个字符
        }
    }

    // 当点击退格/删除键时调用
    public void OnBackspace()
    {
        if (targetInputField != null && targetInputField.text.Length > 0)
        {
            // 删掉最后一个字符
            targetInputField.text = targetInputField.text.Substring(0, targetInputField.text.Length - 1);
        }
    }

    // 当点击清空键时调用
    public void OnClear()
    {
        if (targetInputField != null)
        {
            targetInputField.text = "";
        }
    }
}