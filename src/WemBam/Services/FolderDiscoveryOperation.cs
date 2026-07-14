using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WemBam.Contracts;
using WemBam.Models;
using WemBam.Database;

namespace WemBam.Services
{
    public class FolderDiscoveryOperation : IBackgroundOperation
    {
        private readonly IReadOnlyCollection<Source> _sources;

        public FolderDiscoveryOperation(
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
                StatusMessage = "Scanning folder sources..."
            });

            BackgroundOperationResult result =
                await Task.Run(() =>
                {
                    HashSet<string> matchingFiles = new(
                        StringComparer.OrdinalIgnoreCase);

                    foreach (Source source in _sources)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!Directory.Exists(source.Path))
                        {
                            continue;
                        }

                        foreach (string filePath in Directory.EnumerateFiles(
                                     source.Path,
                                     "*.wem",
                                     SearchOption.AllDirectories))
                        {
                            matchingFiles.Add(filePath);
                        }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
                    }

                    int totalItems = matchingFiles.Count;
                    int processed = 0;

                    progress.Report(new BackgroundTaskProgress
                    {
                        StartedAt = startedAt,
                        StatusMessage = "Indexing audio assets...",
                        ItemsProcessed = 0,
                        TotalItems = totalItems
                    });

                    const int BatchSize = 50;

                    foreach (string filePath in matchingFiles)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        AudioAsset audioAsset = new()
                        {
                            SourceId = 0,
                            FileName = Path.GetFileName(filePath),
                            FileExtension = Path.GetExtension(filePath),
                            ContainerPath = null,
                            AssetPath = filePath,
                            Duration = null
                        };

                        DatabaseManager.AddAudioAsset(audioAsset);

                        processed++;

                        if (processed % BatchSize == 0 ||
                            processed == totalItems)
                        {
                            progress.Report(new BackgroundTaskProgress
                            {
                                StartedAt = startedAt,
                                StatusMessage = "Indexing audio assets...",
                                ItemsProcessed = processed,
                                TotalItems = totalItems
                            });
                        }
                    }

                    return BackgroundOperationResult.Completed(
                        processed);
                },
                cancellationToken);

            return result;
        }
    }
}