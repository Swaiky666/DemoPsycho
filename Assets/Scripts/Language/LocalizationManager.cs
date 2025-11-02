using UnityEngine;
using System;

public class LocalizationManager : MonoBehaviour
{
    [SerializeField] private LocalizationConfig config;
    
    private LocalizationConfig.Language _currentLanguage = LocalizationConfig.Language.Chinese;
    
    public static LocalizationManager Instance { get; private set; }
    
    public event Action<LocalizationConfig.Language> OnLanguageChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (config != null)
            config.Initialize();
        
        // 读取保存的语言设置
        _currentLanguage = (LocalizationConfig.Language)PlayerPrefs.GetInt("Language", 0);
    }
    
    /// <summary>
    /// 获取本地化字符串
    /// </summary>
    public string GetString(string key)
    {
        if (config == null) return key;
        return config.GetString(key, _currentLanguage);
    }
    
    /// <summary>
    /// 切换语言
    /// </summary>
    public void SetLanguage(LocalizationConfig.Language language)
    {
        if (_currentLanguage == language) return;
        
        _currentLanguage = language;
        PlayerPrefs.SetInt("Language", (int)language);
        PlayerPrefs.Save();
        
        OnLanguageChanged?.Invoke(language);
    }
    
    /// <summary>
    /// 获取当前语言
    /// </summary>
    public LocalizationConfig.Language GetCurrentLanguage()
    {
        return _currentLanguage;
    }
    
    /// <summary>
    /// 切换到中文
    /// </summary>
    public void SetChinese()
    {
        SetLanguage(LocalizationConfig.Language.Chinese);
    }
    
    /// <summary>
    /// 切换到英文
    /// </summary>
    public void SetEnglish()
    {
        SetLanguage(LocalizationConfig.Language.English);
    }
}