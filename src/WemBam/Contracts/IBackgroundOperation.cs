using System;
using System.Threading;
using System.Threading.Tasks;
using WemBam.Models;

namespace WemBam.Contracts
{
    public interface IBackgroundOperation
    {
        Task<BackgroundOperationResult> ExecuteAsync(
            IProgress<BackgroundTaskProgress> progress,
            CancellationToken cancellationToken);
    }
}