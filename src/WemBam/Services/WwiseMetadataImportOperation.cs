using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WemBam.Contracts;
using WemBam.Database;
using WemBam.Models;

namespace WemBam.Services
{
    public class WwiseMetadataImportOperation : IBackgroundOperation
    {
        private readonly string _jsonPath;

        public WwiseMetadataImportOperation(
            string jsonPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);

            _jsonPath = jsonPath;
        }

        public async Task<BackgroundOperationResult> ExecuteAsync(
            IProgress<BackgroundTaskProgress> progress,
            CancellationToken cancellationToken)
        {
            DateTimeOffset startedAt =
                DateTimeOffset.UtcNow;

            progress.Report(new BackgroundTaskProgress
            {
                StartedAt = startedAt,
                StatusMessage = "Reading Wwise metadata..."
            });

            BackgroundOperationResult result =
                await Task.Run(
                    () => Import(
                        startedAt,
                        progress,
                        cancellationToken),
                    cancellationToken);

            return result;
        }

        private BackgroundOperationResult Import(
            DateTimeOffset startedAt,
            IProgress<BackgroundTaskProgress> progress,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(_jsonPath))
            {
                throw new FileNotFoundException(
                    "The specified SoundBanksInfo.json file was not found.",
                    _jsonPath);
            }

            using FileStream stream =
                File.OpenRead(_jsonPath);

            using JsonDocument document =
                JsonDocument.Parse(stream);

            List<WwiseEvent> events = new();

            Dictionary<string, WwiseStreamedFile> streamedFiles =
                new(StringComparer.Ordinal);

            HashSet<(string WwiseEventId, string FileId)> relationships =
                new();

            JsonElement root =
                document.RootElement;

            if (!root.TryGetProperty(
                    "SoundBanks",
                    out JsonElement soundBanks))
            {
                throw new InvalidDataException(
                    "SoundBanksInfo.json does not contain a SoundBanks array.");
            }

            foreach (JsonElement soundBank in soundBanks.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!soundBank.TryGetProperty(
                        "IncludedMemoryFiles",
                        out JsonElement includedMemoryFiles))
                {
                    continue;
                }

                foreach (
                    JsonElement memoryFile
                    in includedMemoryFiles.EnumerateArray())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!memoryFile.TryGetProperty(
                            "IncludedEvents",
                            out JsonElement includedEvents))
                    {
                        continue;
                    }

                    foreach (
                        JsonElement eventElement
                        in includedEvents.EnumerateArray())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        WwiseEvent wwiseEvent =
                            ParseEvent(eventElement);

                        events.Add(wwiseEvent);

                        if (!eventElement.TryGetProperty(
                                "ReferencedStreamedFiles",
                                out JsonElement referencedStreamedFiles))
                        {
                            continue;
                        }

                        foreach (
                            JsonElement streamedFileElement
                            in referencedStreamedFiles.EnumerateArray())
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            WwiseStreamedFile streamedFile =
                                ParseStreamedFile(
                                    streamedFileElement);

                            if (!streamedFiles.ContainsKey(
                                    streamedFile.FileId))
                            {
                                streamedFiles.Add(
                                    streamedFile.FileId,
                                    streamedFile);
                            }

                            relationships.Add(
                                (
                                    wwiseEvent.Id,
                                    streamedFile.FileId));
                        }
                    }
                }
            }

            progress.Report(new BackgroundTaskProgress
            {
                StartedAt = startedAt,
                StatusMessage = "Replacing Wwise metadata...",
                ItemsProcessed = 0,
                TotalItems = events.Count
            });

            DatabaseManager.ReplaceWwiseMetadata(
                events,
                streamedFiles.Values,
                relationships,
                cancellationToken);

            return BackgroundOperationResult.Completed(
                events.Count);
        }

        private static WwiseEvent ParseEvent(
            JsonElement eventElement)
        {
            string id =
                eventElement.GetProperty("Id")
                    .GetString() ??
                string.Empty;

            string name =
                eventElement.GetProperty("Name")
                    .GetString() ??
                string.Empty;

            string objectPath =
                eventElement.GetProperty("ObjectPath")
                    .GetString() ??
                string.Empty;

            string durationType =
                eventElement.GetProperty("DurationType")
                    .GetString() ??
                string.Empty;

            double? durationMin =
                ParseNullableDouble(
                    eventElement,
                    "DurationMin");

            double? durationMax =
                ParseNullableDouble(
                    eventElement,
                    "DurationMax");

            return new WwiseEvent
            {
                Id = id,
                Name = name,
                ObjectPath = objectPath,
                DurationType = durationType,
                DurationMin = durationMin,
                DurationMax = durationMax
            };
        }

        private static WwiseStreamedFile ParseStreamedFile(
            JsonElement streamedFileElement)
        {
            string fileId =
                streamedFileElement.GetProperty("Id")
                    .GetString() ??
                string.Empty;

            string language =
                streamedFileElement.GetProperty("Language")
                    .GetString() ??
                string.Empty;

            string shortName =
                streamedFileElement.GetProperty("ShortName")
                    .GetString() ??
                string.Empty;

            string path =
                streamedFileElement.GetProperty("Path")
                    .GetString() ??
                string.Empty;

            return new WwiseStreamedFile
            {
                FileId = fileId,
                Language = language,
                ShortName = shortName,
                Path = path
            };
        }

        private static double? ParseNullableDouble(
            JsonElement element,
            string propertyName)
        {
            if (!element.TryGetProperty(
                    propertyName,
                    out JsonElement value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.GetDouble();
            }

            if (value.ValueKind == JsonValueKind.String &&
                double.TryParse(
                    value.GetString(),
                    out double parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}