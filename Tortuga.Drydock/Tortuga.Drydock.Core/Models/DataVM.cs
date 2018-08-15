using Tortuga.Sails;

namespace Tortuga.Drydock.Models
{
    public class DataVM : ViewModelBase
    {
        public string WindowTitle { get => Get<string>(); set => Set(value); }
        public object Data { get => Get<object>(); set => Set(value); }
    }
}