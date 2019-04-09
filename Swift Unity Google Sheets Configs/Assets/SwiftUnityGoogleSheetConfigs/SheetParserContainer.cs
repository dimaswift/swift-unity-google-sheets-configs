using System;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace SwiftUnityGoogleSheetConfigs
{
    public abstract class SheetParserContainer : DownloadableObject
    {
        [TextArea]
        [SerializeField] string _url = "";

        private string FilePath
        {
            get
            {
                return  Application.persistentDataPath + "/" + name;
            }
        }

        public bool Downloaded { get; set; }
        
        private Action<bool> _downloadCallback;

        private readonly ICustomStringParser[] _defaultParsers = { };
        
        private bool MyRemoteCertificateValidationCallback(System.Object sender,
            X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            if (sslPolicyErrors != SslPolicyErrors.None) 
            {
                for (int i=0; i<chain.ChainStatus.Length; i++) 
                {
                    if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown) 
                    {
                        continue;
                    }
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build ((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                        break;
                    }
                }
            }
            return isOk;
        }

        public override void StartDownloading(Action<bool> downloadCallback)
        {
            _downloadCallback = downloadCallback;
           
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                    var myWebClient = new WebClient();
                    var path = FilePath;
					myWebClient.DownloadFileAsync(new Uri(_url), path);
                    myWebClient.DownloadFileCompleted += OnDownloaded;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    OnDownloaded(null, null);
                }
            }
            else
            {
                Debug.LogError("No internet!");
                OnDownloaded(null, null);
            }
        }

        protected virtual ICustomStringParser[] GetCustomParsers()
        {
            return _defaultParsers;
        }
        
        protected abstract void ParseSheet(string[] rows);

        protected string GetSeparator()
        {
            return _url.EndsWith("tsv") ? "\t" : ",";
        }
        
        public void ParseLocalFile()
        {
            if(!File.Exists(FilePath))
                return;
            Debug.Log(File.ReadAllText(FilePath));
            var cvsText = File.ReadAllLines(FilePath).ToArray();
            ParseSheet(cvsText);
            SetFileDirty();
        }

        public void CheckDownload()
        {
            if (Downloaded)
            {
                ParseLocalFile();
            }
        }
        
        public void OnDownloaded(object sender, AsyncCompletedEventArgs e)
        {
            Debug.Log("downloaded config: " + name);
            Downloaded = e != null && e.Error == null;

            if(e?.Error != null)
                Debug.LogError(e.Error);
            
            if (!Application.isEditor || Application.isPlaying)
            {
                Dispatcher.RunOnMainThread(() =>
                {
                    CheckDownload();
                    _downloadCallback(Downloaded);
                    Downloaded = false;
                });
                return;
            }


            if (_downloadCallback != null)
                _downloadCallback.Invoke(Downloaded);
            
            CheckDownload();
            
        }

        protected void SetFileDirty()
        {
#if UNITY_EDITOR
                
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }
}

