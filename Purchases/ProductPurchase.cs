using System;
using System.Threading.Tasks;
using GameSDK.Core;
using UnityEngine;

namespace GameSDK.Purchases
{
    [Serializable]
    public class ProductPurchase
    {
       public string Id { get; internal set; }
       public string Token { get; internal set; }
       public string Payload { get; internal set; }
       public string Signature { get; internal set; }
       public ProductType Type { get; internal set; }
       public bool IsConsumed { get; internal set; }
       public event Action<ProductPurchase> OnConsumed;

       private Purchases _purchases;
       
       public ProductPurchase(string id, string token, string payload, string signature)
       {
           Id = id;
           Token = token;
           Payload = payload;
           Signature = signature;
       }
       
       internal void InitializePurchase(Purchases purchases)
       {
           _purchases = purchases;
       }

       internal void SetConsumed(bool consumed)
       {
           if (IsConsumed == consumed) return;

           IsConsumed = consumed;

           if (IsConsumed)
           {
               OnConsumed?.Invoke(this);
           }
       }

       public async Task<bool> Consume()
       {
           await Task.CompletedTask;
           
           if (Type is ProductType.NonConsumables or ProductType.None)
           {
               if (GameApp.IsDebugMode)
               {
                   Debug.LogWarning(
                       $"[GameSDK.Purchases]: Product is not consumable!");
               }

               return false;
           }

           var result = await Purchases.Consume(this);

           if (result)
           {
               if (GameApp.IsDebugMode)
               {
                   Debug.Log(
                       $"[GameSDK.Purchases]: Product is consumed {Id} with Token {Token}!");
               }
           }
           else
           {
               if (GameApp.IsDebugMode)
               {
                   Debug.Log(
                       $"[GameSDK.Purchases]: Product is not consumed {Id} with Token {Token}!");
               } 
           }

           return result;
       }
    }
}