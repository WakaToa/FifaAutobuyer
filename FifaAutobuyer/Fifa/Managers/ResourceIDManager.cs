using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Managers
{
    public class ResourceIDManager
    {
        public static int GetAssetID(int resourceID)
        {
            var val = (uint)resourceID;
            return (int)(val & CARD_BID_MASK);
        }

        public static int GetRevID(int resourceID)
        {
            var val = (uint)resourceID;
            return (int)((val & ITEM_REV_MASK) >> 24);
        }

        public static int AssetIDToDefinitionID(int assetID, int revID)
        {
            int definitionID;
            int baseInt = 1610612736;
            int firstInt = 50331648;
            int secondInt = 16777216;
            int temp;
            if (revID == 1)
                return (assetID + baseInt) + firstInt;
            else
            {
                temp = assetID + baseInt;
                for (int i = 0; i < revID; i++)
                {
                    temp = temp + secondInt;
                }
                definitionID = temp;
            }

            return definitionID - 1610612736;
        }

        const uint CARD_BID_MASK = 0x00FFFFFF,
            ITEM_REV_MASK = 0x0F000000,
            UNKNOWN_MASK = 0xF0000000;
    }
}
