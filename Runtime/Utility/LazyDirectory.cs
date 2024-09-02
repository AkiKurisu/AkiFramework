using System.IO;
namespace Kurisu.Framework
{
    public class LazyDirectory
    {
        private readonly string path;
        private bool initialized;
        public LazyDirectory(string path)
        {
            this.path = path;
        }
        public string GetPath()
        {
            if (initialized)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                initialized = true;
            }
            return path;
        }
    }
}
