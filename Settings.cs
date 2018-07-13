using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using BattleNet.Configuration;
using BattleNet.Logging;
using Newtonsoft.Json;

namespace BattleNet {
    class Settings {
        private static Settings instance;
        private static object syncRoot = new Object();
        BotConfiguration _data = new BotConfiguration();
        private String _filename;
        private Settings() { }

        public static Settings Instance {
            get {
                if (instance == null) {
                    lock (syncRoot) {
                        if (instance == null)
                            instance = new Settings();
                    }
                }

                return instance;
            }
        }

        public bool Init(String[] args) {
            _filename = "OmegaBot.json";
            if (args.Count() == 1)
                _filename = args[0];
            Load();
            Save();
            return Validate();
        }

        public bool Validate() {
            return !string.IsNullOrEmpty(CdKeyD2()) && !string.IsNullOrEmpty(CdKeyD2Exp()) && !string.IsNullOrEmpty(BattlenetAccountName())
                   && !string.IsNullOrEmpty(BattlenetAccountPassword());
        }

        private bool Load() {
            bool success = true;
            try {
                var rawConfig = File.ReadAllText(_filename);
                _data = JsonConvert.DeserializeObject<BotConfiguration>(rawConfig);
                Logger.Write(_data.ToString());
                //FileStream fs = new FileStream(_filename, FileMode.Open);
                //XmlSerializer x = new XmlSerializer(typeof(SerializableDictionary<String, String>));
                //_data = (SerializableDictionary<String, String>)x.Deserialize(fs);
                //fs.Close();
            }
            catch {
                success = false;
            }
            return success;
        }

        private bool Save() {
            bool success = true;
            try {
                var data = JsonConvert.SerializeObject(_data);
                File.WriteAllText(_filename, data);

                //XmlSerializer x = new XmlSerializer(typeof(SerializableDictionary<String, String>));
                //TextWriter writer = new StreamWriter(_filename);
                //x.Serialize(writer, _data);
                //writer.Close();
            }
            catch {
                success = false;
            }
            return success;
        }

        public bool KillPindle() {
            return _data.GameOptions.BossOptions.Pindle;
        }

        public bool KillShenk() {
            return _data.GameOptions.BossOptions.Shenk;
        }

        public bool KillEldrich() {
            return _data.GameOptions.BossOptions.Eldritch;
        }

        public bool ResurrectMercenary() {
            return _data.GameOptions.ResurrectMercenary;
        }

        public String CdKeyUser() {
            return _data.CdKeyOptions.User ?? "Omega";
        }

        public String CdKeyD2() {
            return _data.CdKeyOptions.D2;
        }

        public String CdKeyD2Exp() {
            return _data.CdKeyOptions.D2Exp;
        }

        public String BattlenetGateway() {
            return _data.AccountOptions.Gateway;
        }

        public String BattlenetAccountName() {
            return _data.AccountOptions.Name;
        }

        public String BattlenetAccountPassword() {
            return _data.AccountOptions.Password;
        }

        public string BattlenetAccountCharacter() {
            return _data.AccountOptions.CharacterName;
        }

        public int ChickenLeave() {
            return _data.GameOptions.ChickenOptions.Leave;
        }

        public int ChickenPot() {
            return _data.GameOptions.ChickenOptions.Potion;
        }

        public string GameName() {
            return _data.GameOptions.Name;
        }

        public string GamePassword() {
            return _data.GameOptions.Password;
        }

        public int GameMinRuntime() {
            return _data.GameOptions.MinRuntime;
        }

        public int GameStartDelay() {
            return _data.GameOptions.StartDelay;
        }

        public Client.GameDifficulty Difficulty() {
            Client.GameDifficulty ret;
            String difficulty = _data.GameOptions.Difficulty?.ToLower() ?? "hell";

            if (difficulty.Contains("hell")) {
                ret = Client.GameDifficulty.Hell;
            }
            else if (difficulty.Contains("night")) {
                ret = Client.GameDifficulty.Nightmare;
            }
            else {
                ret = Client.GameDifficulty.Normal;
            }

            return ret;
        }

    }
}
