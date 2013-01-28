using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZocBuild.Database.Application.Settings;
using ZocBuild.Database.Application.ViewModels;

namespace ZocBuild.Database.Application.Controls
{
    /// <summary>
    /// Interaction logic for AddDatabaseView.xaml
    /// </summary>
    public partial class AddDatabaseView : UserControl
    {
        public AddDatabaseView()
        {
            InitializeComponent();
            DataContext = new AddDatabaseViewModel();
        }

        public DatabaseSetting Result
        {
            get { return ((AddDatabaseViewModel)DataContext).Database; }
        }

        public event EventHandler DatabaseAdded;
        public event EventHandler Cancelled;

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            if (DatabaseAdded != null)
            {
                DatabaseAdded(this, new EventArgs());
            }
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            if(Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btBrowse_Click(object sender, RoutedEventArgs e)
        {
            var d = new System.Windows.Forms.FolderBrowserDialog();
            var result = d.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ((AddDatabaseViewModel)DataContext).ScriptsPath = d.SelectedPath;
            }
        }
    }
}
