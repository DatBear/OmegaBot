namespace BattleNet.Configuration {
    public class BotConfiguration {
        public CdKeyOptions CdKeyOptions { get; set; }
        public AccountOptions AccountOptions { get; set; }
        public GameOptions GameOptions { get; set; }
    }

    public class CdKeyOptions {
        public string D2 { get; set; }
        public string D2Exp { get; set; }
        public string User { get; set; }
    }

    public class AccountOptions {
        public string Gateway { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string CharacterName { get; set; }
    }

    public class GameOptions {
        public ChickenOptions ChickenOptions { get; set; }
        public BossOptions BossOptions { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string FollowAccountName { get; set; }
        public string FollowPlayerName { get; set; }
        public string Difficulty { get; set; }
        public int StartDelay { get; set; }
        public int MinRuntime { get; set; }
        public bool ResurrectMercenary { get; set; }
    }

    public class ChickenOptions {
        public int Potion { get; set; }
        public int Leave { get; set; }
        public int PotionPercent { get; set; }
        public int LeavePercent { get; set; }
    }

    public class BossOptions {
        public bool Pindle { get; set; }
        public bool Shenk { get; set; }
        public bool Eldritch { get; set; }
    }

}