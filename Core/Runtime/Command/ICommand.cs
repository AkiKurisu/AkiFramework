namespace Kurisu.Framework
{
    /// <summary>
    /// Interface  for command-based architecture
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Execute this command
        /// </summary>
        void Execute();
    }
    public interface IHandleCommand<T> where T : ICommand
    {
        /// <summary>
        /// Receive command type of <see cref="T"/>
        /// </summary>
        /// <param name="command"></param>
        void Handle(T command);
    }
    public static class CommandExtension
    {
        /// <summary>
        /// Send command to handler
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandHandler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Send<T>(this T command, IHandleCommand<T> commandHandler) where T : ICommand
        {
            commandHandler.Handle(command);
            return command;
        }
    }
}