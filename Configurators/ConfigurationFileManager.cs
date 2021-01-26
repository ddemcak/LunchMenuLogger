using Microsoft.Extensions.Configuration.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LunchMenuLogger.Configurators
{
    class ConfigurationFileManager
    {
        #region PROPERTIES

        public string LunchMenuUrl { get; set; }

        public string DbServer { get; set; }
        public string DbUser { get; set; }
        public string DbPass { get; set; }
        public string DbName { get; set; }
        

        #endregion

        #region CONSTRUCTOR

        public ConfigurationFileManager(string configFilename)
        {
            LoadConfigFile(configFilename);
        }

        #endregion

        #region METHODS


        private void LoadConfigFile(string filename)
        {
            StreamReader sr = new StreamReader(filename);

            IniConfigurationSource iniSrouce = new IniConfigurationSource();
            iniSrouce.Path = filename;

            IniConfigurationProvider iniFile = new IniConfigurationProvider(iniSrouce);
            iniFile.Load(sr.BaseStream);

            string value;
            iniFile.TryGet("General:LunchMenuURL", out value);
            LunchMenuUrl = value;

            iniFile.TryGet("Database:Server", out value);
            DbServer = value;

            iniFile.TryGet("Database:User", out value);
            DbUser = value;

            iniFile.TryGet("Database:Password", out value);
            DbPass = value;

            iniFile.TryGet("Database:Name", out value);
            DbName = value;
                        


        }

        #endregion
    }
}
