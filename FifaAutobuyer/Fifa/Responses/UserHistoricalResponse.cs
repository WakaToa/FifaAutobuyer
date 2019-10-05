using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Models;

namespace FifaAutobuyer.Fifa.Responses
{
    public class UserHistoricalResponse : FUTError
    {
        public string clubName { get; set; }
        public string clubAbbr { get; set; }
        public BidTokens bidTokens { get; set; }
        public string established { get; set; }
        public bool isReturningUser { get; set; }
        public List<ReturningUserReward> returningUserRewards { get; set; }
    }

    public class ReturningUserReward
    {
        public int rewardType { get; set; }
        public int rewardValue { get; set; }
        public int rewardQuantity { get; set; }
        public int halId { get; set; }
    }
}
