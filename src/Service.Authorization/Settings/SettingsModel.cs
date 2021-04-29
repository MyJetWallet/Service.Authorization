using MyJetWallet.Sdk.Service;
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

        [YamlProperty("Authorization.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("Authorization.RootSessionLifeTimeHours")]
        public int RootSessionLifeTimeHours { get; set; }

        [YamlProperty("Authorization.TimeoutToRefreshNewSessionInSec")]
        public int TimeoutToRefreshNewSessionInSec { get; set; }

        [YamlProperty("Authorization.SessionLifeTimeMinutes")]
        public int SessionLifeTimeMinutes { get; set; }

        [YamlProperty("Authorization.RequestTimeLifeSec")]
        public int RequestTimeLifeSec { get; set; }

        [YamlProperty("Authorization.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("Authorization.RegistrationGrpcServiceUrl")]
        public string RegistrationGrpcServiceUrl { get; set; }

        [YamlProperty("Authorization.ClientWalletsGrpcServiceUrl")]
        public string ClientWalletsGrpcServiceUrl { get; set; }

        [YamlProperty("Authorization.PostgresConnectionString")]
        public string PostgresConnectionString { get; set; }
    }
}
