using Maui.Models;

namespace Maui.ViewModels
{
    internal class MainViewModel
    {
        public MainModel model = new MainModel();
        public Command ButtonCommand { get; } = new Command(() =>
        {
            // Command logic here
        });
    }
}
