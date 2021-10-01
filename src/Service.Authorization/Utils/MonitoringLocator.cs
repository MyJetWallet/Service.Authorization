using Prometheus;

namespace Service.Authorization.Utils
{
    public static class MonitoringLocator
    {
        public static readonly Gauge ItemsInAuthLogQueue =
            Metrics.CreateGauge("auth_grpc_authlog_queue", "Count items in authlog queue");

        public static readonly Counter TotalItemsInAuthLogQueue =
            Metrics.CreateCounter("auth_grpc_authlog_queue_total", "Count items in authlog queue");
    }
}