using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpToTSConverterWindow.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;

namespace CSharpToTSConverterWindow.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _typeContent = "";

        [ObservableProperty]
        private string _typeName = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Parents))]
        private string[] _parentTypes = { };

        [ObservableProperty]
        private string _generatedTypescript = "";

        public string Parents => string.Join(',', ParentTypes);

        public ObservableCollection<PropertyControlViewModel> CSharpProperties { get; set; } 
            = new ObservableCollection<PropertyControlViewModel>();

        public ObservableCollection<PropertyControlViewModel> TsProperties { get; set; }
            = new ObservableCollection<PropertyControlViewModel>();

        [RelayCommand]
        private void SelectFile()
        {
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = ".cs";
            dialog.Multiselect = false;

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                TypeContent = File.ReadAllText(dialog.FileName);
            }
        }

        [RelayCommand]
        private void SelectProperties()
        {
            var lines = TypeContent.Split('\n');

            var res = Converter.GetNameAndProperties(lines);

            ParentTypes = res.parentTypes;

            TypeName = res.Name;

            CSharpProperties.Clear();
            
            foreach(var prop in res.Properties)
            {
                CSharpProperties.Add(new PropertyControlViewModel(prop, RemoveCSharpProperty));
            }
        }

        [RelayCommand]
        private void GenerateTSProperties()
        {
            var props = CSharpProperties.Select(p => p.GetPropertyDescription()).ToList();

            var tsProps = Converter.ConvertCSharpPropertiesToTypeScript(props);

            TsProperties.Clear();

            foreach(var prop in tsProps)
            {
                TsProperties.Add(new PropertyControlViewModel(prop, RemoveTSProperty));
            }
        }

        [RelayCommand]
        private void GenerateTypescript()
        {
            var props = TsProperties.Select(p => p.GetPropertyDescription());

            GeneratedTypescript = Converter.GenerateTypeScript(props, TypeName, ParentTypes);
        }

        private void RemoveCSharpProperty(PropertyControlViewModel property)
        {
            CSharpProperties.Remove(property);
        }

        private void RemoveTSProperty(PropertyControlViewModel property)
        {
            TsProperties.Remove(property);
        }
    }
}
