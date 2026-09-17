using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Archives;
using Mutagen.Bethesda.Environments;
using WemBam.Contracts;
using WemBam.Database;
using WemBam.Models;
using WemBam.Services.Audio;

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
                                FileId = Path.GetFileNameWithoutExtension(file.Path),
                                FileName = Path.GetFileName(path),
                                FileExtension = Path.GetExtension(path),
                                Duration = null
                            };

                            AudioAsset? existingAudioAsset =
                                DatabaseManager.FindAudioAssetByFileId(
                                    audioAsset.FileId!);

                            if (existingAudioAsset == null)
                            {
                                audioAsset.Id =
                                    DatabaseManager.AddAudioAsset(audioAsset);
                            }
                            else
                            {
                                audioAsset.Id = existingAudioAsset.Id;
                                audioAsset.DefaultSourceId = existingAudioAsset.DefaultSourceId;
                            }

                            AudioAssetSource audioAssetSource = new()
                            {
                                AudioAssetId = audioAsset.Id,
                                SourceId = source.Id,
                                ContainerPath = source.Path,
                                AssetPath = path,
                                ContentHash = CalculateContentHash(file)
                            };

                            long audioAssetSourceId =
                                DatabaseManager.AddAudioAssetSource(audioAssetSource);

                            DatabaseManager.SetDefaultAudioAssetSource(
                                audioAsset.Id,
                                audioAssetSourceId);

                            AudioStreamRequest request = new()
                            {
                                ContainerPath = source.Path,
                                AssetPath = path
                            };

                            audioAsset.Duration =
                                AudioDurationService.GetDuration(request);

                            DatabaseManager.UpdateAudioAssetDuration(
                                audioAsset.Id,
                                audioAsset.Duration);

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

        private static string CalculateContentHash(
            IArchiveFile file)
        {
            using Stream stream = file.AsStream();

            byte[] hash =
                System.Security.Cryptography.SHA256.HashData(stream);

            return Convert.ToHexString(hash);
        }
    }
}