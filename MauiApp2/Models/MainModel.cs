using GlobalDeclar;

namespace Maui.Models
{
    internal class MainModel:BaseNotifyChanged
    {
		private int val;

		public int Value
		{
			get { return val; }
			set {
                val = value;
				OnPropertyChanged();
			}
		}

	}
}
