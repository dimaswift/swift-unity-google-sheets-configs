using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SwiftUnityGoogleSheetConfigs.Editor
{
    public class EditorCoroutineRunner
    {    
        public static void KillAllCoroutines()
        {
            EditorUtility.ClearProgressBar();
            _uiCoroutineState = null;
            _coroutineStates.Clear();
            _finishedThisUpdate.Clear();
        }
    
        static List<EditorCoroutineState> _coroutineStates;
        static List<EditorCoroutineState> _finishedThisUpdate;
        static EditorCoroutineState _uiCoroutineState;
    
       
        public static EditorCoroutine StartCoroutine(IEnumerator coroutine)
        {
            return StoreCoroutine(new EditorCoroutineState(coroutine));
        }
    
        public static EditorCoroutine StartCoroutineWithUI(IEnumerator coroutine, string title, bool isCancelable = false)
        {
            if (_uiCoroutineState != null)
            {
                Debug.LogError("EditorCoroutineRunner only supports running one coroutine that draws a GUI! [" + title + "]");
                return null;
            }
            EditorCoroutineRunner._uiCoroutineState = new EditorCoroutineState(coroutine, title, isCancelable);
            return StoreCoroutine(_uiCoroutineState);
        }

        
        static EditorCoroutine StoreCoroutine(EditorCoroutineState state)
        {
            if (_coroutineStates == null)
            {
                _coroutineStates = new List<EditorCoroutineState>();
                _finishedThisUpdate = new List<EditorCoroutineState>();
            }
    
            if (_coroutineStates.Count == 0)
                EditorApplication.update += Runner;
    
            _coroutineStates.Add(state);
    
            return state.EditorCoroutineYieldInstruction;
        }
    
        public static void UpdateUILabel(string label)
        {
            if (_uiCoroutineState != null && _uiCoroutineState.ShowUi)
            {
                _uiCoroutineState.Label = label; 
            }
        }
    

        public static void UpdateUIProgressBar(float percent)
        {
            if (_uiCoroutineState != null && _uiCoroutineState.ShowUi)
            {
                _uiCoroutineState.PercentComplete = percent;
            }
        }
    
        public static void UpdateUI(string label, float percent)
        {
            if (_uiCoroutineState != null && _uiCoroutineState.ShowUi)
            {
                _uiCoroutineState.Label = label ;
                _uiCoroutineState.PercentComplete = percent;
            }
        }

        static void Runner()
        {
            for (int i = 0; i < _coroutineStates.Count; i++)
            {
                TickState(_coroutineStates[i]);
            }
    
            for (int i = 0; i < _finishedThisUpdate.Count; i++)
            {
                _coroutineStates.Remove(_finishedThisUpdate[i]);
    
                if (_uiCoroutineState == _finishedThisUpdate[i])
                {
                    _uiCoroutineState = null;
                    EditorUtility.ClearProgressBar();
                }
            }
            _finishedThisUpdate.Clear();
    
            if (_coroutineStates.Count == 0)
            {
                EditorApplication.update -= Runner;
            }
        }
        
        static void TickState(EditorCoroutineState state)
        {
            if (state.IsValid)
            {
                state.Tick();
    
                if (state.ShowUi && _uiCoroutineState == state)
                {
                    _uiCoroutineState.UpdateUI();
                }
            }
            else
            {
                _finishedThisUpdate.Add(state);
            }
        }

        static bool KillCoroutine(ref EditorCoroutine coroutine, ref List<EditorCoroutineState> states)
        {
            foreach (EditorCoroutineState state in states)
            {
                if (state.EditorCoroutineYieldInstruction == coroutine) 
                {
                    states.Remove (state);
                    coroutine = null;
                    return true;
                }
            }
            return false;
        }
    
        public static void KillCoroutine(ref EditorCoroutine coroutine)
        {
            if (_uiCoroutineState.EditorCoroutineYieldInstruction == coroutine)
            {
                _uiCoroutineState = null;
                coroutine = null;
                EditorUtility.ClearProgressBar ();
                return;
            }
            if (KillCoroutine (ref coroutine, ref _coroutineStates))
                return;
    
            if (KillCoroutine (ref coroutine, ref _finishedThisUpdate))
                return;
        }   
    }
    
    class EditorCoroutineState
    {
        public bool IsValid
        {
            get { return _coroutine != null; }
        }
        public readonly EditorCoroutine EditorCoroutineYieldInstruction;
        public readonly bool ShowUi;
        public string Label;
        public float PercentComplete;

        object _current;
        Type _currentType;
        float _timer; 
        IEnumerator _coroutine;
        EditorCoroutine _nestedCoroutine; 
        DateTime _lastUpdateTime;
        bool _canceled;
        readonly string _title;
        readonly bool _cancelable;
        
        public EditorCoroutineState(IEnumerator coroutine)
        {
            this._coroutine = coroutine;
            EditorCoroutineYieldInstruction = new EditorCoroutine();
            ShowUi = false;
            _lastUpdateTime = DateTime.Now;
        }
    
        public EditorCoroutineState(IEnumerator coroutine, string title, bool isCancelable)
        {
            _coroutine = coroutine;
            EditorCoroutineYieldInstruction = new EditorCoroutine();
            ShowUi = true;
            _cancelable = isCancelable;
            _title = title;
            Label = "initializing....";
            PercentComplete = 0.0f;
    
            _lastUpdateTime = DateTime.Now;
        }
    
        public void Tick()
        {
            if (_coroutine != null)
            {
              
                if (_canceled)
                {
                    Stop();
                    return;
                }

                bool isWaiting = false;
                var now = DateTime.Now;
                if (_current != null) 
                {
                    if (_currentType == typeof(WaitForSeconds))
                    {
                        var delta = now - _lastUpdateTime;
                        _timer -= (float)delta.TotalSeconds;
    
                        if (_timer > 0.0f)
                        {
                            isWaiting = true;
                        }
                    }
                    else if (_currentType == typeof(WaitForEndOfFrame) || _currentType == typeof(WaitForFixedUpdate))
                    {
                        isWaiting = false;
                    }
                    else if (_currentType == typeof(WWW))
                    {
                        var www = _current as WWW;
                        if (!www.isDone)
                        {
                            isWaiting = true;
                        }
                    }
                    else if (_currentType.IsSubclassOf(typeof(CustomYieldInstruction)))
                    {
                        var yieldInstruction = _current as CustomYieldInstruction;
                        if (yieldInstruction.keepWaiting)
                        {
                            isWaiting = true;
                        }
                    }
                    else if (_currentType == typeof(EditorCoroutine))
                    {
                        var editorCoroutine = _current as EditorCoroutine;
                        if (!editorCoroutine.HasFinished)
                        {
                            isWaiting = true;
                        }
                    }
                    else if (typeof(IEnumerator).IsAssignableFrom(_currentType))
                    {
                        if (_nestedCoroutine == null)
                        {
                            _nestedCoroutine = EditorCoroutineRunner.StartCoroutine(_current as IEnumerator);
                            isWaiting = true;
                        }
                        else
                        {
                            isWaiting = !_nestedCoroutine.HasFinished;
                        }
    
                    }
                    else if (_currentType == typeof(Coroutine))
                    {
                        Debug.LogError("Nested Coroutines started by Unity's defaut StartCoroutine method are not supported in editor! please use EditorCoroutineRunner.Start instead. Canceling.");
                        _canceled = true;
                    } 
                    else
                    {
                        Debug.LogError("Unsupported yield (" + _currentType + ") in editor coroutine!! Canceling.");
                        _canceled = true;
                    }
                }
                _lastUpdateTime = now;

                if (_canceled)
                {
                    Stop();
                    return;
                }
    
                if (!isWaiting)
                {
                    bool update = _coroutine.MoveNext();
    
                    if (update)
                    {
                        _current = _coroutine.Current;
                        if (_current != null)
                        {
                            _currentType = _current.GetType();
    
                            if (_currentType == typeof(WaitForSeconds))
                            {
                                var wait = _current as WaitForSeconds;
                                FieldInfo m_Seconds = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (m_Seconds != null)
                                {
                                    _timer = (float)m_Seconds.GetValue(wait);
                                }
                            }
                            else if (_currentType == typeof(EditorStatusUpdate))
                            {
                                var updateInfo = _current as EditorStatusUpdate;
                                if (updateInfo.HasLabelUpdate)
                                {
                                    Label = updateInfo.Label;
                                }
                                if (updateInfo.HasPercentUpdate)
                                {
                                    PercentComplete = updateInfo.PercentComplete;
                                }
                            }
                        }
                    }
                    else
                    {
                        Stop();
                    }
                }
            }
        }
    
        void Stop()
        {
            _coroutine = null;
            EditorCoroutineYieldInstruction.HasFinished = true;
        }
    
        public void UpdateUI()
        {
            if (_cancelable)
            {
                _canceled = EditorUtility.DisplayCancelableProgressBar(_title, Label, PercentComplete);
                if (_canceled)
                    Debug.Log("CANCLED");
            }
            else
            {
                EditorUtility.DisplayProgressBar(_title, Label, PercentComplete);
            }
        }
    }

    public class EditorStatusUpdate : CustomYieldInstruction
    {
        public readonly string Label;
        public readonly float PercentComplete;
    
        public readonly bool HasLabelUpdate;
        public readonly bool HasPercentUpdate;
    
        public override bool keepWaiting
        {
            get
            {
                return false;
            }
        }

        public EditorStatusUpdate(string label, float percent)
        {
            HasPercentUpdate = true;
            PercentComplete = percent;
    
            HasLabelUpdate = true;
            Label = label;
        }
    }
    
    public class EditorCoroutine : YieldInstruction
    {
        public bool HasFinished;
    }
}
