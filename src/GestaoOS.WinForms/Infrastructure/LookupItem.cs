namespace GestaoOS.WinForms.Infrastructure
{
    public class LookupItem<T>
    {
        public LookupItem(string text, T value)
        {
            Text = text;
            Value = value;
        }

        public string Text { get; private set; }
        public T Value { get; private set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
