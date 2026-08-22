using System;
using System.Collections.Generic;
using WemBam.Database;
using WemBam.Models;

namespace WemBam.Services
{
    public class SearchEngine
    {
        public IReadOnlyList<SearchResult> Search(
            string query)
        {
            ArgumentNullException.ThrowIfNull(query);

            return DatabaseManager.SearchAudioAssets(query);
        }
    }
}