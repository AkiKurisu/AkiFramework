namespace Kurisu.Framework.Events.Editor
{
    internal class TitleInfo : IRegisteredCallbackLine
    {
        public LineType Type => LineType.Title;
        public string Text { get; }
        public CallbackEventHandler CallbackHandler { get; }

        public TitleInfo(string text, CallbackEventHandler handler)
        {
            this.Text = text;
            CallbackHandler = handler;
        }
    }
}
