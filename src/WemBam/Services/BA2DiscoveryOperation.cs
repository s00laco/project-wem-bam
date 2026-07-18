using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WemBam.Contracts;
using WemBam.Models;
using WemBam.Database;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Archives;
using Mutagen.Bethesda.Environments;

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

        public async Task<BackgroundOperationResult> ExecuteAsync(
            IProgress<BackgroundTaskProgress> progress,
            CancellationToken cancellationToken)
        {
            DateTimeOffset startedAt = DateTimeOffset.UtcNow;

            progress.Report(new BackgroundTaskProgress
            {
                StartedAt = startedAt,
                StatusMessage = "Scanning BA2 archives..."
            });

            BackgroundOperationResult result =
                await Task.Run(() =>
                {
                    int processed = 0;
                    int totalItems = 0;

                    foreach (Source source in _sources)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!File.Exists(source.Path))
                        {
                            continue;
                        }

                        var archive = Archive.CreateReader(
                            GameRelease.Starfield,
                            source.Path);

                        totalItems += archive.Files.Count(file =>
                            file.Path.EndsWith(
                                ".wem",
                                StringComparison.OrdinalIgnoreCase));
                    }

                    progress.Report(new BackgroundTaskProgress
                    {
                        StartedAt = startedAt,
                        StatusMessage = "Indexing audio assets...",
                        ItemsProcessed = 0,
                        TotalItems = totalItems
                    });

                    const int BatchSize = 50;

                    foreach (Source source in _sources)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!File.Exists(source.Path))
                        {
                            continue;
                        }

                        var archive = Archive.CreateReader(
                            GameRelease.Starfield,
                            source.Path);

                        foreach (var file in archive.Files)
                        {
                            string path = file.Path;

                            if (!path.EndsWith(
                                ".wem",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            AudioAsset audioAsset = new()
                            {
                                SourceId = source.Id,
                                FileName = Path.GetFileName(path),
                                FileExtension = Path.GetExtension(path),
                                ContainerPath = source.Path,
                                AssetPath = path,
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

                    }

                    return BackgroundOperationResult.Completed(processed);
                },
                cancellationToken);

            return result;
        }
    }
}