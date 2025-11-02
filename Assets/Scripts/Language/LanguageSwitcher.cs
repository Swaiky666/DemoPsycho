using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanguageSwitcher : MonoBehaviour
{
    [SerializeField] private Button chineseButton;
    [SerializeField] private Button englishButton;
    [SerializeField] private TextMeshProUGUI currentLanguageDisplay;
    
    private void Start()
    {
        if (chineseButton != null)
            chineseButton.onClick.AddListener(SetChinese);
        
        if (englishButton != null)
            englishButton.onClick.AddListener(SetEnglish);
        
        // 初始化语言显示
        UpdateLanguageDisplay();
        
        // 监听语言变化
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
    
    private void SetChinese()
    {
        LocalizationManager.Instance?.SetChinese();
    }
    
    private void SetEnglish()
    {
        LocalizationManager.Instance?.SetEnglish();
    }
    
    private void OnLanguageChanged(LocalizationConfig.Language language)
    {
        UpdateLanguageDisplay();
    }
    
    private void UpdateLanguageDisplay()
    {
        if (currentLanguageDisplay != null && LocalizationManager.Instance != null)
        {
            var language = LocalizationManager.Instance.GetCurrentLanguage();
            string displayText = language == LocalizationConfig.Language.Chinese ? "中文" : "English";
            currentLanguageDisplay.text = displayText;
        }
    }
}