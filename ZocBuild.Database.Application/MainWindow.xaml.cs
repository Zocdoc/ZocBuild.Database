using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
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
using ZocBuild.Database.Application.Controls;
using ZocBuild.Database.Application.Converters;
using ZocBuild.Database.Application.Settings;
using ZocBuild.Database.Application.ViewModels;
using ZocBuild.Database.ScriptRepositories;
using ZocBuild.Database.SqlParser;

namespace ZocBuild.Database.Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [Import]
        private IParser sqlParser;
        private readonly FileInfo pathToGit;

        public MainWindow()
        {
            InitializeComponent();

            pathToGit = new FileInfo(@"C:\Program Files (x86)\Git\bin\git.exe");
            try
            {
                DirectoryCatalog catalog =
                    new DirectoryCatalog(new FileInfo(Assembly.GetAssembly(typeof (MainWindow)).Location).DirectoryName);
                CompositionContainer container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
            catch (CompositionException ex)
            {
                sqlParser = null;
            }

            var vm = new MainWindowViewModel(Properties.Settings.Default.Databases.Select(x => x.Create(pathToGit, sqlParser)));
            vm.SourceChangeset = (DvcsScriptRepositoryBase.RevisionIdentifierBase)Properties.Settings.Default.LastChangeset ?? Properties.Settings.Default.LastTag;
            if (vm.SourceChangeset != null)
            {
                vm.SelectedSourceType = RevisionIdentifierConverter.GetLabelFromType(vm.SourceChangeset.GetType());
            }

            var riConverter = (RevisionIdentifierConverter)Resources["riConverter"];
            riConverter.DataContext = vm;
            DataContext = vm;
        }

        private void btAddDb_Click(object sender, RoutedEventArgs e)
        {
            var w = new DatabaseWindow(pathToGit, sqlParser);
            w.DataContext = DataContext;
            w.ShowDialog();
        }

        private void cbSourceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm != null && vm.SourceChangeset != null
                && vm.SelectedSourceType != RevisionIdentifierConverter.GetLabelFromType(vm.SourceChangeset.GetType()))
            {
                vm.SourceChangeset = null;
            }
        }

        private void btUpdate_Click(object sender, RoutedEventArgs e)
        {
            var db = ((MainWindowViewModel)DataContext).SelectedDatabase;
            var dvcsRepo = db.Scripts as DvcsScriptRepositoryBase;
            if(dvcsRepo != null)
            {
                dvcsRepo.SourceChangeset = ((MainWindowViewModel)DataContext).SourceChangeset;
                //dvcsRepo.DestinationChangeset = ((MainWindowViewModel)DataContext).DestinationChangeset;
            }
            ((MainWindowViewModel)DataContext).IsReady = false;
            Task.Factory.StartNew(() => Update(db));
        }

        private void btBuild_Click(object sender, RoutedEventArgs e)
        {
            var items = ((MainWindowViewModel)DataContext).Items.Select(x => x.Item);
            var db = ((MainWindowViewModel) DataContext).SelectedDatabase;
            ((MainWindowViewModel) DataContext).IsReady = false;
            Task.Factory.StartNew(() => Build(db, items));
        }

        private async Task Update(Database db)
        {
            var buildItems = await db.GetChangedBuildItemsAsync();
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ((MainWindowViewModel)DataContext).IsReady = true;
                ((MainWindowViewModel)DataContext).IsDone = false;
                ObservableCollection<BuildItemViewModel> itemsCollection = new ObservableCollection<BuildItemViewModel>
                    (buildItems.Select(x => new BuildItemViewModel(x, Dispatcher)));
                ((MainWindowViewModel)DataContext).Items = itemsCollection;
            }));
        }

        private async Task Build(Database db, IEnumerable<BuildItem> items)
        {
            await db.BuildAsync(items);
            Dispatcher.BeginInvoke((Action) (() =>
                {
                    ((MainWindowViewModel)DataContext).IsReady = true;
                    ((MainWindowViewModel)DataContext).IsDone = true;
                    var vm = ((MainWindowViewModel)DataContext);
                    Properties.Settings.Default.LastChangeset = vm.SourceChangeset as DvcsScriptRepositoryBase.ChangesetId;
                    Properties.Settings.Default.LastTag = vm.SourceChangeset as DvcsScriptRepositoryBase.Tag;
                    Properties.Settings.Default.Save();
                }));
        }
    }
}
