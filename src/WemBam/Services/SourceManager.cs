using System.Collections.ObjectModel;
using System.Linq;
using WemBam.Models;

namespace WemBam.Services
{
    public class SourceManager
    {
        private static readonly SourceManager _instance = new();

        public static SourceManager Instance => _instance;

        public ObservableCollection<Source> Sources { get; } = new();

        private SourceManager()
        {
        }

        public bool AddSource(Source source)
        {
            if (Sources.Any(existing =>
                string.Equals(
                    existing.Path,
                    source.Path,
                    System.StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            Sources.Add(source);
            return true;
        }

        public bool RemoveSource(Source source)
        {
            return Sources.Remove(source);
        }
    }
}