using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WindowsService.Net.Framework
{
    public partial class DatabaseBackup : ServiceBase
    {
        private EventLog _eventLog { get; set; }
        private Timer _timer { get; set; }
        private SqlConnection sqlConnection { get; set; }
        private SqlCommand sqlCommand { get; set; }
        private readonly string SqlBackupDirectoryPath = @"C:\sql_backups\";
        private readonly string serverName = @"DESKTOP-4NBHA2U\SQLEXPRESS";
        private readonly string dbName = "BikeStores";
        private readonly string loggerSource = "DatabaseBackup";
        public DatabaseBackup()
        {
            _eventLog = new EventLog();
            _eventLog.Source = "DatabaseBackup";

            InitializeComponent();
            InitializeSqlConnection();
            if (sqlConnection != null)
            {
                SetupBackupDirectory();
                this._timer = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds) { AutoReset = true };
                this._timer.Elapsed += OnTimerElapsed;
            }
            else
            {
                _eventLog.WriteEntry("Sql connection failed to initialize. Service is stoped.");
                this.Stop();
            }
        }

        protected override void OnStart(string[] args)
        {
            this._eventLog.WriteEntry("DatabaseBackupService is started");
            CreateDbBackup();
            this._timer.Start();
        }

        protected override void OnStop()
        {
            this._timer.Stop();
            _eventLog.WriteEntry("DatabaseBackupService is stopped");
        }


        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CreateDbBackup();
        }

        private void SetupBackupDirectory()
        {
            if (!Directory.Exists(SqlBackupDirectoryPath))
            {
                Directory.CreateDirectory(@SqlBackupDirectoryPath);
            }
        }

        private void CreateDbBackup()
        {
            _eventLog.WriteEntry($"Backup database {dbName} to location {SqlBackupDirectoryPath} started");

            try
            {
                this.sqlConnection.Open();
                var backupCommand = $"Backup database {dbName} to disk='{GetBackupFileLocation()}'";
                this.sqlCommand = new SqlCommand(backupCommand, this.sqlConnection);
                this.sqlCommand.ExecuteNonQuery();

                _eventLog.WriteEntry($"Backup database {dbName} to location {SqlBackupDirectoryPath} sucessfully finished");
                this.sqlConnection.Close();
            }
            catch (Exception ex)
            {
                this._eventLog.WriteEntry($"Backup database {dbName} to location {SqlBackupDirectoryPath} has errored \n {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void InitializeSqlConnection()
        {
            _eventLog.WriteEntry($"SqlConnection initialization to server {serverName} on database {dbName} started");

            try
            {
                this.sqlConnection = new SqlConnection($"Server={serverName};Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true");
                _eventLog.WriteEntry($"SqlConnection initialization to server {serverName} on database {dbName} started");

            }
            catch (Exception ex)
            {
                _eventLog.WriteEntry($"SqlConnection to server {serverName} on database {dbName} has errored \n {ex.Message}", EventLogEntryType.Error);
            }
        }

        private string GetBackupFileLocation()
        {
            return $@"{this.SqlBackupDirectoryPath}{this.dbName}_{DateTime.Now:yyyy-MM-dd_HH-mm}.bak";
        }

    }
}
