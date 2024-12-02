using System;
using UnityEngine;

namespace GameSDK.Plugins.YaMetricaWeb
{
    [Serializable]
    public class MetricaParameters
    {
        [SerializeField] private bool accurateTrackBounce = true;
        [SerializeField] private bool childIframe = false;
        [SerializeField] private bool clickmap = true;
        [SerializeField] private bool defer = false;
        [SerializeField] private bool trackHash = false;
        [SerializeField] private bool trackLinks = true;
        [SerializeField] private bool webvisor = false;
        [SerializeField] private bool triggerEvent = false;
        [SerializeField] private bool sendTitle = true;
        
        internal bool AccurateTrackBounce => accurateTrackBounce;
        internal bool ChildIframe => childIframe;
        internal bool Clickmap => clickmap;
        internal bool Defer => defer;
        internal bool TrackHash => trackHash;
        internal bool TrackLinks => trackLinks;
        internal bool Webvisor => webvisor;
        internal bool TriggerEvent => triggerEvent;
        internal bool SendTitle => sendTitle;
    }
}