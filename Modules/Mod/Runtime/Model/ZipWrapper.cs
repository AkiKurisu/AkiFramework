using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using UnityEngine;
namespace Chris.Mod
{
    public class ZipWrapper
    {
        public static bool Zip(string[] _fileOrDirectoryArray, string _outputPathName, string _password = null)
        {
            if ((null == _fileOrDirectoryArray) || string.IsNullOrEmpty(_outputPathName))
            {

                return false;
            }

            ZipOutputStream zipOutputStream = new(File.Create(_outputPathName));
            zipOutputStream.SetLevel(6);
            if (!string.IsNullOrEmpty(_password))
                zipOutputStream.Password = _password;

            for (int index = 0; index < _fileOrDirectoryArray.Length; ++index)
            {
                bool result = false;
                string fileOrDirectory = _fileOrDirectoryArray[index];
                if (Directory.Exists(fileOrDirectory))
                    result = ZipDirectory(fileOrDirectory, string.Empty, zipOutputStream);
                else if (File.Exists(fileOrDirectory))
                    result = ZipFile(fileOrDirectory, string.Empty, zipOutputStream);

                if (!result)
                {
                    return false;
                }
            }

            zipOutputStream.Finish();
            zipOutputStream.Close();

            return true;
        }
        public static bool UnzipFile(string _filePathName, string _outputPath, string _password = null)
        {
            if (string.IsNullOrEmpty(_filePathName) || string.IsNullOrEmpty(_outputPath))
            {
                return false;
            }

            try
            {
                return UnzipFile(File.OpenRead(_filePathName), _outputPath, _password);
            }
            catch (Exception _e)
            {
                Debug.LogError("[ZipWrapper]: " + _e.ToString());

                return false;
            }
        }
        public static bool UnzipFile(byte[] _fileBytes, string _outputPath, string _password = null)
        {
            if ((null == _fileBytes) || string.IsNullOrEmpty(_outputPath))
            {
                return false;
            }

            bool result = UnzipFile(new MemoryStream(_fileBytes), _outputPath, _password);
            return result;
        }

        public static bool UnzipFile(Stream _inputStream, string _outputPath, string _password = null)
        {
            if ((null == _inputStream) || string.IsNullOrEmpty(_outputPath))
            {
                return false;
            }

            if (!Directory.Exists(_outputPath))
                Directory.CreateDirectory(_outputPath);
            using ZipInputStream zipInputStream = new(_inputStream);
            if (!string.IsNullOrEmpty(_password))
                zipInputStream.Password = _password;


            ZipEntry entry;
            while (null != (entry = zipInputStream.GetNextEntry()))
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                string filePathName = Path.Combine(_outputPath, entry.Name);

                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(filePathName);
                    continue;
                }

                try
                {
                    using FileStream fileStream = File.Create(filePathName);
                    byte[] bytes = new byte[1024];
                    while (true)
                    {
                        int count = zipInputStream.Read(bytes, 0, bytes.Length);
                        if (count > 0)
                            fileStream.Write(bytes, 0, count);
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception _e)
                {
                    Debug.LogError("[ZipWrapper]: " + _e.ToString());
                    return false;
                }
            }
            return true;
        }

        private static bool ZipFile(string _filePathName, string _parentRelPath, ZipOutputStream _zipOutputStream)
        {
            FileStream fileStream = null;
            try
            {
                string entryName = _parentRelPath + '/' + Path.GetFileName(_filePathName);
                ZipEntry entry = new(entryName)
                {
                    DateTime = DateTime.Now
                };

                fileStream = File.OpenRead(_filePathName);
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                fileStream.Close();

                entry.Size = buffer.Length;

                _zipOutputStream.PutNextEntry(entry);
                _zipOutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception _e)
            {
                Debug.LogError("[ZipWrapper]: " + _e.ToString());
                return false;
            }
            finally
            {
                if (null != fileStream)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }

            return true;
        }

        private static bool ZipDirectory(string _path, string _parentRelPath, ZipOutputStream _zipOutputStream)
        {
            try
            {
                string entryName = Path.Combine(_parentRelPath, Path.GetFileName(_path) + '/');
                ZipEntry entry = new(entryName)
                {
                    DateTime = DateTime.Now,
                    Size = 0
                };

                _zipOutputStream.PutNextEntry(entry);
                _zipOutputStream.Flush();

                string[] files = Directory.GetFiles(_path);
                for (int index = 0; index < files.Length; ++index)
                {
                    if (files[index].EndsWith(".meta") == true)
                    {
                        Debug.LogWarning(files[index] + " not to zip");
                        continue;
                    }

                    ZipFile(files[index], Path.Combine(_parentRelPath, Path.GetFileName(_path)), _zipOutputStream);
                }
            }
            catch (Exception _e)
            {
                Debug.LogError("[ZipWrapper]: " + _e.ToString());
                return false;
            }

            string[] directories = Directory.GetDirectories(_path);
            for (int index = 0; index < directories.Length; ++index)
            {
                if (!ZipDirectory(directories[index], Path.Combine(_parentRelPath, Path.GetFileName(_path)), _zipOutputStream))
                {
                    return false;
                }
            }
            return true;
        }
    }
}