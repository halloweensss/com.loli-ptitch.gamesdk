using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameSDK.Purchases
{
    [Serializable]
    public class Product : IDisposable
    {
        public string Id;
        public string Title;
        public string Description;
        public string Price;
        public string PriceValue;
        public string PriceCurrencyCode;
        public ProductType Type { get; internal set; }
        public bool IsPurchased => _productPurchased is { Count: > 0 };

        private Purchases _purchases;
        private List<ProductPurchase> _productPurchased = new List<ProductPurchase>();
        
        internal void InitializePurchases(Purchases purchases)
        {
            _purchases = purchases;
            Purchases.OnConsumed += OnConsumed;
            Purchases.OnPurchased += OnPurchased;
        }

        private void OnPurchased(ProductPurchase productPurchase)
        {
            var product = _productPurchased.FirstOrDefault(el => el.Token == productPurchase.Token);

            if (product != null) return;
            
            _productPurchased.Add(productPurchase);
        }

        private void OnConsumed(ProductPurchase productPurchase)
        {
            var product = _productPurchased.FirstOrDefault(el => el.Token == productPurchase.Token);

            if (product != null)
            {
                _productPurchased.Remove(product);
            }
        }

        internal void AddProductPurchase(ProductPurchase productPurchase)
        {
            _productPurchased.Add(productPurchase);
        }
        internal void AddProductPurchases(IEnumerable<ProductPurchase> productPurchases)
        {
            _productPurchased.AddRange(productPurchases);
        }

        public async Task<bool> Consume()
        {
            if (IsPurchased == false) return true;
            
            for (int i = 0; i < _productPurchased.Count; i++)
            {
                var product = _productPurchased[i];

                var result = true;
                if (product.IsConsumed == false)
                {
                    result = await product.Consume();
                }

                if (result == false) return false;

                _productPurchased.Remove(product);
                
                i--;
            }

            return true;
        }

        public void Dispose()
        {
            if (_purchases != null)
            {
                Purchases.OnConsumed -= OnConsumed;
                Purchases.OnPurchased -= OnPurchased;
            }
        }
    }
}