using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Extensions;
using FifaAutobuyer.Fifa.Http;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Requests;
using FifaAutobuyer.Fifa.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Factories
{
    public class FUTRequestFactory
    {
        private HttpClientEx _httpClient;
        public HttpClientEx HttpClient { get { return _httpClient; } }

        private CookieContainer _cookies;

        public FUTAccount FUTAccount { get; set; }

        public RequestPerMinuteManager RequestPerMinuteManager { get; set; }
        public RequestPerMinuteManager RequestPerMinuteManagerSearch { get; set; }

        public int RequestCounter { get; set; }
        public LoginMethod CurrentLoginMethod { get; set; }

        public FUTRequestFactory(FUTAccount account)
        {
            FUTAccount = account;
            _httpClient = new HttpClientEx();
            _httpClient.AddCommonHeaders();
            _cookies = new CookieContainer();
            //load cookies from db
            //var futCookies = FUTAccountsDatabase.GetFUTCookiesByFUTAccount(FUTAccount);
            //if (futCookies != null)
            //{
            //    if(!string.IsNullOrEmpty(futCookies.FutWeb))
            //    {
            //        _cookies = futCookies.FutWeb.DeserializeCookieContainer();
            //    }
            //}
            _httpClient.ClientHandler.CookieContainer = _cookies;

            RequestPerMinuteManager = new RequestPerMinuteManager();
            RequestPerMinuteManagerSearch = new RequestPerMinuteManager(true);
            RequestCounter = 0;
        }

        public void SetProxy(string proxyServer, string proxyUsername, string proxyPassword)
        {
            if(proxyServer != "")
            {
                var webProxy = new WebProxy(proxyServer);
                webProxy.Credentials = new NetworkCredential(proxyUsername, proxyPassword);

                _httpClient.ClientHandler.Proxy = webProxy;
                _httpClient.ClientHandler.UseProxy = true;
            }
            
        }

        private Func<IFUTRequest<CreditsResponse>> _creditsRequestFactory;
        public Func<IFUTRequest<CreditsResponse>> CreditsRequestFactory
        {
            get
            {
                if(_creditsRequestFactory == null)
                {
                    var creditsRequest = new CreditsRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    creditsRequest.HttpClient = HttpClient;
                    _creditsRequestFactory = (_creditsRequestFactory = () => creditsRequest);
                }
                RequestCounter++;
                return _creditsRequestFactory;
            }
        }

        private Func<IFUTRequest<TradepileResponse>> _tradepileRequestFactory;
        public Func<IFUTRequest<TradepileResponse>> TradepileRequestFactory
        {
            get
            {
                if (_tradepileRequestFactory == null)
                {
                    var tradepileRequest = new TradepileRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    tradepileRequest.HttpClient = HttpClient;
                    _tradepileRequestFactory = (_tradepileRequestFactory = () => tradepileRequest);
                }
                RequestCounter++;
                return _tradepileRequestFactory;
            }
        }

        private Func<IFUTRequest<bool>> _relistAllRequestFactory;
        public Func<IFUTRequest<bool>> RelistAllRequestFactory
        {
            get
            {
                if (_relistAllRequestFactory == null)
                {
                    var relistRequest = new RelistAllRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    relistRequest.HttpClient = HttpClient;
                    _relistAllRequestFactory = (_relistAllRequestFactory = () => relistRequest);
                }
                RequestCounter++;
                return _relistAllRequestFactory;
            }
        }

        private Func<IFUTRequest<WatchlistResponse>> _watchlistRequestFactory;
        public Func<IFUTRequest<WatchlistResponse>> WatchlistRequestFactory
        {
            get
            {
                if (_watchlistRequestFactory == null)
                {
                    var watchlistRequest = new WatchlistRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    watchlistRequest.HttpClient = HttpClient;
                    _watchlistRequestFactory = (_watchlistRequestFactory = () => watchlistRequest);
                }
                RequestCounter++;
                return _watchlistRequestFactory;
            }
        }

        private Func<IFUTRequest<UserMassInfoResponse>> _userMassInfoRequestFactory;
        public Func<IFUTRequest<UserMassInfoResponse>> UserMassInfoRequestFactory
        {
            get
            {
                if (_userMassInfoRequestFactory == null)
                {
                    var watchlistRequest = new UserMassInfoRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    watchlistRequest.HttpClient = HttpClient;
                    _userMassInfoRequestFactory = (_userMassInfoRequestFactory = () => watchlistRequest);
                }
                RequestCounter++;
                return _userMassInfoRequestFactory;
            }
        }

        private Func<int, IFUTRequest<bool>> _deleteActiveMessageRequestFactory;
        public Func<int, IFUTRequest<bool>> DeleteActiveMessageRequestFactory
        {
            get
            {
                RequestCounter++;
                return _deleteActiveMessageRequestFactory ?? (_deleteActiveMessageRequestFactory = (item) => new DeleteActiveMessageRequest(FUTAccount, item, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod)
                {
                    HttpClient = HttpClient
                });
            }
        }

        private Func<IFUTRequest<NotAssignedResponse>> _notAssignedRequestFactory;
        public Func<IFUTRequest<NotAssignedResponse>> NotAssignedRequestFactory
        {
            get
            {
                if (_notAssignedRequestFactory == null)
                {
                    var notAssignedRequest = new NotAssignedRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    notAssignedRequest.HttpClient = HttpClient;
                    _notAssignedRequestFactory = (_notAssignedRequestFactory = () => notAssignedRequest);
                }
                RequestCounter++;
                return _notAssignedRequestFactory;
            }
        }

        private Func<IFUTRequest<UserHistoricalResponse>> _userHistoricalRequestFactory;
        public Func<IFUTRequest<UserHistoricalResponse>> UserHistoricalRequestFactory
        {
            get
            {
                if (_userHistoricalRequestFactory == null)
                {
                    var userHistoricalRequest = new UserHistoricalRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    userHistoricalRequest.HttpClient = HttpClient;
                    _userHistoricalRequestFactory = (_userHistoricalRequestFactory = () => userHistoricalRequest);
                }
                RequestCounter++;
                return _userHistoricalRequestFactory;
            }
        }

        private Func<UserHistoricalResponse, IFUTRequest<UserResponse>> _setNewUserRequestFactory;
        public Func<UserHistoricalResponse, IFUTRequest<UserResponse>> SetNewUserRequestFactory
        {
            get
            {
                RequestCounter++;
                return _setNewUserRequestFactory ?? (_setNewUserRequestFactory = (item) => new SetNewUserRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod, item)
                {
                    HttpClient = HttpClient
                });
            }
        }

        private Func<FUTSearchParameter, bool, IFUTRequest<TransferMarketResponse>> _transferMarketRequestFactory;
        public Func<FUTSearchParameter, bool, IFUTRequest<TransferMarketResponse>> TransferMarketRequestFactory
        {
            get
            {
                RequestCounter++;
                return _transferMarketRequestFactory ?? (_transferMarketRequestFactory = (item, isMuling) => new TransferMarketRequest(FUTAccount, item, isMuling,  RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod)
                {
                HttpClient = HttpClient
                });
            }
        }

        private Func<long,long, IFUTRequest<BuyItemResponse>> _buyTradeRequestFactory;
        public Func<long, long, IFUTRequest<BuyItemResponse>> BuyTradeRequestFactory
        {
            get
            {
                RequestCounter++;
                return _buyTradeRequestFactory ?? (_buyTradeRequestFactory = (tradeID, price) => new BuyTradeRequest(FUTAccount, tradeID, price, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod)
                {
                    HttpClient = HttpClient
                });
            }
        }

        private Func<long, IFUTRequest<MoveItemResponse>> _moveItemToTradepileRequestFactory;
        public Func<long, IFUTRequest<MoveItemResponse>> MoveItemToTradepileRequestFactory
        {
            get
            {
                RequestCounter++;
                return _moveItemToTradepileRequestFactory ?? (_moveItemToTradepileRequestFactory = (itemID) => new MoveItemToTradepileRequest(FUTAccount, itemID, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod)
                {
                    HttpClient = HttpClient
                });
            }
        }

        private Func<OfferItemModel, IFUTRequest<OfferItemOnTransferMarketResponse>> _offerItemonTransferMarketRequestFactory;
        public Func<OfferItemModel, IFUTRequest<OfferItemOnTransferMarketResponse>> OfferItemonTransferMarketRequestFactory
        {
            get
            {
                RequestCounter++;
                return _offerItemonTransferMarketRequestFactory ?? (_offerItemonTransferMarketRequestFactory = (item) => new OfferItemOnTransferMarketRequest(FUTAccount, item, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod)
                {
                    HttpClient = HttpClient
                });
            }
        }

        private Func<long, IFUTRequest<bool>> _removeItemFromTradepileRequestFactory;
        public Func<long, IFUTRequest<bool>> RemoveItemFromTradepileRequestFactory
        {
            get
            {
                RequestCounter++;
                return _removeItemFromTradepileRequestFactory ?? (_removeItemFromTradepileRequestFactory = (trade) => new RemoveItemFromTradepileRequest(FUTAccount, trade, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod)
                {
                    HttpClient = HttpClient
                });
            }
        }
        private Func<List<string>, IFUTRequest<bool>> _removeItemsFromWatchlistRequestFactory;
        public Func<List<string>, IFUTRequest<bool>> RemoveItemsFromWatchlistRequestFactory
        {
            get
            {
                RequestCounter++;
                return _removeItemsFromWatchlistRequestFactory ?? (_removeItemsFromWatchlistRequestFactory = (trades) => new RemoveItemsFromWatchlistRequest(FUTAccount, trades, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod)
                {
                    HttpClient = HttpClient
                });
            }
        }


        private Func<IFUTRequest<LoginResponse>> _loginWebRequestFactory;
        public Func<IFUTRequest<LoginResponse>> LoginWebRequestFactory
        {
            get
            {
                if (_loginWebRequestFactory == null)
                {
                    var loginRequest = new LoginWebRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    loginRequest.HttpClient = HttpClient;
                    _loginWebRequestFactory = (_loginWebRequestFactory = () => loginRequest);

                }
                return _loginWebRequestFactory;
            }
        }

        private Func<IFUTRequest<LoginResponse>> _loginIOSRequestFactory;
        public Func<IFUTRequest<LoginResponse>> LoginIOSRequestFactory
        {
            get
            {
                if (_loginIOSRequestFactory == null)
                {
                    var loginRequest = new LoginIOSRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    loginRequest.HttpClient = HttpClient;
                    _loginIOSRequestFactory = (_loginIOSRequestFactory = () => loginRequest);

                }
                return _loginIOSRequestFactory;
            }
        }

        private Func<IFUTRequest<LoginResponse>> _loginAndroidRequestFactory;
        public Func<IFUTRequest<LoginResponse>> LoginAndroidRequestFactory
        {
            get
            {
                if (_loginAndroidRequestFactory == null)
                {
                    var loginRequest = new LoginAndroidRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    loginRequest.HttpClient = HttpClient;
                    _loginAndroidRequestFactory = (_loginAndroidRequestFactory = () => loginRequest);

                }
                return _loginAndroidRequestFactory;
            }
        }

        private Func<long, IFUTRequest<PriceLimitsResponse>> _priceLimitsRequestFactory;
        public Func<long, IFUTRequest<PriceLimitsResponse>> PriceLimitsRequestFactory
        {
            get
            {
                RequestCounter++;
                return _priceLimitsRequestFactory ?? (_priceLimitsRequestFactory = (assetID) => new PriceLimitsRequest(FUTAccount, assetID, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod)
                {
                    HttpClient = HttpClient
                });
            }
        }


        private Func<long,bool, IFUTRequest<DiscardItemResponse>> _discardItemRequestFactory;
        public Func<long,bool, IFUTRequest<DiscardItemResponse>> DiscardItemRequestFactory
        {
            get
            {
                RequestCounter++;
                return _discardItemRequestFactory ?? (_discardItemRequestFactory = (assetID, isMuling) => new DiscardItemRequest(FUTAccount, assetID, isMuling, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod)
                {
                    HttpClient = HttpClient
                });
            }
        }

        private Func<IFUTRequest<SettingsResponse>> _settingsRequestFactory;
        public Func<IFUTRequest<SettingsResponse>> SettingsRequestFactory
        {
            get
            {
                if (_settingsRequestFactory == null)
                {
                    var settingsRequest = new SettingsRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    settingsRequest.HttpClient = HttpClient;
                    RequestCounter++;
                    _settingsRequestFactory = (_settingsRequestFactory = () => settingsRequest);

                }
                return _settingsRequestFactory;
            }
        }

        private Func<IFUTRequest<UserResponse>> _userRequestFactory;
        public Func<IFUTRequest<UserResponse>> UserRequestFactory
        {
            get
            {
                if (_userRequestFactory == null)
                {
                    var userRequest = new UserRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    userRequest.HttpClient = HttpClient;
                    _userRequestFactory = (_userRequestFactory = () => userRequest);
                }
                RequestCounter++;
                return _userRequestFactory;
            }
        }


        private Func<IFUTRequest<PileSizeResponse>> _pileSizeRequestFactory;
        public Func<IFUTRequest<PileSizeResponse>> PileSizeRequestFactory
        {
            get
            {
                if (_userRequestFactory == null)
                {
                    var pileRequest = new PileSizeRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod);
                    pileRequest.HttpClient = HttpClient;
                    _pileSizeRequestFactory = (_pileSizeRequestFactory = () => pileRequest);

                }
                RequestCounter++;
                return _pileSizeRequestFactory;
            }
        }

        private Func<string, IFUTRequest<bool>> _solveCaptchaRequestFactory;
        public Func<string, IFUTRequest<bool>> SolveCaptchaRequestFactory
        {
            get
            {
                RequestCounter++;
                return _solveCaptchaRequestFactory ?? (_solveCaptchaRequestFactory = (captchaResponse) => new SolveCaptchaRequest(FUTAccount, RequestPerMinuteManager, RequestPerMinuteManagerSearch, CurrentLoginMethod, captchaResponse)
                {
                    HttpClient = HttpClient
                });
            }
        }
    }
}
