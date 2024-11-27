namespace Divination.Common
{
    public class ClientOptions: CommonOptions
    { 
        public string CheckUpdateSchedule { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }        
        public string Architecture { get; set; }
        public string ReleasePath { get; set; }
        public string DownloadedVersionField { get; set; }
        public string InstalledVersionField { get; set; }

        public string CheckUpdateScheduleSelf { get; set; }
        public string LoginSelf { get; set; }
        public string PasswordSelf { get; set; }        
        public string ArchitectureSelf { get; set; }
        public string ReleasePathSelf { get; set; }
        public string DownloadedSelfVersionField { get; set; }
        public string InstalledSelfVersionField { get; set; }
        public string NextRunDateTimeField { get; set; }
        public string NextRunDateTimeSelfField { get; set; }
        public string InstallSchedule { get; set; }
        public string InstallSelfSchedule { get; set; }
        public string ApplicationDirectory { get; set; }
        public string BackupDirectory { get; set; }

        public string ApplicationSelfDirectory { get; set; }
        public string BackupSelfDirectory { get; set; }

        public string UpdateScript { get; set; }
        public string UpdateSelfScript { get; set; }
        public string ServiceName { get; set; }

        public RunMode Mode { get; set; }
        public string SelfUpdateTempDir { get; set; }
        public string Server { get; set; }
    }
}