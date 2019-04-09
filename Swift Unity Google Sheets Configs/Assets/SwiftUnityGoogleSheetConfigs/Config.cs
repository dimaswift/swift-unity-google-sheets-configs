using UnityEngine;

namespace SwiftUnityGoogleSheetConfigs
{
    public abstract class Config<T> : SheetParserContainer where T : new() 
    {
        [SerializeField] T _data;
        
        protected override void ParseSheet(string[] rows)
        {
            _data = SheetParser.ParseToObject(_data, rows, GetSeparator(), GetCustomParsers());
        }
    }
}

