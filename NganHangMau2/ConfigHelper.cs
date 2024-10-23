using System;
using System.Configuration;

namespace NganHangMau2
{
    public static class ConfigHelper
    {
        public static string GetConnectionString(string name)
        {
            DecryptConfigSection(name);
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[name];
            if (connectionStringSettings == null)
            {
                throw new Exception($"Connection string '{name}' not found.");
            }
            string connectionString = connectionStringSettings.ConnectionString;
            EncryptConfigSection("connectionStrings");
            return connectionString;
        }

        public static void SaveConnectionString(string name, string connectionString)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringsSection section = config.ConnectionStrings;

            if (section.ConnectionStrings[name] != null)
            {
                section.ConnectionStrings[name].ConnectionString = connectionString;
            }
            else
            {
                section.ConnectionStrings.Add(new ConnectionStringSettings(name, connectionString));
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        public static void DecryptConfigSection(string sectionKey)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection section = config.GetSection(sectionKey);

            if (section != null && section.SectionInformation.IsProtected)
            {
                section.SectionInformation.UnprotectSection();
                config.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection(sectionKey);

                // Log the decrypted section for debugging
                Console.WriteLine(section.SectionInformation.GetRawXml());
            }
        }

        public static void EncryptConfigSection(string sectionKey)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection section = config.GetSection(sectionKey);

            if (section != null && !section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                config.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection(sectionKey);
            }
        }
    }
}
