using System.IO;
using System.Linq;
using Kurisu.Framework.Collections;
using UnityEngine;
namespace Kurisu.Framework.Resource
{
    public struct ResourceReference
    {
        public int fileId;
        public int directoryId;
        public int version;
    }
    public class ResourceDirectory
    {
        /// <summary>
        /// Path of this directory
        /// </summary>
        public string DirectoryPath;
        /// <summary>
        /// Locator id
        /// </summary>
        public int LocatorId;
        /// <summary>
        /// Files path
        /// </summary>
        public string[] Files;
        /// <summary>
        /// Ref for this directory
        /// </summary>
        public ResourceReference Ref;
        /// <summary>
        /// Get virtual file references
        /// </summary>
        /// <returns></returns>
        public ResourceReference[] GetReferences()
        {
            return Files.Select((x, id) => new ResourceReference()
            {
                fileId = id, 
                directoryId = Ref.directoryId, 
                version = Ref.version
            }).ToArray();
        }
    }
    public interface IResourceLocator
    {
        /// <summary>
        /// Create virtual directory from path
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        ResourceDirectory CreateDirectory(string directoryPath);
        /// <summary>
        /// Load stream from virtual directory
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Stream GetStream(ResourceDirectory directory, int fileId);
    }
    internal class AddressableResourceLocator : IResourceLocator
    {
        public ResourceDirectory CreateDirectory(string directoryPath)
        {
            using var textAsset = ResourceSystem.AsyncLoadAsset<TextAsset>(directoryPath);
            var lines = textAsset.WaitForCompletion().text;
            var dir = new ResourceDirectory()
            {
                DirectoryPath = directoryPath,
                LocatorId = ResourceDataBase.AddressableLocatorId,
                Files = lines.Split('\n').Where(x => !string.IsNullOrEmpty(x)).ToArray()
            };
            return dir;
        }
        public Stream GetStream(ResourceDirectory directory, int fileId)
        {
            using var handle = ResourceSystem.AsyncLoadAsset<TextAsset>(directory.Files[fileId]);
            var ta = handle.WaitForCompletion();
            return new MemoryStream(ta.bytes);
        }
    }
    internal class FileResourceLocator : IResourceLocator
    {
        public ResourceDirectory CreateDirectory(string directoryPath)
        {
            var dir = new ResourceDirectory()
            {
                DirectoryPath = directoryPath,
                LocatorId = ResourceDataBase.FileLocatorId,
                Files = System.IO.Directory.GetFiles(directoryPath).Select(x=>Path.GetRelativePath(directoryPath,x)).ToArray()
            };
            return dir;
        }
        public Stream GetStream(ResourceDirectory directory, int fileId)
        {
            return new FileStream(Path.Combine(directory.DirectoryPath,directory.Files[fileId]), FileMode.Open);
        }
    }

    public class ResourceDataBase
    {
        public static int FileLocatorId = 0;
        public static int AddressableLocatorId = 1;
        public IResourceLocator[] Locators;
        public SparseList<ResourceDirectory> Directories;
        private int version;
        public ResourceDataBase()
        {
            Locators = new IResourceLocator[] { new FileResourceLocator(), new AddressableResourceLocator() };
            Directories = new SparseList<ResourceDirectory>(10,1000);
        }
        /// <summary>
        /// Load stream from dataBase
        /// </summary>
        /// <param name="rr"></param>
        /// <returns></returns>
        public Stream GetStream(ResourceReference rr)
        {
            var dir = Directories[rr.directoryId];
            if (dir == null) return null;
            if (dir.Ref.version != rr.version) return null;
            var locator = Locators[dir.LocatorId];
            return locator.GetStream(dir, rr.fileId);
        }
        /// <summary>
        /// Create directory in dataBase
        /// </summary>
        /// <param name="locatorId"></param>
        /// <param name="directoryPath"></param>
        public void CreateDirectory(int locatorId, string directoryPath)
        {
            var dir = Locators[locatorId].CreateDirectory(directoryPath);
            var RefId = Directories.AddUninitialized();
            dir.Ref = new ResourceReference() { directoryId = RefId, version = version };
            Directories[RefId] = dir;
        }
    }
}
