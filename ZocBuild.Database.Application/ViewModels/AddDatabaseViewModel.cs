using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Application.Settings;

namespace ZocBuild.Database.Application.ViewModels
{
    class AddDatabaseViewModel : INotifyPropertyChanged
    {
        public AddDatabaseViewModel()
        {
            Database = new DatabaseSetting();
        }

        public DatabaseSetting Database { get; private set; }

        public string ServerName
        {
            get { return Database.ServerName; }
            set
            {
                Database.ServerName = value;
                NotifyPropertyChanged("ServerName");
                NotifyPropertyChanged("CanAdd");
            }
        }

        public string DatabaseName
        {
            get { return Database.DatabaseName; }
            set
            {
                Database.DatabaseName = value;
                NotifyPropertyChanged("DatabaseName");
                NotifyPropertyChanged("CanAdd");
            }
        }

        public string ConnectionString
        {
            get { return Database.ConnectionString; }
            set
            {
                Database.ConnectionString = value;
                NotifyPropertyChanged("ConnectionString");
                NotifyPropertyChanged("CanAdd");
            }
        }

        public string ScriptsPath
        {
            get { return Database.ScriptsPath; }
            set
            {
                Database.ScriptsPath = value;
                NotifyPropertyChanged("ScriptsPath");
                NotifyPropertyChanged("CanAdd");
            }
        }

        public bool CanAdd
        {
            get
            {
                return (!string.IsNullOrWhiteSpace(ServerName))
                    && (!string.IsNullOrWhiteSpace(DatabaseName))
                    && (!string.IsNullOrWhiteSpace(ConnectionString))
                    && (!string.IsNullOrWhiteSpace(ScriptsPath));
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
