using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSDK.Core;
using Debug = UnityEngine.Debug;

namespace GameSDK.Purchases
{
    public class Purchases : IGameService
    {
        private static readonly Purchases Instance = new();
        private readonly Dictionary<string, ProductType> _productTypes = new();

        private readonly Dictionary<string, IPurchasesApp> _services = new();

        private InitializationStatus _initializationStatus = InitializationStatus.None;

        public static bool IsInitialized => Instance._initializationStatus == InitializationStatus.Initialized;

        public string ServiceName => "Purchases";

        public static event Action OnInitialized;
        public static event Action OnInitializeError;
        public static event Action<ProductPurchase> OnPurchased;
        public static event Action<ProductPurchase> OnConsumed;

        public static void Register(IPurchasesApp app)
        {
            Instance.RegisterInternal(app);
        }

        private void RegisterInternal(IPurchasesApp app)
        {
            if (_services.TryAdd(app.ServiceId, app) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning($"[GameSDK.Purchases]: The platform {app.ServiceId} has already been registered!");

                return;
            }

            if (GameApp.IsDebugMode)
                Debug.Log($"[GameSDK.Purchases]: Platform {app.ServiceId} is registered!");
        }

        public static async Task Initialize()
        {
            if (IsInitialized)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning("[GameSDK.Purchases]: SDK has already been initialized!");

                return;
            }

            if (GameApp.IsInitialized == false)
                await GameApp.Initialize();

            if (GameApp.IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Purchases]: Before initialize purchases, initialize the sdk\nGameApp.Initialize()!");

                return;
            }

            Instance._initializationStatus = InitializationStatus.Waiting;

            foreach (var service in Instance._services)
                try
                {
                    await service.Value.Initialize();
                    if (service.Value.InitializationStatus == InitializationStatus.Initialized) continue;

                    Instance._initializationStatus = service.Value.InitializationStatus;
                    return;
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Purchases]: An initialize SDK error has occurred {e.Message}!");

                    Instance._initializationStatus = InitializationStatus.Error;
                    OnInitializeError?.Invoke();
                    return;
                }

            Instance._initializationStatus = InitializationStatus.Initialized;
            OnInitialized?.Invoke();
        }

        public static async Task<(bool, Product[])> GetCatalog()
        {
            if (IsInitialized == false)
                await Initialize();

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Purchases]: Before get catalog, initialize the purchases\nPurchases.Initialize()!");

                return (false, null);
            }

            var products = new List<Product>();
            var purchasedProducts = (await GetPurchases()).GroupBy(el => el.Id)
                .ToDictionary(el => el.Key, el => el.ToList());

            foreach (var service in Instance._services)
                try
                {
                    var productsService = await service.Value.GetCatalog();

                    if (productsService.Item1 == false) continue;

                    foreach (var product in productsService.Item2)
                    {
                        if (products.Exists(el => el.Id == product.Id)) continue;

                        product.InitializePurchases(Instance);

                        product.Type = ProductType.None;
                        if (Instance._productTypes.TryGetValue(product.Id, out var typeProduct))
                            product.Type = typeProduct;

                        if (purchasedProducts.TryGetValue(product.Id, out var value))
                            product.AddProductPurchases(value);

                        products.Add(product);
                    }
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Purchases]: An get catalog error has occurred {e.Message}!");

                    return (false, null);
                }

            return (products.Count > 0, products.ToArray());
        }

        public static void AddProduct(string id, ProductType productType)
        {
            if (Instance._productTypes.ContainsKey(id))
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Purchases]: The product has already been added!");

                return;
            }

            Instance._productTypes.Add(id, productType);
        }

        public static void RemoveProduct(string id)
        {
            if (Instance._productTypes.ContainsKey(id) == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Purchases]: The product has already been removed!");

                return;
            }

            Instance._productTypes.Remove(id);
        }

        public static async Task<(bool, ProductPurchase)> Purchase(string id, string developerPayload = "")
        {
            if (IsInitialized == false)
                await Initialize();

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Purchases]: Before purchase, initialize the purchases\nPurchases.Initialize()!");

                return (false, null);
            }

            var item = (await GetPurchases()).FirstOrDefault(el => el.Id == id);

            if (item is { Type: ProductType.NonConsumables })
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        $"[GameSDK.Purchases]: The item {id} has already been purchased!");

                return (false, null);
            }

            foreach (var service in Instance._services)
                try
                {
                    var result = await service.Value.Purchase(id, developerPayload);

                    if (result.Item1 == false) continue;

                    result.Item2.InitializePurchase(Instance);

                    result.Item2.Type = ProductType.None;

                    if (Instance._productTypes.TryGetValue(result.Item2.Id, out var value))
                        result.Item2.Type = value;

                    OnPurchased?.Invoke(result.Item2);

                    return result;
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Purchases]: An purchase error has occurred {e.Message}!");

                    return (false, null);
                }

            return (false, null);
        }

        public static async Task<bool> Consume(ProductPurchase productPurchase)
        {
            if (IsInitialized == false)
                await Initialize();

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Purchases]: Before consume purchase, initialize the purchases\nPurchases.Initialize()!");

                return false;
            }

            foreach (var service in Instance._services)
                try
                {
                    var result = await service.Value.Consume(productPurchase);

                    if (result == false) return false;

                    productPurchase.SetConsumed(true);

                    OnConsumed?.Invoke(productPurchase);

                    return true;
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Purchases]: An consume purchase error has occurred {e.Message}!");

                    return false;
                }

            return false;
        }

        public static async Task<ProductPurchase[]> GetPurchases()
        {
            if (IsInitialized == false)
                await Initialize();

            if (IsInitialized == false)
            {
                if (GameApp.IsDebugMode)
                    Debug.LogWarning(
                        "[GameSDK.Purchases]: Before get purchases, initialize the purchases\nPurchases.Initialize()!");

                return Array.Empty<ProductPurchase>();
            }

            foreach (var service in Instance._services)
                try
                {
                    var result = await service.Value.GetPurchases();

                    if (result == null || result.Length == 0) continue;

                    foreach (var purchase in result)
                    {
                        purchase.InitializePurchase(Instance);
                        purchase.Type = ProductType.None;

                        if (Instance._productTypes.TryGetValue(purchase.Id, out var value))
                            purchase.Type = value;
                    }

                    return result;
                }
                catch (Exception e)
                {
                    if (GameApp.IsDebugMode)
                        Debug.LogError($"[GameSDK.Purchases]: An get purchases error has occurred {e.Message}!");

                    return Array.Empty<ProductPurchase>();
                }

            return Array.Empty<ProductPurchase>();
        }
    }
}