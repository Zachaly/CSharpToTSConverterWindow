using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpToTSConverterWindow.Model;
using System.Windows.Input;

namespace CSharpToTSConverterWindow.ViewModel
{
    public partial class PropertyControlViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _type;

        [ObservableProperty]
        private bool _isNullable;

        public ICommand OnDelete { get; }

        public PropertyControlViewModel(PropertyDescription propertyDescription, Action<PropertyControlViewModel> delete)
        {
            Name = propertyDescription.Name;
            Type = propertyDescription.Type;
            IsNullable = propertyDescription.IsNullable;
            OnDelete = new RelayCommand(() => delete(this));
        }

        public PropertyDescription GetPropertyDescription()
            => new PropertyDescription(Name, Type, IsNullable);
    }
}
