using HomeCenter.Actors.Tests.Builders;

namespace HomeCenter.Services.MotionService.Tests
{
    public static class RavenConfig
    {
        public static bool UseRavenDbLogs { get; }
        public static bool CleanLogsBeforeRun { get; }

        static RavenConfig()
        {
            if (UseRavenDbLogs)
            {
                var ravenConfig = new RavenDbConfigurator();
                UseRavenDbLogs = ravenConfig.CheckDbConnection();
                ravenConfig.Dispose();
            }
        }
    }
}