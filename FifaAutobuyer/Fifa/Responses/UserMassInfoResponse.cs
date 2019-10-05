using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class UserMassInfoResponse : FUTError
    {
        public Errors errors { get; set; }
        public Settings settings { get; set; }
        public UserInfo userInfo { get; set; }
        public PurchasedItems purchasedItems { get; set; }
        public PileSizeClientData pileSizeClientData { get; set; }
        public Squad2 squad { get; set; }
        public ClubUser clubUser { get; set; }
        public ActiveMessages activeMessages { get; set; }
    }

    public class Errors
    {
    }

    public class Config
    {
        public long value { get; set; }
        public string type { get; set; }
    }

    public class Settings
    {
        public List<Config> configs { get; set; }
    }

    public class BidTokens
    {
    }

    public class Currency
    {
        public string name { get; set; }
        public int funds { get; set; }
        public int finalFunds { get; set; }
    }

    public class Active
    {
        public object id { get; set; }
        public int timestamp { get; set; }
        public string formation { get; set; }
        public bool untradeable { get; set; }
        public int assetId { get; set; }
        public int rating { get; set; }
        public string itemType { get; set; }
        public int resourceId { get; set; }
        public int owners { get; set; }
        public int discardValue { get; set; }
        public string itemState { get; set; }
        public int cardsubtypeid { get; set; }
        public int lastSalePrice { get; set; }
        public List<object> statsList { get; set; }
        public List<object> lifetimeStats { get; set; }
        public List<object> attributeList { get; set; }
        public int teamid { get; set; }
        public int rareflag { get; set; }
        public int leagueId { get; set; }
        public int pile { get; set; }
        public int cardassetid { get; set; }
        public int value { get; set; }
        public int category { get; set; }
        public string name { get; set; }
        public int weightrare { get; set; }
        public string description { get; set; }
        public string header { get; set; }
        public string biodescription { get; set; }
        public int? stadiumid { get; set; }
        public int? capacity { get; set; }
        public int? year { get; set; }
        public string manufacturer { get; set; }
    }

    public class Squad
    {
        public int id { get; set; }
        public bool valid { get; set; }
        public object personaId { get; set; }
        public string formation { get; set; }
        public int rating { get; set; }
        public int chemistry { get; set; }
        public List<object> manager { get; set; }
        public List<object> players { get; set; }
        public object dreamSquad { get; set; }
        public object changed { get; set; }
        public string squadName { get; set; }
        public int starRating { get; set; }
        public object captain { get; set; }
        public List<object> kicktakers { get; set; }
        public List<object> actives { get; set; }
        public object newSquad { get; set; }
        public string squadType { get; set; }
        public object custom { get; set; }
    }

    public class SquadList
    {
        public List<Squad> squad { get; set; }
        public int activeSquadId { get; set; }
    }

    public class UnopenedPacks
    {
        public int preOrderPacks { get; set; }
        public int recoveredPacks { get; set; }
    }

    public class Reliability
    {
        public int reliability { get; set; }
        public int startedMatches { get; set; }
        public int finishedMatches { get; set; }
        public int matchUnfinishedTime { get; set; }
    }

    public class Feature
    {
        public int trade { get; set; }
    }

    public class UserInfo
    {
        public int personaId { get; set; }
        public string clubName { get; set; }
        public string clubAbbr { get; set; }
        public int draw { get; set; }
        public int loss { get; set; }
        public int credits { get; set; }
        public BidTokens bidTokens { get; set; }
        public List<Currency> currencies { get; set; }
        public int trophies { get; set; }
        public int won { get; set; }
        public List<Active> actives { get; set; }
        public string established { get; set; }
        public int divisionOffline { get; set; }
        public int divisionOnline { get; set; }
        public string personaName { get; set; }
        public SquadList squadList { get; set; }
        public UnopenedPacks unopenedPacks { get; set; }
        public bool purchased { get; set; }
        public Reliability reliability { get; set; }
        public bool seasonTicket { get; set; }
        public string accountCreatedPlatformName { get; set; }
        public int fifaPointsFromLastYear { get; set; }
        public int fifaPointsTransferredStatus { get; set; }
        public int unassignedPileSize { get; set; }
        public Feature feature { get; set; }
        public int sessionCoinsBankBalance { get; set; }
    }

    public class PurchasedItems
    {
        public List<object> itemData { get; set; }
    }

    public class Entry
    {
        public int value { get; set; }
        public int key { get; set; }
    }

    public class PileSizeClientData
    {
        public List<Entry> entries { get; set; }
    }

    public class Manager
    {
        public long id { get; set; }
        public int timestamp { get; set; }
        public string formation { get; set; }
        public bool untradeable { get; set; }
        public int assetId { get; set; }
        public int rating { get; set; }
        public string itemType { get; set; }
        public int resourceId { get; set; }
        public int owners { get; set; }
        public int discardValue { get; set; }
        public string itemState { get; set; }
        public int cardsubtypeid { get; set; }
        public int lastSalePrice { get; set; }
        public List<object> statsList { get; set; }
        public List<object> lifetimeStats { get; set; }
        public int contract { get; set; }
        public List<object> attributeList { get; set; }
        public int teamid { get; set; }
        public int rareflag { get; set; }
        public int leagueId { get; set; }
        public int pile { get; set; }
        public int nation { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int negotiation { get; set; }
    }

    public class ItemData
    {
        public long id { get; set; }
        public int timestamp { get; set; }
        public string formation { get; set; }
        public bool untradeable { get; set; }
        public int assetId { get; set; }
        public int rating { get; set; }
        public string itemType { get; set; }
        public int resourceId { get; set; }
        public int owners { get; set; }
        public int discardValue { get; set; }
        public string itemState { get; set; }
        public int cardsubtypeid { get; set; }
        public int lastSalePrice { get; set; }
        public int morale { get; set; }
        public int fitness { get; set; }
        public string injuryType { get; set; }
        public int injuryGames { get; set; }
        public string preferredPosition { get; set; }
        public List<object> statsList { get; set; }
        public List<object> lifetimeStats { get; set; }
        public int training { get; set; }
        public int contract { get; set; }
        public int suspension { get; set; }
        public List<object> attributeList { get; set; }
        public int teamid { get; set; }
        public int rareflag { get; set; }
        public int playStyle { get; set; }
        public int leagueId { get; set; }
        public int assists { get; set; }
        public int lifetimeAssists { get; set; }
        public int loyaltyBonus { get; set; }
        public int pile { get; set; }
        public int nation { get; set; }
    }

    public class Player
    {
        public int index { get; set; }
        public ItemData itemData { get; set; }
        public int loyaltyBonus { get; set; }
        public int kitNumber { get; set; }
    }

    public class Kicktaker
    {
        public int id { get; set; }
        public int index { get; set; }
    }

    public class Active2
    {
        public object id { get; set; }
        public int timestamp { get; set; }
        public string formation { get; set; }
        public bool untradeable { get; set; }
        public int assetId { get; set; }
        public int rating { get; set; }
        public string itemType { get; set; }
        public int resourceId { get; set; }
        public int owners { get; set; }
        public int discardValue { get; set; }
        public string itemState { get; set; }
        public int cardsubtypeid { get; set; }
        public int lastSalePrice { get; set; }
        public List<object> statsList { get; set; }
        public List<object> lifetimeStats { get; set; }
        public List<object> attributeList { get; set; }
        public int teamid { get; set; }
        public int rareflag { get; set; }
        public int leagueId { get; set; }
        public int pile { get; set; }
        public int cardassetid { get; set; }
        public int value { get; set; }
        public int category { get; set; }
        public string name { get; set; }
        public int weightrare { get; set; }
        public string description { get; set; }
        public string header { get; set; }
        public string biodescription { get; set; }
        public int? stadiumid { get; set; }
        public int? capacity { get; set; }
        public int? year { get; set; }
        public string manufacturer { get; set; }
    }

    public class Squad2
    {
        public int id { get; set; }
        public bool valid { get; set; }
        public int personaId { get; set; }
        public string formation { get; set; }
        public object rating { get; set; }
        public int chemistry { get; set; }
        public List<Manager> manager { get; set; }
        public List<Player> players { get; set; }
        public object dreamSquad { get; set; }
        public int changed { get; set; }
        public string squadName { get; set; }
        public int starRating { get; set; }
        public int captain { get; set; }
        public List<Kicktaker> kicktakers { get; set; }
        public List<Active2> actives { get; set; }
        public object newSquad { get; set; }
        public string squadType { get; set; }
        public object custom { get; set; }
    }

    public class User
    {
        public int personaId { get; set; }
        public string persona { get; set; }
        public bool @public { get; set; }
    }

    public class ClubUser
    {
        public List<User> user { get; set; }
    }

    public class ActiveMessage
    {
        public object message { get; set; }
        public int id { get; set; }
        public string type { get; set; }
        public object time { get; set; }
        public object startTime { get; set; }
        public int rewardValue { get; set; }
        public string rewardType { get; set; }
        public object read { get; set; }
    }

    public class ActiveMessages
    {
        public List<ActiveMessage> activeMessage { get; set; }
    }
}
