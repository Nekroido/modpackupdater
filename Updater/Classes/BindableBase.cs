using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Updater.Classes
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface ILoadingPlaceholder
    {
        bool IsLoading { get; set; }
    }

    public interface IListAd
    {
        bool IsAd { get; set; }
    }
}
