namespace Kurisu.Framework.Events.Editor
{
    internal class CallbackInfo : IRegisteredCallbackLine
    {
        public LineType Type => LineType.Callback;
        public string Text { get; }
        public CallbackEventHandler CallbackHandler { get; }

        public CallbackInfo(string text, CallbackEventHandler handler)
        {
            this.Text = text;
            CallbackHandler = handler;
        }
    }
}
