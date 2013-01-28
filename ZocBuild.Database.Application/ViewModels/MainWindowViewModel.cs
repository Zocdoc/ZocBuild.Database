using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ZocBuild.Database.ScriptRepositories;

namespace ZocBuild.Database.Application.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private Database selectedDatabase;
        private DvcsScriptRepositoryBase.RevisionIdentifierBase sourceChangeset;
        private string selectedSourceType;
        //private DvcsScriptRepositoryBase.ChangesetId destinationChangeset;
        private ObservableCollection<BuildItemViewModel> items;
        private bool isReady;
        private bool isDone;

        public MainWindowViewModel(IEnumerable<Database> databases)
        {
            Databases = new ObservableCollection<Database>(databases);
            SelectedDatabase = Databases.FirstOrDefault();
            isReady = true;
        }

        public ObservableCollection<Database> Databases { get; private set; }
        public Database SelectedDatabase
        {
            get { return selectedDatabase; }
            set
            {
                selectedDatabase = value;
                Items = null;
                NotifyPropertyChanged("SelectedDatabase");
                NotifyPropertyChanged("CanUpdate");
                NotifyPropertyChanged("CanBuild");
            }
        }

        public bool CanUpdate
        {
            get { return IsReady && SelectedDatabase != null; }
        }

        public bool CanBuild
        {
            get { return IsReady && SelectedDatabase != null && Items != null && !IsDone; }
        }

        public bool IsReady
        {
            get { return isReady; }
            set
            {
                isReady = value;
                NotifyPropertyChanged("IsReady");
                NotifyPropertyChanged("CanBuild");
                NotifyPropertyChanged("CanUpdate");
            }
        }

        public bool IsDone
        {
            get { return isDone; }
            set
            {
                isDone = value;
                NotifyPropertyChanged("IsDone");
                NotifyPropertyChanged("CanBuild");
            }
        }

        public DvcsScriptRepositoryBase.RevisionIdentifierBase SourceChangeset
        {
            get { return sourceChangeset; }
            set
            {
                sourceChangeset = value;
                NotifyPropertyChanged("SourceChangeset");
            }
        }

        public string SelectedSourceType
        {
            get { return selectedSourceType; }
            set
            {
                selectedSourceType = value;
                NotifyPropertyChanged("SelectedSourceType");
            }
        }

        /*
        public DvcsScriptRepositoryBase.ChangesetId DestinationChangeset
        {
            get { return destinationChangeset; }
            set
            {
                destinationChangeset = value;
                NotifyPropertyChanged("DestinationChangeset");
            }
        }
        */

        public ObservableCollection<BuildItemViewModel> Items
        {
            get { return items; }
            set
            {
                items = value;
                NotifyPropertyChanged("Items");
                NotifyPropertyChanged("CanBuild");
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

    }
}
