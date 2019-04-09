using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SwiftUnityGoogleSheetConfigs
{
    public interface ICustomStringParser
    {
        bool TryParse(string field, out object result);
        Type ItemType();
    }

    public static class SheetParser
    {
        static void SetField(FieldInfo field, object target, string txt, IEnumerable<ICustomStringParser> stringParsers)
        {
            
            foreach (var customParser in stringParsers)
            {
                
                if (field.FieldType == customParser.ItemType())
                {
                    object res;
                    if (customParser.TryParse(txt, out res))
                    {
                        field.SetValue(target, res);
                    }
                    break;
                }
            }
            if (field.FieldType == typeof(int))
            {
                int intValue = 0;
                int.TryParse(txt, out intValue);
                field.SetValue(target, intValue);
            }
            else if (field.FieldType == typeof(float))
            {
                float floatValue = 0;
                float.TryParse(txt, out floatValue);
                field.SetValue(target, floatValue);
            }
            else if (field.FieldType == typeof(string[]))
            {
                string[] values = new string[] {txt};
                if (txt.Contains("-"))
                {
                    values = txt.Split('-');
                    for (int j = 0; j < values.Length; j++)
                    {
                        values[j] = values[j].Replace(" ", "");
                    }
                }
                else if (txt.Contains(","))
                {
                    values = txt.Split(',');
                    for (int j = 0; j < values.Length; j++)
                    {
                        values[j] = values[j].Replace(" ", "");
                    }
                }
                field.SetValue(target, values);
            }
            else if (field.FieldType == typeof(string))
            {
                field.SetValue(target, txt);
            }
            else
            {
                try
                {
                    var json = JsonUtility.FromJson(txt, field.FieldType);
                    if(json != null)
                        field.SetValue(target, json);
                }
                catch
                {
                    
                }
            }
        }
     
        public static T ParseToObject<T>(T obj, string[] rows, string separator, params ICustomStringParser[] stringParsers) where T : new()
        {
            var separatorChars = separator.ToCharArray();
            var fields = new List<FieldInfo>(typeof(T).GetFields());
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                if(string.IsNullOrEmpty(row))
                    continue;
                var items = row.Split(separatorChars);
                if (items.Length < 2)
                    continue;
                var fieldName = items[0];
                var value = items[1];
                var field = fields.Find(f =>
                    string.Equals(f.Name, fieldName, StringComparison.InvariantCultureIgnoreCase));
                if (field != null)
                {
                    SetField(field, obj, value, stringParsers);
                }
            }
            return obj;
        }
        
        public static void ParseToList<T>(List<T> list, string[] rows, string separator, IEnumerable<ICustomStringParser> stringParsers) where T : class, new()
        {
            var separatorChars = separator.ToCharArray();
            var headers = rows[0].Split(separatorChars);

            for (int i = 0; i < headers.Length; i++)
            {
                headers[i] = headers[i].Replace((char)160, (char)32); 
            }
            
            var fields = new List<FieldInfo>(typeof(T).GetFields());

            int itemsAdded = 0;
            
            for (int i = 1; i < rows.Length; i++)
            {
                var col = rows[i].Split(separatorChars);
                T item;
                if (itemsAdded < list.Count)
                {
                    item = list[itemsAdded];
                }
                else
                {
                    item = new T();
                    list.Add(item);
                }
                for (int j = 0; j < col.Length; j++)
                {
                    if(j >= headers.Length)
                        continue;
                    
                    var header = headers[j];
                
                    if (header != null && !string.IsNullOrEmpty(header))
                    {
                        var field = fields.Find(f => string.Equals(f.Name, header, StringComparison.InvariantCultureIgnoreCase));
                        
                        if (field != null)
                        {
                            var fieldCell = col[j];
                            SetField(field, item, fieldCell, stringParsers);
                        }
                    }
                }
                itemsAdded++;
            }
        }
    }

}
