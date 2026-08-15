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
                    Dictionary<string, Source> discoveredFiles = new(
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
                            discoveredFiles.TryAdd(filePath, source);
                        }
                    }

                    int totalItems = discoveredFiles.Count;
                    int processed = 0;

                    progress.Report(new BackgroundTaskProgress
                    {
                        StartedAt = startedAt,
                        StatusMessage = "Indexing audio assets...",
                        ItemsProcessed = 0,
                        TotalItems = totalItems
                    });

                    const int BatchSize = 50;

                    foreach ((string filePath, Source source) in discoveredFiles)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        AudioAsset audioAsset = new()
                        {
                            FileId = Path.GetFileNameWithoutExtension(filePath),
                            FileName = Path.GetFileName(filePath),
                            FileExtension = Path.GetExtension(filePath),
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
                            ContainerPath = null,
                            AssetPath = filePath,
                            ContentHash = CalculateContentHash(filePath)
                        };

                        long audioAssetSourceId =
                            DatabaseManager.AddAudioAssetSource(audioAssetSource);

                        if (audioAsset.DefaultSourceId == null)
                        {
                            audioAsset.DefaultSourceId = audioAssetSourceId;

                            DatabaseManager.SetDefaultAudioAssetSource(
                                audioAsset.Id,
                                audioAssetSourceId);
                        }

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

        private static string CalculateContentHash(
    string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);

            byte[] hash =
                System.Security.Cryptography.SHA256.HashData(stream);

            return Convert.ToHexString(hash);
        }
    }
}