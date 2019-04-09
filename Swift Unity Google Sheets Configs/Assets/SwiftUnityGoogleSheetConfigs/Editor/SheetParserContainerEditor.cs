using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SwiftUnityGoogleSheetConfigs.Editor
{
	[CustomEditor(typeof(SheetParserContainer), true)]
	public class SheetParserContainerEditor : UnityEditor.Editor
	{
		DateTime _downloadTime;
		DateTime _downloadStartTime;
		bool _downloadedSuccessfully;
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var d = target as SheetParserContainer;
			GUI.enabled = !d.IsDownloading;
			if (GUILayout.Button(d.IsDownloading ? "Downloading..." : "Download"))
			{
				d.StartDownloading(OnDownloaded);
				d.IsDownloading = true;
				_downloadStartTime = DateTime.Now;
			}

			if (!d.IsDownloading)
			{
				GUI.enabled = true;
				if ((DateTime.Now - _downloadTime).TotalSeconds < 3)
				{
					if (_downloadedSuccessfully)
						EditorGUILayout.HelpBox("Successfully downloaded!", MessageType.Info);
					else EditorGUILayout.HelpBox("Error while downloading!", MessageType.Error);
				}
			}
			else
			{
				if ((DateTime.Now - _downloadStartTime).TotalSeconds > 10)
				{
					OnDownloaded(false);
				}
			}
			
		}

		void OnDownloaded(bool succ)
		{
			EditorDispatcher.Dispatch(() => 
			{
				var d = target as SheetParserContainer;
				d.IsDownloading = false;
				_downloadTime = DateTime.Now; 
				_downloadedSuccessfully = succ;
			});
		}
	}

}
