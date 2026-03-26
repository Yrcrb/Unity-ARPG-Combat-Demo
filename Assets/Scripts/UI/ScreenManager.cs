using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    [Header("界面1下拉列表")]
    public TMP_Dropdown modeDropdown;
    public TMP_Dropdown resolutionDropdown; // 分辨率列表
    [Header("界面2下拉列表")]
    public TMP_Dropdown modeDropdown_2;
    public TMP_Dropdown resolutionDropdown_2; 

    [SerializeField]private List<Vector2Int> resolutionList = new List<Vector2Int>();

    private void Start()
    {
        ReadValue();
        // 模式切换
        if (modeDropdown != null) modeDropdown.onValueChanged.AddListener(OnModeChanged);
        if (modeDropdown_2 != null) modeDropdown_2.onValueChanged.AddListener(OnModeChanged);

        // 分辨率切换
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (resolutionDropdown_2 != null) resolutionDropdown_2.onValueChanged.AddListener(OnResolutionChanged);
    }
    private void ReadValue()
    {
        foreach (var resolutions in resolutionDropdown.options)
        { 
            string text = resolutions.text;
            if (TryParseResolution(text, out int w, out int h))
            {
                resolutionList.Add(new Vector2Int(w, h));
            }
            else {
                resolutionList.Add(new Vector2Int(2560, 1440));
            }
        }
    }

    private bool TryParseResolution(string text, out int width, out int height)
    {
        width = 0;
        height = 0;

        // 移除所有空格，统一格式
        string cleanText = text.Replace(" ", "");

        // 尝试常见的分隔符：x, X, *
        char[] separators = { 'x', 'X', '*' };
        string[] parts = cleanText.Split(separators); //根据字符分成两部分

        if (parts.Length == 2 && int.TryParse(parts[0], out width) && int.TryParse(parts[1], out height))
        {
            return true;
        }
        return false;
    }

    public void OnModeChanged(int index)
    {
        if (modeDropdown != null) modeDropdown.value = index;
        if (modeDropdown_2 != null) modeDropdown_2.value = index;
        // 0=全屏，1=窗口
        if (index == 0)
        {
            ApplySettings(true, Screen.width, Screen.height);
        }
        else if (index == 1)
        {
            ApplySettings(false, Screen.width, Screen.height);
        }
        Debug.Log("模式切换为: " + (index == 0 ? "全屏" : "窗口"));
    }

    //  切换分辨率
    public void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= resolutionList.Count) return;
        if (resolutionDropdown != null) resolutionDropdown.value = index;
        if (resolutionDropdown_2 != null) resolutionDropdown_2.value = index;
        Vector2Int res = resolutionList[index];

        // 保持当前的全屏状态不变
        bool isCurrentlyFullScreen = Screen.fullScreen;
        ApplySettings(isCurrentlyFullScreen, res.x, res.y);
        Debug.Log($"分辨率切换为: {res.x}x{res.y}");
    }
    private void ApplySettings(bool fullScreen, int width, int height)
    {
        Screen.SetResolution(width, height, fullScreen);
        Screen.fullScreen = fullScreen; // 再次确认，防止 SetResolution 没生效
    }

}
