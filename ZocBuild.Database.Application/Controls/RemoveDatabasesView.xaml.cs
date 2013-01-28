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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZocBuild.Database.Application.ViewModels;

namespace ZocBuild.Database.Application.Controls
{
    /// <summary>
    /// Interaction logic for RemoveDatabasesView.xaml
    /// </summary>
    public partial class RemoveDatabasesView : UserControl
    {
        public RemoveDatabasesView()
        {
            InitializeComponent();
        }

        public event EventHandler Closed;

        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            if (Closed != null)
            {
                Closed(this, new EventArgs());
            }
        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            var db = ((Button) sender).DataContext as Database;
            if(vm != null && db != null)
            {
                if(vm.SelectedDatabase == db)
                {
                    vm.SelectedDatabase = null;
                }
                vm.Databases.Remove(db);
                var setting = Properties.Settings.Default.Databases.FirstOrDefault(x => x.IsSettingForDatabase(db));
                Properties.Settings.Default.Databases.Remove(setting);
                Properties.Settings.Default.Save();
            }
        }
    }
}
