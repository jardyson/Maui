namespace GlobalDeclar
{
    public class Option<T>
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private T code;

        public T Code
        {
            get { return code; }
            set { code = value; }
        }
    }
}