using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 简化的语言切换系统 - 单按钮切换
/// 
/// 用法:
/// 1. 在 Canvas 中创建一个 Button
/// 2. 将此脚本挂到 Button 的 GameObject 上
/// 3. 设置 displayText 的引用
/// 4. 完成！点击按钮即可切换中英文
/// 
/// 行为:
/// - 默认英文，按钮显示 "中文" (点击切换到中文)
/// - 当前中文，按钮显示 "English" (点击切换到英文)
/// </summary>
public class SimplifiedLanguageSwitcher : MonoBehaviour
{
    [SerializeField] private Button switchButton;
    [SerializeField] private TextMeshProUGUI displayText;
    
    private void Start()
    {
        if (switchButton == null)
        {
            switchButton = GetComponent<Button>();
        }
        
        if (switchButton != null)
        {
            switchButton.onClick.AddListener(ToggleLanguage);
        }
        
        // 初始化显示
        UpdateButtonDisplay();
        
        // 订阅语言改变事件
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }
    }
    
    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }
    
    /// <summary>
    /// 切换语言
    /// </summary>
    private void ToggleLanguage()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("LocalizationManager not found!");
            return;
        }
        
        var currentLanguage = LocalizationManager.Instance.GetCurrentLanguage();
        
        if (currentLanguage == LocalizationConfig.Language.Chinese)
        {
            LocalizationManager.Instance.SetEnglish();
        }
        else
        {
            LocalizationManager.Instance.SetChinese();
        }
    }
    
    /// <summary>
    /// 语言改变时的回调
    /// </summary>
    private void OnLanguageChanged(LocalizationConfig.Language language)
    {
        UpdateButtonDisplay();
    }
    
    /// <summary>
    /// 更新按钮显示文字
    /// 当前中文 → 显示 "English"
    /// 当前英文 → 显示 "中文"
    /// </summary>
    private void UpdateButtonDisplay()
    {
        if (displayText == null) return;
        
        if (LocalizationManager.Instance == null)
        {
            displayText.text = "中文";
            return;
        }
        
        var currentLanguage = LocalizationManager.Instance.GetCurrentLanguage();
        
        // 当前是中文，显示 "English"；当前是英文，显示 "中文"
        if (currentLanguage == LocalizationConfig.Language.Chinese)
        {
            displayText.text = "English";
        }
        else
        {
            displayText.text = "中文";
        }
    }
}