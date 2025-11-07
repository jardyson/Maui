namespace GlobalDeclar
{
    public class OptionBuilder : BaseNotifyChanged
    {
        private static OptionBuilder _;

        public static OptionBuilder Instance => _ ?? (_ = new OptionBuilder());

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string code;
        public string Code
        {
            get { return code; }
            set
            {
                code = value;
                OnPropertyChanged();
            }
        }

        public List<Network> network { get; set; }

        public OptionBuilder Create(string name, string code)
        {
            return new OptionBuilder
            {
                Name = name,
                Code = code,
                network = new List<Network>()
            };
        }

        public static Option<T> Create<T>(string name, T code)
        {
            var a = new Option<T>()
            {
                Name = name,
                Code = code
            };

            return a;
        }
    }
}