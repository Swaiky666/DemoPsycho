using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Localization/Config")]
public class LocalizationConfig : ScriptableObject
{
    public enum Language { Chinese, English }
    
    [System.Serializable]
    public class LocalizedString
    {
        public string key;
        public string chinese;
        public string english;
    }
    
    [SerializeField] public LocalizedString[] strings = new LocalizedString[0];
    
    private Dictionary<string, LocalizedString> _dict;
    
    public void Initialize()
    {
        _dict = new Dictionary<string, LocalizedString>();
        foreach (var str in strings)
        {
            if (!_dict.ContainsKey(str.key))
                _dict.Add(str.key, str);
        }
    }
    
    public string GetString(string key, Language language)
    {
        if (_dict == null) Initialize();
        
        if (_dict.TryGetValue(key, out var str))
        {
            return language == Language.Chinese ? str.chinese : str.english;
        }
        return key; // 如果没找到，返回key
    }
}