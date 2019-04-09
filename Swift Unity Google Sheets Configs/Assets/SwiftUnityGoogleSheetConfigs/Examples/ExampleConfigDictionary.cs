using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwiftUnityGoogleSheetConfigs;

namespace SwiftUnityGoogleSheetConfigs.Examples
{
    [CreateAssetMenu(menuName = "SwiftUnityGoogleSheetConfigs/ExampleConfig")]
    public class ExampleConfigDictionary : ConfigDictionary<MyData>
    {
        public override void OnItemsDownloaded(List<MyData> items)
        {
            foreach (var item in items)
            {
                Debug.Log(item.Id + " config downloaded!");
            }
        }
    }

    [Serializable]
    public class MyData : IUniqueItem
    {
        public string Id;
        public int Damage;

        public string UniqueId
        {
            get { return Id; }
        }
    }
}
