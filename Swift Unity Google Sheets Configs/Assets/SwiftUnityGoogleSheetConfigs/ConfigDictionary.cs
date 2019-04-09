using System;
using System.Collections.Generic;
using UnityEngine;

namespace SwiftUnityGoogleSheetConfigs
{
    public abstract class ConfigDictionary<T> : SheetParserContainer where T : class, IUniqueItem, new() 
    {
        public List<T> Items
        {
            get { return _items; }
        }
        
        [NonSerialized] Dictionary<string, T> _dict;
        [SerializeField] List<T> _items;

        
        public T this[string id]
        {
            get
            {
                if(_dict == null)
                    FillDictionary();
                T value;
                if (!_dict.TryGetValue(id, out value))
                {
                    Debug.LogError("Item with Id: " + id +$" not found in the config {name}");
                    return default(T);
                }
                return value;
            }
        }

        public bool Contains(string id)
        {
            if(_dict == null)
                FillDictionary();
            return _dict.ContainsKey(id);
        }
        
        public T GetById(string id)
        {
            return this[id];
        }
        
        public virtual void OnItemsDownloaded(List<T> items) { }

        protected override void ParseSheet(string[] rows)
        {
            if(_items == null)
                _items = new List<T>();
            SheetParser.ParseToList(_items, rows, GetSeparator(),  GetCustomParsers());
            OnItemsDownloaded(_items);
            FillDictionary();
        }
             
        private void FillDictionary()
        {
            _dict = new Dictionary<string, T>(Items.Count);
            Items.RemoveAll(i => i == null || i.UniqueId == null);
            foreach (var item in Items)
            {
                if (_dict.ContainsKey(item.UniqueId))
                {
                    Debug.LogError("Cannot add item to the dictionary: <color='white'>" +item.UniqueId + "</color>. Such Id already added!");
                    continue;
                }
                _dict.Add(item.UniqueId, item);
            }
        }
    }
}

