using System;
using System.Collections.Generic;
using WemBam.Database;
using WemBam.Models;

namespace WemBam.Services
{
    public class SearchEngine
    {
        public IReadOnlyList<SearchResult> Search(
            string query,
            long? collectionId = null)
        {
            ArgumentNullException.ThrowIfNull(query);

            return DatabaseManager.SearchAudioAssets(
                query,
                collectionId);
        }
    }
}