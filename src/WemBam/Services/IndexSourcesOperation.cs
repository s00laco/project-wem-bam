using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WemBam.Contracts;
using WemBam.Database;
using WemBam.Models;

namespace WemBam.Services
{
    public class IndexSourcesOperation : IBackgroundOperation
    {
        private readonly FolderDiscoveryOperation _folderDiscoveryOperation;
        private readonly BA2DiscoveryOperation _ba2DiscoveryOperation;

        public IndexSourcesOperation(
            IEnumerable<Source> sources)
        {
            _folderDiscoveryOperation = new FolderDiscoveryOperation(sources);
            _ba2DiscoveryOperation = new BA2DiscoveryOperation(sources);
        }

        public async Task<BackgroundOperationResult> ExecuteAsync(
            IProgress<BackgroundTaskProgress> progress,
            CancellationToken cancellationToken)
        {
            DatabaseManager.ClearAudioAssets();

            BackgroundOperationResult folderResult =
                await _folderDiscoveryOperation.ExecuteAsync(
                    progress,
                    cancellationToken);

            if (folderResult.State != BackgroundTaskState.Completed)
            {
                return folderResult;
            }

            cancellationToken.ThrowIfCancellationRequested();

            BackgroundOperationResult ba2Result =
                await _ba2DiscoveryOperation.ExecuteAsync(
                    progress,
                    cancellationToken);

            if (ba2Result.State != BackgroundTaskState.Completed)
            {
                return ba2Result;
            }

            return BackgroundOperationResult.Completed(
                folderResult.ItemsProcessed +
                ba2Result.ItemsProcessed,
                "Index completed.");
        }
    }
}