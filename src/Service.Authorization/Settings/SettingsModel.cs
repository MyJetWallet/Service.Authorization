﻿using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Authorization.Settings
{
    public class SettingsModel
    {
        [YamlProperty("Authorization.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("Authorization.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("Authorization.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
        
        [YamlProperty("Authorization.PostgresConnectionString")]
        public string PostgresConnectionString { get; set; }  
        
        [YamlProperty("Authorization.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }
        
        [YamlProperty("Authorization.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }      
        
        [YamlProperty("Authorization.MaxItemsInCache")]
        public int MaxItemsInCache { get; set; }

        [YamlProperty("Authorization.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }
        
    }
}
