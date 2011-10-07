using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace BattleNet
{
    class Settings
    {
        private static Settings instance;
        private static object syncRoot = new Object();
        SerializableDictionary<String, String> m_data = new SerializableDictionary<String, String>();
        private String m_filename;
        private Settings() { }

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Settings();
                    }
                }

                return instance;
            }
        }

        public bool init(String[] args)
        {
            m_filename = "OmegaBot.xml";
            if (args.Count() == 1)
                m_filename = args[0];
            Load();
            Save();
            return Validate();
        }

        public bool Validate()
        {
            return m_data.ContainsKey("cdkey_d2") && m_data.ContainsKey("cdkey_d2exp") && m_data.ContainsKey("cdkey_user") 
                && m_data.ContainsKey("battlenet_account_name") && m_data.ContainsKey("battlenet_account_password");
        }

        private String GetString(String name, String default_value)
        {
            String ret;
            try
            {
                ret = GetString(name);
            }
            catch
            {
                ret = default_value;
            }
            return ret;
        }

        private String GetString(String name)
        {
            if (!m_data.ContainsKey(name))
            {
                throw new Exception("could not get " + name);
            }
            return m_data[name];
        }


        private Int32 GetInt(String name, Int32 default_value)
        {
            Int32 ret;
            try
            {
                ret = GetInt(name);
            }
            catch
            {
                ret = default_value;
            }
            return ret;
        }

        private Int32 GetInt(String name)
        {
            if (!m_data.ContainsKey(name))
            {
                throw new Exception("could not get " + name);
            }
            return Int32.Parse(m_data[name]);
        }

        private bool GetBool(String name, bool default_value)
        {
            bool ret;
            try
            {
                ret = GetBool(name);
            }
            catch
            {
                ret = default_value;
            }
            return ret;
        }

        private bool GetBool(String name)
        {
            if (!m_data.ContainsKey(name))
            {
                throw new Exception("could not get " + name);
            }
            return m_data[name].Contains("1") || m_data[name].ToLower().Contains("true") || m_data[name].ToLower().Contains("on");
        }

        private bool Load() 
        {
            bool success = true;
            try
            {
                FileStream fs = new FileStream(m_filename, FileMode.Open);
                XmlSerializer x = new XmlSerializer(typeof(SerializableDictionary<String, String>));
                m_data = (SerializableDictionary<String, String>)x.Deserialize(fs);
                fs.Close();
            }
            catch
            {
                success = false;
            }
            return success;  
        }

        private bool Save() {
            bool success = true;
            try
            {
                XmlSerializer x = new XmlSerializer(typeof(SerializableDictionary<String, String>));
                TextWriter writer = new StreamWriter(m_filename);
                x.Serialize(writer, m_data);
                writer.Close();
            }
            catch
            {
                success = false;
            }
            return success; 
        }

        public bool KillPindle()
        {
            return GetBool("kill_pindle", true);
        }

        public bool KillShenk()
        {
            return GetBool("kill_shenk", true);
        }

        public bool KillEldrich()
        {
            return GetBool("kill_eldrich", false);
        }

        public bool ResurrectMercenary()
        {
            return GetBool("resurrect_mercenary", true);
        }

        public String CdKeyUser()
        {
            return GetString("cdkey_user", "OmegaBot");
        }

        public String CdKeyD2()
        {
            return GetString("cdkey_d2");
        }

        public String CdKeyD2Exp()
        {
            return GetString("cdkey_d2exp");
        }

        public String BattlenetGateway()
        {
            return GetString("battlenet_gateway", "europe.battle.net");
        }

        public String BattlenetAccountName()
        {
            return GetString("battlenet_account_name");
        }

        public String BattlenetAccountPassword()
        {
            return GetString("battlenet_account_password");
        }

        public String BattlenetAccountCharacter()
        {
            return GetString("battlenet_account_character");
        }

        public Int32 ChickenLeave()
        {
            return GetInt("chicken_leave", 150);
        }

        public Int32 ChickenPot()
        {
            return GetInt("chicken_pot", 350);
        }

        public Int32 GameMinRuntime() {

            return GetInt("game_min_runtime", 0);
        }

        public Int32 GameStartDelay()
        {

            return GetInt("game_start_delay", 35000);
        }

        public Client.GameDifficulty Difficulty()
        {
            Client.GameDifficulty ret;
            String s = GetString("game_difficulty", "hell");

            if (s.ToLower().Contains("hell"))
            {
                ret = Client.GameDifficulty.HELL;
            }

            else if(s.ToLower().Contains("night")) {
                ret = Client.GameDifficulty.NIGHTMARE;
            }

            else {
                ret = Client.GameDifficulty.NORMAL;
            }

            return ret;
        }

    }
}
