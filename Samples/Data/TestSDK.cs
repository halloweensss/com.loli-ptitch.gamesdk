using System;
using System.Collections.Generic;
using System.Globalization;
using GameSDK.Advertisement;
using GameSDK.Authentication;
using GameSDK.Core;
using GameSDK.GameFeedback;
using GameSDK.GameStorage;
using GameSDK.Leaderboard;
using GameSDK.Localization;
using GameSDK.Purchases;
using GameSDK.RemoteConfigs;
using GameSDK.Shortcut;
using TMPro;
using UnityEngine;
using Time = GameSDK.Time.Time;

namespace Test
{
    public class TestSDK : MonoBehaviour
    {
        [SerializeField] private TMP_Text _deviceType;
        [SerializeField] private TMP_Text _lang;
        [SerializeField] private TMP_Text _appId;
        [SerializeField] private TMP_Text _userId;
        [SerializeField] private TMP_Text _userName;
        [SerializeField] private TMP_Text _canReviewStatus;
        [SerializeField] private TMP_Text _requestReviewStatus;
        [SerializeField] private TMP_Text _canCreateShortcut;
        [SerializeField] private TMP_Text _leaderboardInitialized;
        [SerializeField] private TMP_Text _leaderboardName;
        [SerializeField] private TMP_Text _leaderboardTitleEN;
        [SerializeField] private TMP_Text _leaderboardTitleRU;
        [SerializeField] private TMP_Text _leaderboardPlayerName;
        [SerializeField] private TMP_Text _leaderboardPlayerScore;
        [SerializeField] private TMP_Text _leaderboardPlayerRank;
        [SerializeField] private TMP_Text _purchasesInitialized;
        [SerializeField] private TMP_Text _purchasesCoins;
        [SerializeField] private TMP_Text _purchasesNoAds;
        [SerializeField] private TMP_Text _authStatus;
        [SerializeField] private TMP_Text _timestamp;
        [SerializeField] private TMP_Text _localTime;
        [SerializeField] private TMP_Text _time;
        [SerializeField] private TMP_Text _visibleStatus;
        [SerializeField] private TMP_Text _startedText;

        [SerializeField] private RectTransform _leaderboardContent;
        [SerializeField] private LeaderboardEntityElement _prefabLeaderboardElement;
        
        [SerializeField] private RectTransform _purchaseContent;
        [SerializeField] private PurchaseEntityElement _prefabPurchaseElement;

        [SerializeField] private TMP_InputField _inputFieldKey; 
        [SerializeField] private TMP_InputField _inputFieldValue;
        [SerializeField] private TMP_InputField _leaderboardId;
        [SerializeField] private TMP_InputField _leaderboardScore;

        [SerializeField] private List<LocalizationDatabase> _databasesLocalizations = new List<LocalizationDatabase>();  
        [SerializeField] private List<TMP_InputField> _inputFieldsKeysSave = new List<TMP_InputField>();  
        [SerializeField] private List<TMP_InputField> _inputFieldsValuesSave = new List<TMP_InputField>();

        [SerializeField] private TMP_InputField _inputFieldsFlags;

        [SerializeField] private TMP_InputField _inputFieldRemoteUserPropertiesId;
        [SerializeField] private TMP_InputField _inputFieldRemoteUserPropertiesValue;

        private List<LeaderboardEntityElement> _leaderboardEntities = new();
        private List<PurchaseEntityElement> _purchasesEntities = new();

        private int _coins = 0;
        private bool _noAds = false;

        private void Awake()
        {
            GameApp.OnInitialized += OnGameAppInitialized;
            GameApp.OnVisibilityChanged += OnVisibilityChanged;
            GameApp.OnStartChanged += OnStartChanged;
            Auth.OnSignIn += OnSignIn;
            Leaderboard.OnInitialized += OnLeaderboardInitialized;
            Purchases.OnInitialized += OnPurchasesInitialized;
            
            RemoteConfigs.SetDefaultValue("4", 4);
            RemoteConfigs.SetDefaultValue("5", "5");
            RemoteConfigs.SetDefaultValue("6", true);
            
            SubscribeInterstitial();
            SubscribeRewarded();
            SubscribeBanner();

            _purchasesCoins.text = _coins.ToString();
            _purchasesNoAds.text = _noAds.ToString();
            
            Purchases.AddProduct("test_pack_1", ProductType.Consumable);
            Purchases.AddProduct("test_pack_2", ProductType.NonConsumables);

            foreach (var localization in _databasesLocalizations)
            {
                Localization.AddDatabase(localization);
            }
            
            void OnGameAppInitialized()
            {
                GameApp.OnInitialized -= OnGameAppInitialized;
                InitializeUI();
            }
            
            void OnLeaderboardInitialized()
            {
                Leaderboard.OnInitialized -= OnLeaderboardInitialized;
                _leaderboardInitialized.text = "yes";
            }
            
            void OnPurchasesInitialized()
            {
                Purchases.OnInitialized -= OnPurchasesInitialized;
                _purchasesInitialized.text = "yes";
            }
        }

        private void OnStartChanged(bool status)
        {
            _startedText.text = GameApp.IsStarted.ToString();
        }

        private void OnVisibilityChanged(bool status) => 
            _visibleStatus.text = GameApp.IsVisible.ToString();

        private void SubscribeInterstitial()
        {
            Ads.Interstitial.OnShowed += OnShowed;
            Ads.Interstitial.OnClosed += OnClosed;
            Ads.Interstitial.OnError += OnError;
            Ads.Interstitial.OnClicked += OnClicked;
            Ads.Interstitial.OnShowFailed += OnShowFailed;

            void OnShowed()
            {
                Debug.Log("On Interstitial Showed");
            }
            
            void OnClosed()
            {
                Debug.Log("On Interstitial Closed");
            }
            
            void OnError()
            {
                Debug.Log("On Interstitial Error");
            }
            
            void OnClicked()
            {
                Debug.Log("On Interstitial Clicked");
            }
            
            void OnShowFailed()
            {
                Debug.Log("On Interstitial Show Failed");
            }
        }
        
        private void SubscribeRewarded()
        {
            Ads.Rewarded.OnShowed += OnShowed;
            Ads.Rewarded.OnClosed += OnClosed;
            Ads.Rewarded.OnError += OnError;
            Ads.Rewarded.OnClicked += OnClicked;
            Ads.Rewarded.OnRewarded += OnRewarded;

            void OnShowed()
            {
                Debug.Log("On Rewarded Showed");
            }
            
            void OnClosed()
            {
                Debug.Log("On Rewarded Closed");
            }
            
            void OnError()
            {
                Debug.Log("On Rewarded Error");
            }
            
            void OnClicked()
            {
                Debug.Log("On Rewarded Clicked");
            }
            
            void OnRewarded()
            {
                Debug.Log("On Rewarded Reward");
            }
        }
        
        private void SubscribeBanner()
        {
            Ads.Banner.OnShowed += OnShowed;
            Ads.Banner.OnHidden += OnHidden;
            Ads.Banner.OnError += OnError;

            void OnShowed()
            {
                Debug.Log("On Banner Showed");
            }
            
            void OnHidden()
            {
                Debug.Log("On Banner Closed");
            }
            
            void OnError()
            {
                Debug.Log("On Banner Error");
            }
        }

        private void OnDestroy()
        {
            Auth.OnSignIn -= OnSignIn;
            GameApp.OnVisibilityChanged -= OnVisibilityChanged;
        }

        void OnSignIn(SignInType obj)
        {
            InitializeUISignIn();
        }

        public async void Initialize()
        {
            await GameApp.Initialize();
        }
        
        public async void GameReady()
        {
            await GameApp.GameReady();
        }
        
        public async void GameStart()
        {
            await GameApp.Start();
        }

        public async void GameStop()
        {
            await GameApp.Stop();
        }

        private void InitializeUI()
        {
            Debug.Log("[Test]: Initialize UI!");
            _deviceType.text = GameApp.DeviceType.ToString();
            _lang.text = GameApp.Lang;
            _appId.text = GameApp.AppId;
            _visibleStatus.text = GameApp.IsVisible.ToString();
            _startedText.text = GameApp.IsStarted.ToString();
        }
        
        private void InitializeUISignIn()
        {
            Debug.Log("[Test]: Initialize Sign In UI!");
            _userId.text = Auth.Id;
            _userName.text = Auth.Name;
            _authStatus.text = Auth.SignInType.ToString();
        }
        
        public async void SignIn()
        {
            await Auth.SignIn();
        }
        
        public async void SignInGuest()
        {
            await Auth.SignInAsGuest();
        }

        public async void Save()
        {
            var key = _inputFieldKey.text;
            var value = _inputFieldValue.text;
            await Storage.Save(key, value);
        }

        public async void Load()
        {
            var key = _inputFieldKey.text;
            var data = await Storage.Load(key);
            
            _inputFieldValue.text = data.Item1 == StorageStatus.Success ? data.Item2 : "Fail Loading";
        }

        public async void PocketSave()
        {
            for (int i = 0; i < _inputFieldsKeysSave.Count; i++)
            {
                var key = _inputFieldsKeysSave[i].text;
                var value = _inputFieldsValuesSave[i].text;
                
                if(string.IsNullOrEmpty(key))
                    continue;
                
                await Storage.Save(key, value);
            }
        }
        
        public async void PocketLoad()
        {
            for (int i = 0; i < _inputFieldsKeysSave.Count; i++)
            {
                var key = _inputFieldsKeysSave[i].text;
                var data = await Storage.Load(key);
                _inputFieldsValuesSave[i].text = data.Item1 == StorageStatus.Success ? data.Item2 : "Fail Loading";
            }
        }

        public async void CanReview()
        {
            var status = await Feedback.CanReview();

            _canReviewStatus.text = $"{status.Item1} [{status.Item2}]";
        }
        
        public async void RequestReview()
        {
            var status = await Feedback.RequestReview();

            _requestReviewStatus.text = $"{status.Item1} [{status.Item2}]";
        }

        public void ShowInterstitial()
        {
            Ads.Interstitial.Show();
        }
        
        public void ShowRewarded()
        {
            Ads.Rewarded.Show();
        }
        
        public void ShowBanner()
        {
            Ads.Banner.Show();
        }
        
        public void HideBanner()
        {
            Ads.Banner.Hide();
        }
        
        public async void CreateShortcut()
        {
            await Shortcut.Create();
        }
        public async void CanCreateShortcut()
        {
            _canCreateShortcut.text = (await Shortcut.CanCreate()).ToString();
        }
        
        public async void InitializeLeaderboard()
        {
            await Leaderboard.Initialize();
        }
        
        public async void GetLeaderboardDescription()
        {
            var id = _leaderboardId.text;
            var description = await Leaderboard.GetDescription(id);
            _leaderboardName.text = description.Name;
            _leaderboardTitleEN.text = description.Title.EN;
            _leaderboardTitleRU.text = description.Title.RU;
        }
        
        public async void SetScoreLeaderboard()
        {
            var score = Convert.ToInt32(_leaderboardScore.text);
            var id = _leaderboardId.text;
            await Leaderboard.SetScore(id, score);
        }
        
        public async void GetLeaderboardPlayerData()
        {
            var result = await Leaderboard.GetPlayerData(_leaderboardId.text);

            if (result.Item1 == false) return;
            
            _leaderboardPlayerName.text = result.Item2.Name;
            _leaderboardPlayerRank.text = result.Item2.Rank.ToString();
            _leaderboardPlayerScore.text = result.Item2.Score.ToString();
        }

        public async void GetLeaderboardEntries()
        {
            var parameters = new LeaderboardParameters()
            {
                id = _leaderboardId.text,
                includeUser = true,
                quantityAround = 5,
                quantityTop = 5
            };

            var result = await Leaderboard.GetEntries(parameters);
            
            if(result.Item1 == false) return;

            GenerateLeaderboard(result.Item2);

            void GenerateLeaderboard(LeaderboardEntries entries)
            {
                if (_leaderboardEntities.Count > 0)
                {
                    foreach (var entity in _leaderboardEntities)
                    {
                        Destroy(entity.gameObject);
                    }
                    
                    _leaderboardEntities.Clear();
                }
                
                foreach (var entry in entries.Entries)
                {
                    var entity = Instantiate(_prefabLeaderboardElement, _leaderboardContent);
                    entity.Initialize(entry.Name, entry.Rank, entry.Score);
                    _leaderboardEntities.Add(entity);
                }
            }
        }
        
        public async void InitializePurchases()
        {
            await Purchases.Initialize();
        }
        
        public async void GetCatalogPurchases()
        {
            var result = await Purchases.GetCatalog();

            if (result.Item1 == false) return;
            
            GenerateCatalog(result.Item2);

            foreach (var item in result.Item2)
            {
                if (item.IsPurchased)
                {
                    await item.Consume();
                }
            }

            void GenerateCatalog(Product[] products)
            {
                if (_purchasesEntities.Count > 0)
                {
                    foreach (var entity in _purchasesEntities)
                    {
                        entity.OnBuyClick -= PurchaseItem;
                        Destroy(entity.gameObject);
                    }
                    
                    _purchasesEntities.Clear();
                }
                
                foreach (var product in products)
                {
                    var entity = Instantiate(_prefabPurchaseElement, _purchaseContent);
                    entity.Initialize(product.Id, product.Title, product.Price);
                    entity.OnBuyClick += PurchaseItem;

                    if (product.Type == ProductType.NonConsumables && product.IsPurchased)
                    {
                        entity.SetActiveButton(false);
                        
                        if (product.Id.Equals("test_pack_2"))
                        {
                            _noAds = true;
                            _purchasesNoAds.text = _noAds.ToString();
                        }
                    }

                    _purchasesEntities.Add(entity);
                }
            }

            async void PurchaseItem(PurchaseEntityElement element)
            {
                var (result, purchaseItem) = await Purchases.Purchase(element.ID);

                if (result == false) return;

                if (purchaseItem.Id.Equals("test_pack_1"))
                {
                    _coins += 100;
                    await purchaseItem.Consume();
                    
                    _purchasesCoins.text = _coins.ToString();
                    return;
                }

                if (purchaseItem.Id.Equals("test_pack_2"))
                {
                    _noAds = true;
                    _purchasesNoAds.text = _noAds.ToString();
                    element.SetActiveButton(false);
                    return;
                }
            }
        }

        public async void GetPurchases()
        {
            var purchases = await Purchases.GetPurchases();

            foreach (var purchase in purchases)
            {
                Debug.Log($"[Purchase]: Purchased product {purchase.Id}");
            }
        }

        public async void InitializeRemoteConfigs()
        {
            await RemoteConfigs.Initialize();

            string values = string.Empty;
            
            foreach (var (key, value) in RemoteConfigs.RemoteValues)
            {
                values += $"{key}: {value} | {value.Source}\n";
            }

            _inputFieldsFlags.text = values;
        }
        
        public async void InitializeRemoteConfigsWithParameters()
        {
            if(string.IsNullOrEmpty(_inputFieldRemoteUserPropertiesId.text) || string.IsNullOrEmpty(_inputFieldRemoteUserPropertiesValue.text))
                return;
            
            await RemoteConfigs.InitializeWithUserParameters(new KeyValuePair<string, string>(_inputFieldRemoteUserPropertiesId.text, _inputFieldRemoteUserPropertiesValue.text));

            string values = string.Empty;
            
            foreach (var (key, value) in RemoteConfigs.RemoteValues)
            {
                values += $"{key}: {value} | {value.Source}\n";
            }

            _inputFieldsFlags.text = values;
        }

        public async void GetTime()
        {
            var time = await Time.GetTimestamp();

            _timestamp.text = $"Timestamp: {time}";
            var datetime = DateTimeOffset.FromUnixTimeMilliseconds(time).UtcDateTime;
            _time.text = $"UTC Time: {datetime.ToString(CultureInfo.InvariantCulture)}";
            _localTime.text = $"Local Time: {datetime.ToLocalTime().ToString(CultureInfo.InvariantCulture)}";
        }
    }
}