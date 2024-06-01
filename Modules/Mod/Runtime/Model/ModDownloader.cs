using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.Networking;
namespace Kurisu.Framework.Mod
{
    public class ModDownloader : IDisposable
    {
        public readonly Subject<float> onProgress = new();
        public readonly Subject<Result> onComplete = new();
        private readonly CancellationToken cancellationToken;
        public ModDownloader(CancellationToken cancellationToken = default)
        {
            this.cancellationToken = cancellationToken;
        }
        public async UniTask DownloadMod(string url, string downloadFileName)
        {
            Result result = new();
            using UnityWebRequest request = UnityWebRequest.Get(new Uri(url).AbsoluteUri);
            string downloadPath = Path.Combine(ImportConstants.LoadingPath, downloadFileName);
            result.downloadPath = downloadPath.Replace(".zip", string.Empty);
            request.downloadHandler = new DownloadHandlerFile(downloadPath);
            using UnityWebRequest www = UnityWebRequest.Get(new Uri(url).AbsoluteUri);
            await www.SendWebRequest().ToUniTask(new Progress(this), cancellationToken: cancellationToken);
            if (!ZipWrapper.UnzipFile(downloadPath, ImportConstants.LoadingPath))
            {
                result.errorInfo = $"Can't unzip mod: {downloadPath}!";
                File.Delete(downloadPath);
                onComplete.OnNext(result);
                return;
            }
            result.success = true;
            onComplete.OnNext(result);
        }
        public void Dispose()
        {
            onProgress.Dispose();
            onComplete.Dispose();
        }
        public struct Result
        {
            public string errorInfo;
            public bool success;
            public string downloadPath;
        }
        private readonly struct Progress : IProgress<float>
        {
            private readonly ModDownloader downloader;
            public Progress(ModDownloader downloader)
            {
                this.downloader = downloader;
            }
            public void Report(float value)
            {
                downloader.onProgress.OnNext(value);
            }
        }
    }
}