using System.Collections.Generic;
using UnityEngine;

namespace SwiftUnityGoogleSheetConfigs
{
    public abstract class ConfigList<T> : SheetParserContainer where T : class, new()
    {
        [SerializeField] private List<T> _data;

        public List<T> GetItems()
        {
            return _data;
        }
        
        protected override void ParseSheet(string[] rows)
        {
            if(_data == null)
                _data = new List<T>();
            SheetParser.ParseToList(_data, rows, GetSeparator(),  GetCustomParsers());
            SetFileDirty();
        }
    }
}

