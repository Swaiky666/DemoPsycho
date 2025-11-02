using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string localizationKey;
    private TextMeshProUGUI _textComponent;
    
    private void Awake()
    {
        _textComponent = GetComponent<TextMeshProUGUI>();
    }
    
    private void OnEnable()
    {
        // 初始化文本
        UpdateText();
        
        // 监听语言变化
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        }
    }
    
    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }
    
    private void UpdateText(LocalizationConfig.Language language = default)
    {
        if (LocalizationManager.Instance != null && _textComponent != null)
        {
            _textComponent.text = LocalizationManager.Instance.GetString(localizationKey);
        }
    }
    
    /// <summary>
    /// 在编辑器中手动设置key
    /// </summary>
    public void SetKey(string key)
    {
        localizationKey = key;
        UpdateText();
    }
}