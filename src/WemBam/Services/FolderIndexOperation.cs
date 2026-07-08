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
    public class FolderIndexOperation : IBackgroundOperation
    {
        private readonly IReadOnlyCollection<Source> _sources;

        public FolderIndexOperation(
            IEnumerable<Source> sources)
        {
            _sources = sources
                .Where(source => source.Type == SourceType.Folder)
                .ToList();
        }

        public async Task<BackgroundOperationResult> ExecuteAsync(
            IProgress<BackgroundTaskProgress> progress,
            CancellationToken cancellationToken)
        {
            DateTimeOffset startedAt = DateTimeOffset.UtcNow;

            progress.Report(new BackgroundTaskProgress
            {
                StartedAt = startedAt,
                StatusMessage = "Scanning folders..."
            });

            BackgroundOperationResult result =
                await Task.Run(() =>
                {
                    List<string> matchingFiles = new();

                    foreach (Source source in _sources)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!Directory.Exists(source.Path))
                        {
                            continue;
                        }

                        matchingFiles.AddRange(
                            Directory.EnumerateFiles(
                                source.Path,
                                "*.wem",
                                SearchOption.AllDirectories));

                        matchingFiles.AddRange(
                            Directory.EnumerateFiles(
                                source.Path,
                                "*.ba2",
                                SearchOption.AllDirectories));
                    }

                    int totalItems = matchingFiles.Count;
                    int processed = 0;

                    progress.Report(new BackgroundTaskProgress
                    {
                        StartedAt = startedAt,
                        StatusMessage = "Processing files...",
                        ItemsProcessed = 0,
                        TotalItems = totalItems
                    });

                    const int BatchSize = 50;

                    for (int i = 0; i < matchingFiles.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // Placeholder for future indexing work.

                        processed++;

                        if (processed % BatchSize == 0 ||
                            processed == totalItems)
                        {
                            progress.Report(new BackgroundTaskProgress
                            {
                                StartedAt = startedAt,
                                StatusMessage = "Processing files...",
                                ItemsProcessed = processed,
                                TotalItems = totalItems
                            });
                        }
                    }

                    return BackgroundOperationResult.Completed(
                        processed,
                        "Index completed.");
                },
                cancellationToken);

            return result;
        }
    }
}