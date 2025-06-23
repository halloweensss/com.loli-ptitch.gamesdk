using System;
using System.Runtime.InteropServices;
using AOT;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Plugins.Web.Visibility
{
    internal sealed class WebVisibility
    {
        private static readonly WebVisibility Instance = new();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
#if !UNITY_EDITOR
            GameAppVisibilityHandler(OnVisibilityChanged);
            return;
#endif
            Application.focusChanged += OnVisibilityChanged;
        }

        [MonoPInvokeCallback(typeof(Action<bool>))]
        private static void OnVisibilityChanged(bool isVisible) => GameApp.OnVisibilityChange(isVisible);

        [DllImport("__Internal")]
        private static extern void GameAppVisibilityHandler(Action<bool> onVisibilityChanged);
    }
}