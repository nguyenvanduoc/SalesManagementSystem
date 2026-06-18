using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using Newtonsoft.Json;

namespace SalesManagementSystem.Helpers.Security
{
    public static class ConfigManager
    {
        private static Dictionary<string, string> _cachedConfig;
        private static readonly object _lockObj = new object();

        /// <summary>
        /// Khởi tạo và đọc cấu hình từ file system.dat
        /// </summary>
        private static void EnsureConfigLoaded()
        {
            if (_cachedConfig != null) return;

            lock (_lockObj)
            {
                if (_cachedConfig != null) return;

                try
                {
                    string configFilePath = ConfigurationManager.AppSettings["ConfigFile"];
                    if (string.IsNullOrEmpty(configFilePath))
                        throw new Exception("AppSetting 'ConfigFile' is missing.");

                    // Map virtual path to physical path if inside web context
                    if (configFilePath.StartsWith("~/") && HttpContext.Current != null)
                    {
                        configFilePath = HttpContext.Current.Server.MapPath(configFilePath);
                    }
                    else if (configFilePath.StartsWith("~/"))
                    {
                        configFilePath = configFilePath.Replace("~/", AppDomain.CurrentDomain.BaseDirectory);
                    }

                    if (!File.Exists(configFilePath))
                        throw new FileNotFoundException(string.Format("Configuration file not found at {0}", configFilePath));

                    string fileContent = File.ReadAllText(configFilePath);
                    var parts = fileContent.Split('|');

                    if (parts.Length != 3)
                        throw new Exception("Invalid configuration file format.");

                    string keyPart2Base64 = parts[0];
                    string ivBase64 = parts[1];
                    string encryptedDataBase64 = parts[2];

                    string keyPart1 = ConfigurationManager.AppSettings["KeyPart1"];
                    if (string.IsNullOrEmpty(keyPart1))
                        throw new Exception("AppSetting 'KeyPart1' is missing.");

                    // Convert keyPart2 from Base64
                    byte[] keyPart2Bytes = Convert.FromBase64String(keyPart2Base64);
                    string keyPart2 = System.Text.Encoding.UTF8.GetString(keyPart2Bytes);

                    string fullKey = keyPart1 + keyPart2;
                    byte[] iv = Convert.FromBase64String(ivBase64);

                    // Decrypt data
                    string decryptedJson = AesCryptoHelper.Decrypt(encryptedDataBase64, fullKey, iv);

                    // Parse JSON
                    _cachedConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedJson);

                    if (_cachedConfig == null)
                        _cachedConfig = new Dictionary<string, string>();
                }
                catch (Exception ex)
                {
                    // Throw explicitly to prevent application from running with broken configs
                    throw new Exception("Failed to load secure configuration. Check KeyPart1 and system.dat.", ex);
                }
            }
        }

        public static string GetConnectionString(string name = "DefaultConnection")
        {
            EnsureConfigLoaded();
            if (_cachedConfig.TryGetValue("ConnectionStrings:" + name, out string value))
                return value;
            
            // Fallback (for older items that might just use the name directly)
            if (_cachedConfig.TryGetValue(name, out string fallbackValue))
                return fallbackValue;

            throw new Exception(string.Format("ConnectionString '{0}' not found in secure configuration.", name));
        }

        public static string GetValue(string key)
        {
            EnsureConfigLoaded();
            if (_cachedConfig.TryGetValue(key, out string value))
                return value;
            return null;
        }
    }
}
