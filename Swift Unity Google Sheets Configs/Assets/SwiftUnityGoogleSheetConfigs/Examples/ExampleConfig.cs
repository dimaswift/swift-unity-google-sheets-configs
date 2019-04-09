using System;
using System.Collections;
using System.Collections.Generic;
using SwiftUnityGoogleSheetConfigs;
using UnityEngine;

namespace SwiftUnityGoogleSheetConfigs.Examples
{
    [CreateAssetMenu(menuName = "SwiftUnityGoogleSheetConfigs/ExampleConfig")]
    public class ExampleConfig : Config<ConfigExample>
    {
    
    }

    [Serializable]
    public class ConfigExample
    {
        public string stringTest;
        public float floatTest;
        public Person jsonTest;
    
    
        [Serializable]
        public class Person
        {
            public string name;
            public int age;
        }
    }


}
