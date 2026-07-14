using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WemBam.Contracts;
using WemBam.Models;

namespace WemBam.Services
{
    public class BA2DiscoveryOperation : IBackgroundOperation
    {
        private readonly IReadOnlyCollection<Source> _sources;

        public BA2DiscoveryOperation(
            IEnumerable<Source> sources)
        {
            _sources = sources
                .Where(source =>
                    source.Type == SourceType.File &&
                    string.Equals(
                        Path.GetExtension(source.Path),
                        ".ba2",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public Task<BackgroundOperationResult> ExecuteAsync(
            IProgress<BackgroundTaskProgress> progress,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            progress.Report(new BackgroundTaskProgress
            {
                StatusMessage = "Scanning BA2 archives..."
            });

            return Task.FromResult(
                BackgroundOperationResult.Completed(
                    0,
                    "BA2 discovery completed."));
        }
    }
}