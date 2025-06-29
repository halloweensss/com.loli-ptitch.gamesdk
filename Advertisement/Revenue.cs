using System;
using System.Collections.Generic;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Advertisement
{
    public sealed class Revenue
    {
        private readonly Dictionary<string, IAdRevenueSource> _services = new(2);
        
        public event Action<AdRevenueData> RevenueReceived;
        
        internal Revenue()
        {
            
        }
        
        public void Register(IAdRevenueSource service)
        {
            if (_services.TryAdd(service.ServiceId, service) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement.Revenue]: The platform {service.ServiceId} has already been registered!");

                return;
            }
            
            service.OnAdRevenuePaid += OnRevenueReceived;

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Advertisement.Revenue]: Platform {service.ServiceId} is registered!");
        }

        public void Unregister(IAdRevenueSource service)
        {
            if (_services.Remove(service.ServiceId) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Advertisement.Revenue]: The platform {service.ServiceId} has not been registered!");

                return;
            }
            
            service.OnAdRevenuePaid -= OnRevenueReceived;

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Advertisement.Revenue]: Platform {service.ServiceId} is unregistered!");
        }

        private void OnRevenueReceived(AdRevenueData revenueData)
        {
            if (GameApp.IsDebugMode)
                Debug.Log("[GameSDK.Advertisement.Revenue]: Ad revenue received!");
            
            RevenueReceived?.Invoke(revenueData);
        }
    }
}