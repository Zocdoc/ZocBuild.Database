using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ZocBuild.Database.ScriptRepositories;

namespace ZocBuild.Database.Application.Converters
{
    class RevisionIdentifierConverter : FrameworkElement, IValueConverter
    {
        private const string ChangesetLabel = "Source Revision:";
        private const string TagLabel = "Source Tag:";

        internal static string GetLabelFromType(Type t)
        {
            if(t == typeof(DvcsScriptRepositoryBase.ChangesetId))
            {
                return ChangesetLabel;
            }
            else if(t == typeof(DvcsScriptRepositoryBase.Tag))
            {
                return TagLabel;
            }
            else
            {
                throw new ArgumentException("t");
            }
        }

        public static IEnumerable<string> IdentifierTypes
        {
            get
            {
                yield return ChangesetLabel;
                yield return TagLabel;
            }
        }

        public string SelectedType
        {
            get { return (string)GetValue(SelectedTypeProperty); }
            set { SetValue(SelectedTypeProperty, value); }
        }
        public static readonly DependencyProperty SelectedTypeProperty =
            DependencyProperty.Register("SelectedType", typeof(string), typeof(RevisionIdentifierConverter), new PropertyMetadata(TagLabel));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var id = value as DvcsScriptRepositoryBase.RevisionIdentifierBase;
            if (id != null)
            {
                return id.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as string;
            if(string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            if (SelectedType == ChangesetLabel)
            {
                try
                {
                    return new DvcsScriptRepositoryBase.ChangesetId(str);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
            else
            {
                return new DvcsScriptRepositoryBase.Tag(str);
            }
        }
    }
}
