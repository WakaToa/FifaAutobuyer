using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.EADatabase
{
    public class EADatabaseResultJson
    {
        public int page { get; set; }
        public int totalPages { get; set; }
        public int totalResults { get; set; }
        public string type { get; set; }
        public int count { get; set; }
        public List<Item> items { get; set; }

        public class Item
        {
            public string commonName { get; set; }
            public string firstName { get; set; }
            public string headshotImgUrl { get; set; }
            public string lastName { get; set; }
            public League league { get; set; }
            public Nation nation { get; set; }
            public Club club { get; set; }
            public Headshot headshot { get; set; }
            public SpecialImages specialImages { get; set; }
            public string position { get; set; }
            public string playStyle { get; set; }
            public object playStyleId { get; set; }
            public int height { get; set; }
            public int weight { get; set; }
            public string birthdate { get; set; }
            public int age { get; set; }
            public int acceleration { get; set; }
            public int aggression { get; set; }
            public int agility { get; set; }
            public int balance { get; set; }
            public int ballcontrol { get; set; }
            public string foot { get; set; }
            public int skillMoves { get; set; }
            public int crossing { get; set; }
            public int curve { get; set; }
            public int dribbling { get; set; }
            public int finishing { get; set; }
            public int freekickaccuracy { get; set; }
            public int gkdiving { get; set; }
            public int gkhandling { get; set; }
            public int gkkicking { get; set; }
            public int gkpositioning { get; set; }
            public int gkreflexes { get; set; }
            public int headingaccuracy { get; set; }
            public int interceptions { get; set; }
            public int jumping { get; set; }
            public int longpassing { get; set; }
            public int longshots { get; set; }
            public int marking { get; set; }
            public int penalties { get; set; }
            public int positioning { get; set; }
            public int potential { get; set; }
            public int reactions { get; set; }
            public int shortpassing { get; set; }
            public int shotpower { get; set; }
            public int slidingtackle { get; set; }
            public int sprintspeed { get; set; }
            public int standingtackle { get; set; }
            public int stamina { get; set; }
            public int strength { get; set; }
            public int vision { get; set; }
            public int volleys { get; set; }
            public int weakFoot { get; set; }
            public List<string> traits { get; set; }
            public List<string> specialities { get; set; }
            public string atkWorkRate { get; set; }
            public string defWorkRate { get; set; }
            public string playerType { get; set; }
            public List<Attribute> attributes { get; set; }
            public string name { get; set; }
            public string quality { get; set; }
            public string color { get; set; }
            public bool isGK { get; set; }
            public string positionFull { get; set; }
            public bool isSpecialType { get; set; }
            public object contracts { get; set; }
            public object fitness { get; set; }
            public object rawAttributeChemistryBonus { get; set; }
            public object isLoan { get; set; }
            public object squadPosition { get; set; }
            public string itemType { get; set; }
            public object discardValue { get; set; }
            public string id { get; set; }
            public string modelName { get; set; }
            public int baseId { get; set; }
            public int rating { get; set; }

            public class League
            {
                public string abbrName { get; set; }
                public int id { get; set; }
                public object imgUrl { get; set; }
                public string name { get; set; }
            }

            public class Nation
            {
                public ImageUrls imageUrls { get; set; }
                public string abbrName { get; set; }
                public int id { get; set; }
                public object imgUrl { get; set; }
                public string name { get; set; }

                public class ImageUrls
                {
                    public string small { get; set; }
                    public string medium { get; set; }
                    public string large { get; set; }
                }
            }

            public class Club
            {
                public ImageUrls2 imageUrls { get; set; }
                public string abbrName { get; set; }
                public int id { get; set; }
                public object imgUrl { get; set; }
                public string name { get; set; }

                public class ImageUrls2
                {
                    public Dark dark { get; set; }
                    public Normal normal { get; set; }

                    public class Dark
                    {
                        public string small { get; set; }
                        public string medium { get; set; }
                        public string large { get; set; }
                    }

                    public class Normal
                    {
                        public string small { get; set; }
                        public string medium { get; set; }
                        public string large { get; set; }
                    }
                }
            }

            public class Headshot
            {
                public string largeImgUrl { get; set; }
                public string medImgUrl { get; set; }
                public string smallImgUrl { get; set; }
            }

            public class SpecialImages
            {
                public object largeTOTWImgUrl { get; set; }
                public object medTOTWImgUrl { get; set; }
            }

            public class Attribute
            {
                public string name { get; set; }
                public int value { get; set; }
                public List<int> chemistryBonus { get; set; }
            }
        }
    }
}
