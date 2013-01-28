using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ZocBuild.Database.Errors;

namespace ZocBuild.Database.Application.ViewModels
{
    class BuildItemViewModel : INotifyPropertyChanged
    {
        public BuildItemViewModel(BuildItem item, Dispatcher dispatcher)
        {
            Item = item;
            Item.StatusChanged += (sender, args) => dispatcher.BeginInvoke((Action)(() =>
            {
                NotifyPropertyChanged("Status");
                NotifyPropertyChanged("Error");
                NotifyPropertyChanged("ErrorTitle");
                NotifyPropertyChanged("ErrorMessage");
                NotifyPropertyChanged("ShowError");
            }));
        }

        public BuildItem Item { get; private set; }

        public TypedDatabaseObject DatabaseObject
        {
            get { return Item.DatabaseObject; }
        }

        public BuildItem.BuildStatusType Status
        {
            get { return Item.Status; }
        }

        public BuildErrorBase Error
        {
            get { return Item.Error; }
        }

        public string ErrorTitle
        {
            get
            {
                if (Item != null && Item.Error != null)
                {
                    return Item.Error.ErrorType;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string ErrorMessage
        {
            get
            {
                if (Item != null && Item.Error != null)
                {
                    return Item.Error.GetMessage();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool ShowError
        {
            get { return Item != null && Item.Error != null; }
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
