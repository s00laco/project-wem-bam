using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;

namespace WemBam.Models
{
    public class SearchResult : INotifyPropertyChanged
    {
        public string FileName { get; set; } = string.Empty;

        public long AudioAssetId { get; set; }

        public string WwisePath { get; set; } = string.Empty;

        public string? ContainerPath { get; set; }

        public string AssetPath { get; set; } = string.Empty;

        private bool _isPlaying;

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying == value)
                {
                    return;
                }

                _isPlaying = value;

                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(IsPlaying)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IReadOnlyList<string> WwiseEvents { get; set; } =
            Array.Empty<string>();

        public IReadOnlyList<string> VisibleWwiseEvents =>
            WwiseEvents.Take(5).ToList();

        public IReadOnlyList<string> AdditionalWwiseEvents =>
            WwiseEvents.Skip(5).ToList();

        public string VisibleWwiseEventsText =>
            string.Join(
                Environment.NewLine,
                VisibleWwiseEvents);

        public string AdditionalWwiseEventsText =>
            string.Join(
                Environment.NewLine,
                AdditionalWwiseEvents);

        public bool HasAdditionalWwiseEvents =>
            WwiseEvents.Count > 5;

    }
}