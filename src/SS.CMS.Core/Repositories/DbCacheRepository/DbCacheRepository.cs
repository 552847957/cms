using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SS.CMS.Data;
using SS.CMS.Models;
using SS.CMS.Repositories;
using SS.CMS.Services;

namespace SS.CMS.Core.Repositories
{
    public class DbCacheRepository : IDbCacheRepository
    {
        private readonly Repository<DbCacheInfo> _repository;
        public DbCacheRepository(ISettingsManager settingsManager)
        {
            _repository = new Repository<DbCacheInfo>(new Database(settingsManager.DatabaseType, settingsManager.DatabaseConnectionString));
        }

        public IDatabase Database => _repository.Database;
        public string TableName => _repository.TableName;
        public List<TableColumn> TableColumns => _repository.TableColumns;

        private static class Attr
        {
            public const string CacheKey = nameof(DbCacheInfo.CacheKey);
            public const string CacheValue = nameof(DbCacheInfo.CacheValue);
            public const string CreatedDate = nameof(DbCacheInfo.CreatedDate);
        }

        public async Task RemoveAndInsertAsync(string cacheKey, string cacheValue)
        {
            if (string.IsNullOrEmpty(cacheKey)) return;

            await DeleteExcess90DaysAsync();

            await _repository.DeleteAsync(Q
                .Where(Attr.CacheKey, cacheKey));

            _repository.Insert(new DbCacheInfo
            {
                CacheKey = cacheKey,
                CacheValue = cacheValue
            });
        }

        public async Task ClearAsync()
        {
            await _repository.DeleteAsync();
        }

        public bool IsExists(string cacheKey)
        {
            return _repository.Exists(Q.Where(Attr.CacheKey, cacheKey));
        }

        public string GetValue(string cacheKey)
        {
            return _repository.Get<string>(Q
                .Select(Attr.CacheValue)
                .Where(Attr.CacheKey, cacheKey));
        }

        public async Task<string> GetValueAndRemoveAsync(string cacheKey)
        {
            var retVal = _repository.Get<string>(Q
                .Select(Attr.CacheValue)
                .Where(Attr.CacheKey, cacheKey));

            await _repository.DeleteAsync(Q
                .Where(Attr.CacheKey, cacheKey));

            return retVal;
        }

        public int GetCount()
        {
            return _repository.Count();
        }

        public async Task DeleteExcess90DaysAsync()
        {
            await _repository.DeleteAsync(Q
                .Where(Attr.CreatedDate, "<", DateTime.Now.AddDays(-90)));
        }
    }
}