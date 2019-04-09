using System;
using UnityEngine;

namespace SwiftUnityGoogleSheetConfigs
{
    public abstract class DownloadableObject : ScriptableObject
    {
        public bool IsDownloading { get; set; }

        public abstract void StartDownloading(Action<bool> downloadCallback);

        public void Load(Action onLoad)
        {
            StartDownloading(succ => onLoad());
        }
    }
} 