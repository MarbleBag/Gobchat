using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Core.Resource
{
    public interface IResourceRepository
    {
        Uri Uri { get; }

        IResourceRepository GetRepository(string key);

        IEnumerable<IResourceAccessor> FindResource(string searchPattern);

        IResourceAccessor CreateResource(string resourceName);
    }

    public interface IResourceAccessor
    {
        bool CanRead { get; }

        bool CanWrite { get; }

        Uri Uri { get; }

        string ResourceName { get; }

        System.IO.Stream OpenWrite();

        System.IO.Stream OpenRead();
    }

    public class FileSystemResourceRepository : IResourceRepository
    {
        public static string FilePathToFileUrl(string filePath)
        {
            StringBuilder uri = new StringBuilder();
            foreach (char v in filePath)
            {
                if ((v >= 'a' && v <= 'z') || (v >= 'A' && v <= 'Z') || (v >= '0' && v <= '9') ||
                  v == '+' || v == '/' || v == ':' || v == '.' || v == '-' || v == '_' || v == '~' ||
                  v > '\xFF')
                {
                    uri.Append(v);
                }
                else if (v == System.IO.Path.DirectorySeparatorChar || v == System.IO.Path.AltDirectorySeparatorChar)
                {
                    uri.Append('/');
                }
                else
                {
                    uri.Append(String.Format("%{0:X2}", (int)v));
                }
            }
            if (uri.Length >= 2 && uri[0] == '/' && uri[1] == '/') // UNC path
                uri.Insert(0, "file:");
            else
                uri.Insert(0, "file:///");
            return uri.ToString();
        }

        public Uri Uri { get; }

        private string _folder;

        public FileSystemResourceRepository(string folder)
        {
            _folder = folder;
            Uri = new Uri(FilePathToFileUrl(folder));
        }

        public IEnumerable<IResourceAccessor> FindResource(string searchPattern)
        {
            throw new NotImplementedException();
        }

        public IResourceRepository GetRepository(string key)
        {
            var path = System.IO.Path.Combine(_folder, key);
            return new FileSystemResourceRepository(path);
        }

        public IResourceAccessor CreateResource(string resourceName)
        {
            throw new NotImplementedException();
        }
    }
}