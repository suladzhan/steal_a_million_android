using System;
using System.Collections.Generic;
using System.Globalization;
using StealAMillion.Core;
using UnityEngine;

namespace StealAMillion
{
    [Serializable] public sealed class LocaleEntry { public string key, value; }
    [Serializable] public sealed class LocaleTable { public string code, name; public bool rtl; public LocaleEntry[] entries; }
    public sealed class LocalizationManager
    {
        private readonly Dictionary<string, string> english = new Dictionary<string, string>();
        private readonly Dictionary<string, string> selected = new Dictionary<string, string>();
        private readonly RunnerSave save;
        private readonly Dictionary<string,string> shaped = new Dictionary<string,string>();
        private readonly RTLTMPro.FastStringBuilder rtlBuffer = new RTLTMPro.FastStringBuilder(256);
        public string Code { get; private set; }
        public bool IsRtl { get; private set; }
        public static readonly string[] Codes = { "auto", "en", "ru", "tr", "de", "ar", "zh", "es", "pt", "fr", "it", "pl", "uk", "hi", "id", "vi", "th", "ja", "ko", "zh-Hant" };
        public LocalizationManager(RunnerSave data) { save = data; Read("en", english); Select(data.language); }
        private bool Read(string code, Dictionary<string, string> dictionary)
        {
            var asset = Resources.Load<TextAsset>("Localization/" + code);
            if (asset == null) return false;
            var table = JsonUtility.FromJson<LocaleTable>(asset.text);
            foreach (var entry in table.entries) dictionary[entry.key] = entry.value;
            return table.rtl;
        }
        public void Select(string code)
        {
            save.language = code;
            if (code == "auto" || string.IsNullOrEmpty(code))
            {
                code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                switch (Application.systemLanguage) {
                    case SystemLanguage.Russian: code = "ru"; break; case SystemLanguage.Turkish: code = "tr"; break;
                    case SystemLanguage.Arabic: code = "ar"; break; case SystemLanguage.German: code = "de"; break;
                    case SystemLanguage.ChineseSimplified: code = "zh"; break; case SystemLanguage.ChineseTraditional: code = "zh-Hant"; break;
                    case SystemLanguage.Spanish: code = "es"; break; case SystemLanguage.Portuguese: code = "pt"; break;
                    case SystemLanguage.French: code = "fr"; break; case SystemLanguage.Italian: code = "it"; break;
                    case SystemLanguage.Polish: code = "pl"; break; case SystemLanguage.Ukrainian: code = "uk"; break;
                    case SystemLanguage.Hindi: code = "hi"; break; case SystemLanguage.Indonesian: code = "id"; break;
                    case SystemLanguage.Vietnamese: code = "vi"; break; case SystemLanguage.Thai: code = "th"; break;
                    case SystemLanguage.Japanese: code = "ja"; break; case SystemLanguage.Korean: code = "ko"; break;
                }
            }
            selected.Clear(); shaped.Clear(); Code = code; IsRtl = Read(code, selected);
        }
        public string Text(string key, params object[] args)
        {
            string value;
            if (!selected.TryGetValue(key, out value) && !english.TryGetValue(key, out value)) value = key;
            value = args.Length == 0 ? value : string.Format(CultureInfo.InvariantCulture, value, args);
            if (IsRtl || key == "LANG_ar")
            {
                bool arabic = false; foreach (char c in value) if (c >= '\u0600' && c <= '\u06ff') { arabic = true; break; }
                if (arabic)
                {
                    string result;
                    if (shaped.TryGetValue(value,out result)) return result;
                    rtlBuffer.Clear(); RTLTMPro.RTLSupport.FixRTL(value,rtlBuffer,false,true,true); result=rtlBuffer.ToString();
                    if(shaped.Count>1000)shaped.Clear();shaped[value]=result;return result;
                }
            }
            return value;
        }
    }
}
