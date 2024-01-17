namespace Kurisu.Framework
{
    public class Command
    {
        /// <summary>
        /// Get command from pool
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : BaseCommand, new()
        {
            return PoolManager.Instance.GetObject<T>();
        }
    }
}
