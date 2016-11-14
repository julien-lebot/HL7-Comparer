using System.Collections;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace HL7Comparer.Views
{
    /// <summary>
    /// Interaction logic for SimpleListEditor.xaml
    /// </summary>
    public partial class SimpleListEditor : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(SimpleListEditor), new PropertyMetadata(default(string)));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(SimpleListEditor), new PropertyMetadata(default(IEnumerable)));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(SimpleListEditor), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty AddItemCommandProperty = DependencyProperty.Register(
            "AddItemCommand", typeof(ICommand), typeof(SimpleListEditor), new PropertyMetadata(default(ICommand)));

        public ICommand AddItemCommand
        {
            get { return (ICommand) GetValue(AddItemCommandProperty); }
            set { SetValue(AddItemCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteItemCommandProperty = DependencyProperty.Register(
            "DeleteItemCommand", typeof(ICommand), typeof(SimpleListEditor), new PropertyMetadata(default(ICommand)));

        public ICommand DeleteItemCommand
        {
            get { return (ICommand)GetValue(DeleteItemCommandProperty); }
            set { SetValue(DeleteItemCommandProperty, value); }
        }

        public static readonly DependencyProperty SaveCommandProperty = DependencyProperty.Register(
            "SaveCommand", typeof(ICommand), typeof(SimpleListEditor), new PropertyMetadata(default(ICommand)));

        public ICommand SaveCommand
        {
            get { return (ICommand)GetValue(SaveCommandProperty); }
            set { SetValue(SaveCommandProperty, value); }
        }

        public static readonly DependencyProperty DiscardCommandProperty = DependencyProperty.Register(
            "DiscardCommand", typeof(ICommand), typeof(SimpleListEditor), new PropertyMetadata(default(ICommand)));

        public ICommand DiscardCommand
        {
            get { return (ICommand)GetValue(DiscardCommandProperty); }
            set { SetValue(DiscardCommandProperty, value); }
        }

        public SimpleListEditor()
        {
            InitializeComponent();
        }

        private void DiscardClicked(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(this, this);
            // Bit of a hack really, but do that so that if
            // the handler command executes synchronously
            // the UI doesn't show the items source being
            // reset.
            Task.Run(() => DiscardCommand?.Execute(null));
        }

        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(this, this);
            SaveCommand?.Execute(null);
        }
    }
}
