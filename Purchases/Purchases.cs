using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSDK.Core;
using GameSDK.Core.Properties;
using Debug = UnityEngine.Debug;

namespace GameSDK.Purchases
{
    public class Purchases
    {
        private static Purchases _instance;
        
        private InitializationStatus _initializationStatus = InitializationStatus.None;

        private Dictionary<PlatformServiceType, IPurchasesApp> _services = new Dictionary<PlatformServiceType, IPurchasesApp>();
        public Dictionary<string, ProductType> _productTypes = new Dictionary<string, ProductType>();
        internal static Purchases Instance => _instance ??= new Purchases();
        
        public static bool IsInitialized => Instance._initializationStatus == InitializationStatus.Initialized;
        
        public static event Action OnInitialized;
        public static event Action OnInitializeError;
        public static event Action<ProductPurchase> OnPurchased;
        public static event Action<ProductPurchase> OnConsumed;
        
        internal void Register(IPurchasesApp app)
        {
            if (_services.ContainsKey(app.PlatformService))
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Purchases]: The platform {app.PlatformService} has already been registered!");
                }

                return;
            }

            _services.Add(app.PlatformService, app);

            if (GameApp.IsDebugMode)
            {
                Debug.Log($"[GameSDK.Purchases]: Platform {app.PlatformService} is registered!");
            }
        }
        
        public static async Task Initialize()
        {
            if (IsInitialized)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning($"[GameSDK.Purchases]: SDK has already been initialized!");
                }
                
                return;
            }
            
            if (GameApp.IsInitialized == false)
            {
                await GameApp.Initialize();
            }
            
            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Purchases]: Before initialize purchases, initialize the sdk\nGameApp.Initialize()!");
                }
                
                return;
            }
            
            _instance._initializationStatus = InitializationStatus.Waiting;

            foreach (var service in _instance._services)
            {
                try
                {
                    await service.Value.Initialize();
                    if (service.Value.InitializationStatus == InitializationStatus.Initialized) continue;
                    
                    _instance._initializationStatus = service.Value.InitializationStatus;
                    return;
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Purchases]: An initialize SDK error has occurred {e.Message}!");
                    }
                    
                    _instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }
            }

            _instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
        }

        public static async Task<(bool, Product[])> GetCatalog()
        {
            if (IsInitialized == false)
            {
                await Initialize();
            }

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Purchases]: Before get catalog, initialize the purchases\nPurchases.Initialize()!");
                }

                return (false, null);
            }

            List<Product> products = new List<Product>();
            var purchasedProducts = (await GetPurchases()).GroupBy(el => el.Id).ToDictionary(el => el.Key, el => el.ToList());

            foreach (var service in _instance._services)
            {
                try
                {
                    var productsService = await service.Value.GetCatalog();

                    if(productsService.Item1 == false) continue;
                    
                    foreach (var product in productsService.Item2)
                    {
                        if (products.Exists(el => el.Id == product.Id)) continue;

                        product.InitializePurchases(_instance);
                        
                        product.Type = ProductType.None;
                        if (_instance._productTypes.TryGetValue(product.Id, out var typeProduct))
                        {
                            product.Type = typeProduct;
                        }

                        if (purchasedProducts.TryGetValue(product.Id, out var value))
                        {
                            product.AddProductPurchases(value);
                        }

                        products.Add(product);
                    }

                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Purchases]: An get catalog error has occurred {e.Message}!");
                    }

                    return (false, null);
                }
            }

            return (products.Count > 0, products.ToArray());
        }

        public static void AddProduct(string id, ProductType productType)
        {
            if (_instance._productTypes.ContainsKey(id))
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Purchases]: The product has already been added!");
                }
                
                return;
            }
            
            _instance._productTypes.Add(id, productType);
        }
        
        public static void RemoveProduct(string id)
        {
            if (_instance._productTypes.ContainsKey(id) == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Purchases]: The product has already been removed!");
                }
                
                return;
            }
            
            _instance._productTypes.Remove(id);
        }
        
        public static async Task<(bool, ProductPurchase)> Purchase(string id, string developerPayload = "")
        {
            if (IsInitialized == false)
            {
                await Initialize();
            }

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Purchases]: Before purchase, initialize the purchases\nPurchases.Initialize()!");
                }

                return (false, null);
            }

            var item = (await GetPurchases()).FirstOrDefault(el => el.Id == id);

            if (item is { Type: ProductType.NonConsumables })
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Purchases]: The item {id} has already been purchased!");
                }

                return (false, null);
            }

            foreach (var service in _instance._services)
            {
                try
                {
                    var result = await service.Value.Purchase(id, developerPayload);
                    
                    if(result.Item1 == false) continue;

                    result.Item2.InitializePurchase(_instance);

                    result.Item2.Type = ProductType.None;
                    
                    if (_instance._productTypes.TryGetValue(result.Item2.Id, out var value))
                    {
                        result.Item2.Type = value;
                    }
                    
                    OnPurchased?.Invoke(result.Item2);
                    
                    return result;

                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Purchases]: An purchase error has occurred {e.Message}!");
                    }

                    return (false, null);
                }
            }

            return (false, null);
        }
        
        public static async Task<bool> Consume(ProductPurchase productPurchase)
        {
            if (IsInitialized == false)
            {
                await Initialize();
            }

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Purchases]: Before consume purchase, initialize the purchases\nPurchases.Initialize()!");
                }

                return false;
            }

            foreach (var service in _instance._services)
            {
                try
                {
                    var result = await service.Value.Consume(productPurchase);
                    
                    if(result == false) return false;

                    productPurchase.SetConsumed(true);

                    OnConsumed?.Invoke(productPurchase);
                    
                    return true;

                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Purchases]: An consume purchase error has occurred {e.Message}!");
                    }

                    return false;
                }
            }

            return false;
        }
        
        public static async Task<ProductPurchase[]> GetPurchases()
        {
            if (IsInitialized == false)
            {
                await Initialize();
            }

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                {
                    Debug.LogWarning(
                        $"[GameSDK.Purchases]: Before get purchases, initialize the purchases\nPurchases.Initialize()!");
                }

                return Array.Empty<ProductPurchase>();
            }

            foreach (var service in _instance._services)
            {
                try
                {
                    var result = await service.Value.GetPurchases();
                    
                    if(result == null || result.Length == 0) continue;

                    foreach (var purchase in result)
                    {
                        purchase.InitializePurchase(_instance);
                        purchase.Type = ProductType.None;
                        
                        if (_instance._productTypes.TryGetValue(purchase.Id, out var value))
                        {
                            purchase.Type = value;
                        }
                    }

                    return result;

                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                    {
                        Debug.LogError($"[GameSDK.Purchases]: An get purchases error has occurred {e.Message}!");
                    }

                    return Array.Empty<ProductPurchase>();

                }
            }

            return Array.Empty<ProductPurchase>();
        }
    }
}