using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gobchat.Updater
{
    internal static class DownloadHelper
    {
        public interface ProgressMonitor
        {
            void Progress(int progressPercentage, long bytesReceived, long totalBytesToReceive);

            void StartWork();

            void EndWork();
        }

        public static async Task<bool> DownloadFile(WebClient client, string url, string targetFile, ProgressMonitor progressMonitor = null)
        {
            var directory = Path.GetDirectoryName(targetFile);
            if (!Directory.Exists(directory))
                throw new ArgumentException($"Destination '{directory}' does not exist.", nameof(directory));

            void OnEvent_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                progressMonitor.Progress(e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive);
            }

            progressMonitor.StartWork();

            try
            {
                client.DownloadProgressChanged += OnEvent_ProgressChanged;
                await client.DownloadFileTaskAsync(url, targetFile).ConfigureAwait(false);
                return true;
            }
            finally
            {
                client.DownloadProgressChanged -= OnEvent_ProgressChanged;
                progressMonitor.EndWork();
            }
        }
    }
}