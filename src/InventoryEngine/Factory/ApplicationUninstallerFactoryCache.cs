using System.Collections.Generic;
using System.IO;

namespace InventoryEngine.Factory
{
    internal class ApplicationUninstallerFactoryCache
    {
        internal string Filename { get; set; }

        private Dictionary<string, ApplicationUninstallerEntry> Cache { get; }

        internal ApplicationUninstallerFactoryCache(string filename) : this(filename, new Dictionary<string, ApplicationUninstallerEntry>())
        {
        }

        private ApplicationUninstallerFactoryCache(string filename, Dictionary<string, ApplicationUninstallerEntry> cacheData)
        {
            Filename = filename;
            Cache = cacheData;
        }

        internal void Delete()
        {
            File.Delete(Filename);
            Cache.Clear();
        }

        internal void Read()
        {
            //var result = SerializationTools.DeserializeFromXml<List<CacheEntry>>(Filename);

            //PersistentCache.Clear();

            //// Ignore entries if more than 1 have the same cache id
            //foreach (var group in result
            //    .GroupBy(x => x.Entry.GetCacheId())
            //    .Where(g => g.Key != null && g.CountEquals(1)))
            //{
            //    var cacheEntry = group.Single();

            // if (SerializeIcons && cacheEntry.Icon != null) cacheEntry.Entry.IconBitmap = DeserializeIcon(cacheEntry.Icon);

            //    PersistentCache.Add(group.Key, cacheEntry.Entry);
            //}
        }

        internal void Save()
        {
            //SerializationTools.SerializeToXml(Filename, PersistentCache.Select(x => new CacheEntry(
            //    x.Value,
            //    SerializeIcons && x.Value.IconBitmap != null ? SerializeIcon(x.Value.IconBitmap) : null))
            //    .ToList());
        }

        internal void TryCacheItem(ApplicationUninstallerEntry item)
        {
            var id = item?.GetCacheId();
            if (!string.IsNullOrEmpty(id))
            {
                Cache[id] = item;
            }
        }

        internal ApplicationUninstallerEntry TryGetCachedItem(ApplicationUninstallerEntry notCachedEntry)
        {
            var id = notCachedEntry?.GetCacheId();

            if (!string.IsNullOrEmpty(id) && Cache.TryGetValue(id, out var matchedEntry))
            {
                return matchedEntry;
            }

            return null;
        }
    }
}