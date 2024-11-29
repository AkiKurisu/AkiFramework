using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.Networking;
namespace Chris.Mod
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
        public async UniTask DownloadMod(string url, string downloadPath)
        {
            Result result = new();
            using UnityWebRequest request = UnityWebRequest.Get(new Uri(url).AbsoluteUri);
            request.downloadHandler = new DownloadHandlerFile(downloadPath);
            using UnityWebRequest www = UnityWebRequest.Get(new Uri(url).AbsoluteUri);
            await www.SendWebRequest().ToUniTask(new Progress(this), cancellationToken: cancellationToken);
            string unzipFolder = Path.GetDirectoryName(downloadPath);
            if (!ZipWrapper.UnzipFile(downloadPath, unzipFolder))
            {
                result.errorInfo = $"Can't unzip mod: {downloadPath}!";
                File.Delete(downloadPath);
                result.downloadPath = downloadPath[..4];
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