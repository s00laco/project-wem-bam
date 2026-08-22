using System;
using System.Collections.Generic;
using System.Linq;

namespace WemBam.Models
{
    public class SearchResult
    {
        public string FileName { get; set; } = string.Empty;

        public string WwisePath { get; set; } = string.Empty;

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