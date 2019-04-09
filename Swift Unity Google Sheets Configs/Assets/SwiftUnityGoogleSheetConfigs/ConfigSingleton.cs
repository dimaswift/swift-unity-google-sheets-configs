using UnityEngine;

namespace SwiftUnityGoogleSheetConfigs
{
    public abstract class ConfigSingleton<T, TM> : SheetParserContainer where T : new() where TM : ConfigSingleton<T, TM>
    {
        public static T Main
        {
            get
            {
                return Instance._data;
            }
        }

        static TM _instance;

        public static TM Instance
        {
            get
            {
                if (_instance == null)
                {
                    var i = CreateInstance<TM>();
                    i.name = typeof(TM).Name;
                    var path = i.ResourcesFolder;
                    
                    if(Application.isEditor)
                        DestroyImmediate(i);
                    else  Destroy(i);
                    
                    _instance = Resources.Load<TM>(path);
                    
                    if (_instance == null)
                    {
                        Debug.LogError("Cannot find instance of " +  typeof(T).Name + " under path: " + path);
                    }
                }

                return _instance;
            }
        }

        protected abstract string ResourcesFolder { get; }
        
        [SerializeField] T _data;
        
        protected override void ParseSheet(string[] rows)
        {
            _data = SheetParser.ParseToObject(_data, rows, GetSeparator(), GetCustomParsers());
        }
    }
}

