using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Database.Settings;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.MuleApi.Clients;

namespace FifaAutobuyer.WebServer.Models
{
    public class MuleApiManagerModel : BaseModel
    {
        public string GTEPartnerID => FUTSettings.Instance.GTEPartnerID;
        public string GTEPartnerApiKey => FUTSettings.Instance.GTEPartnerApiKey;
        public bool GTEClientPlatformPS4 => FUTSettings.Instance.GTEClientPlatform == GameTradeEasyApiClient.PlatformPS4;
        public string MuleFactoryUser => FUTSettings.Instance.MuleFactoryUser;
        public string MuleFactorySecretWord => FUTSettings.Instance.MuleFactorySecretWord;
        public bool MuleFactoryPlatformPS4 => FUTSettings.Instance.MuleFactoryClientPlatform == MuleFactoryApiClient.PlatformPS4;
        public string WholeSaleApiKey => FUTSettings.Instance.WholeSaleApiKey;
        public bool WholeSalePlatformPS4 => FUTSettings.Instance.WholeSaleClientPlatform == WholeSaleApiClient.PlatformPS4;
        public int MuleApiMaxSellPerDayPerAccount => FUTSettings.Instance.MuleApiMaxSellPerDayPerAccount;
        public int MuleApiMaxTransactionValue => FUTSettings.Instance.MuleApiMaxTransactionValue;
        public int MuleApiMinCoinsOnAccount => FUTSettings.Instance.MuleApiMinCoinsOnAccount;
        public int MuleApiRequestDelay => FUTSettings.Instance.MuleApiRequestDelay;
        public int MuleApiSellDelayPerAccount => FUTSettings.Instance.MuleApiSellDelayPerAccount;


        public bool GameTradeEasyApiClientRunning => GameTradeEasyApiClient.Client.Running;
        public bool MuleFactoryApiClientRunning => MuleFactoryApiClient.Client.Running;
        public bool WholeSaleApiClientRunning => WholeSaleApiClient.Client.Running;

        public double GTETotalDollarVolume => FUTMuleApiStatistic.Instance.GTETotalDollarVolume;
        public double GTETotalCoinVolume => FUTMuleApiStatistic.Instance.GTETotalCoinVolume;
        public double MFTotalDollarVolume => FUTMuleApiStatistic.Instance.MFTotalDollarVolume;
        public double MFTotalCoinVolume => FUTMuleApiStatistic.Instance.MFTotalCoinVolume;
        public double WSTotalDollarVolume => FUTMuleApiStatistic.Instance.WSTotalDollarVolume;
        public double WSTotalCoinVolume => FUTMuleApiStatistic.Instance.WSTotalCoinVolume;

        public double TotalDollarVolume => FUTMuleApiStatistic.Instance.GTETotalDollarVolume + FUTMuleApiStatistic.Instance.MFTotalDollarVolume + FUTMuleApiStatistic.Instance.WSTotalDollarVolume;
        public double TotalCoinVolume => FUTMuleApiStatistic.Instance.GTETotalCoinVolume + FUTMuleApiStatistic.Instance.MFTotalCoinVolume + FUTMuleApiStatistic.Instance.WSTotalCoinVolume;

        public MuleApiManagerModel()
        {
            MuleApiManagerActive = "active";
        }
    }
}
