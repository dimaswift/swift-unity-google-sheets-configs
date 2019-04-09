using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace SwiftUnityGoogleSheetConfigs.Editor
{
    public static class EditorDispatcher
    {
        static readonly Queue<Action> dispatchQueue = new Queue<Action>();
        static double timeSliceLimit = 10.0;
        static Stopwatch timer;
    
        static EditorDispatcher()
        {
            EditorApplication.update += Update;
            timer = new Stopwatch();
        }
        
        static void Update ()
        {
            lock (dispatchQueue)
            {
                var dispatchCount = 0;
    
                timer.Reset();
                timer.Start();
    
                while (dispatchQueue.Count > 0 && (timer.Elapsed.TotalMilliseconds <= timeSliceLimit))
                {
                    dispatchQueue.Dequeue().Invoke();
    
                    dispatchCount++;
                }
    
                timer.Stop();
            }
        }
    

        public static AsyncDispatch Dispatch(Action task)
        {
            lock (dispatchQueue)
            {
                AsyncDispatch dispatch = new AsyncDispatch();
                
              
                dispatchQueue.Enqueue(() => { task(); dispatch.FinishedDispatch(); }); 
    
                return dispatch;
            }
        }

        public static AsyncDispatch Dispatch(IEnumerator task, bool showUI = false)
        {
            lock (dispatchQueue)
            {
                AsyncDispatch dispatch = new AsyncDispatch();
    
                dispatchQueue.Enqueue(() => 
                {
                    if (showUI)
                    {
                        EditorCoroutineRunner.StartCoroutineWithUI(DispatchCorotine(task, dispatch), "Dispatcher task", false);
                    }
                    else
                    {
                        EditorCoroutineRunner.StartCoroutine(task);
                    }
                });
    
                return dispatch;
            }
        }
    
        static IEnumerator DispatchCorotine(IEnumerator dispatched, AsyncDispatch tracker)
        {
            yield return dispatched;
            tracker.FinishedDispatch();
        }
    }

    public class AsyncDispatch : CustomYieldInstruction
    {
        public bool IsDone { get; private set; }
        public override bool keepWaiting { get { return !IsDone; } }
    

        internal void FinishedDispatch()
        {
            IsDone = true;
        }
    }
    
}
