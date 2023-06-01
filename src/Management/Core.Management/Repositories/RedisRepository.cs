using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Redis.OM;
using StackExchange.Redis;

using Core.Management.Interfaces;

namespace Core.Management.Repositories {

    public class RedisRepository : IRedisRepository
    {
        private readonly IDatabase _database;
        public RedisRepository(IDatabase database)
        {
            _database = database;
        }

        public static string ComposeKey(string entityType, string entityId) => $"{entityType}:{entityId}".ToUpper();

        #region Hashes
        public async Task<bool> UpsertHashRecord<T>(string entityType, string entityId, Dictionary<string, T> dictionary)
        {
            if (dictionary is null || dictionary.Count < 1) return false;

            string key = ComposeKey(entityType, entityId);

            List<HashEntry> actionEntry = new List<HashEntry>();

            foreach (KeyValuePair<string, T> entry in dictionary)
            {
                RedisValue redisValue = RedisValue.Unbox(entry.Value);

                if (redisValue.IsNullOrEmpty && await _database.HashExistsAsync(key, entry.Key))
                {
                    await _database.HashDeleteAsync(key, new RedisValue[] { entry.Key }).ConfigureAwait(false);
                    continue;
                }

                actionEntry.Add(new HashEntry(entry.Key, redisValue));
            }

            await _database.HashSetAsync(key, actionEntry.ToArray()).ConfigureAwait(false);

            return true;

        }

        public async Task<bool> RemoveHashKeyOrFields(string entityType, string entityId, List<string> hashFields)
        {
            string key = ComposeKey(entityType, entityId);

            if (hashFields is null || hashFields.Count < 1)
                return await _database.KeyDeleteAsync(key);

            return await _database.HashDeleteAsync(key, hashFields.Select(x => new RedisValue(x)).ToArray()) > 0;
        }

        public string GetHashField(string entityType, string entityId, string field)
        {
            string key = ComposeKey(entityType, entityId);
            return _database.HashGet(key, field).ToString();
        }

        public HashEntry[] GetHashRecord(string entityType, string entityId)
        {
            string key = ComposeKey(entityType, entityId);
            return _database.HashGetAll(key);
        }

        #endregion

        #region Sets

        public async Task InsertSets<TValue>(Dictionary<string, TValue[]> sets)
        {
            foreach (KeyValuePair<string, TValue[]> set in sets)
            {
                await _database.KeyDeleteAsync(set.Key).ConfigureAwait(false);

                await _database.SetAddAsync(set.Key, set.Value.Select(x => RedisValue.Unbox(x)).ToArray()).ConfigureAwait(false);
            }
        }

        public async Task<string[]> GetSetMembers(string key)
        {
            RedisValue[] setMembers = await _database.SetMembersAsync(key).ConfigureAwait(false);

            return setMembers.Select(x => x.ToString()).ToArray();
        }

        public async Task<string[]> GetRandomSetMembers(string key, int count)
        {
            RedisValue[] setMembers = await _database.SetRandomMembersAsync(key, count).ConfigureAwait(false);

            return setMembers.Select(x => x.ToString()).ToArray();
        }

        #endregion

        #region Lists

        /// <summary>
        /// Insert elements into a list at the head(left).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>Total number of inserted elements</returns>
        public async Task<long> ListLeftPush(string key, string[] values)
        {
            await _database.KeyDeleteAsync(key).ConfigureAwait(false);

            //implement queue in redis - pushing left popping right
            long insertedRecords = await _database.ListLeftPushAsync(key, values.Select(x => (RedisValue)x).ToArray()).ConfigureAwait(false);

            return insertedRecords;
        }

        /// <summary>
        /// Insert elements into a list at the head(left) with a timespan time to live(expiry) on the key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>Total number of inserted elements</returns>
        public async Task<long> ListLeftPush(string key, string[] values, TimeSpan expiry)
        {
            await _database.KeyDeleteAsync(key).ConfigureAwait(false);

            //implement queue in redis - pushing left popping right
            long insertedRecords = await _database.ListLeftPushAsync(key, values.Select(x => (RedisValue)x).ToArray()).ConfigureAwait(false);

            await _database.KeyExpireAsync(key, expiry).ConfigureAwait(false);

            return insertedRecords;
        }

        /// <summary>
        /// Insert elements into a list at the tail(right).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>Total number of inserted elements</returns>
        public async Task<long> ListRightPush(string key, string[] values)
        {
            await _database.KeyDeleteAsync(key).ConfigureAwait(false);

            //implement queue in redis - pushing left popping right
            long insertedRecords = await _database.ListRightPushAsync(key, values.Select(x => (RedisValue)x).ToArray()).ConfigureAwait(false);

            return insertedRecords;
        }

        /// <summary>
        /// Insert elements into a list at the tail(right) with a timespan time to live(expiry) on the key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>Total number of inserted elements</returns>
        public async Task<long> ListRightPush(string key, string[] values, TimeSpan expiry)
        {
            await _database.KeyDeleteAsync(key).ConfigureAwait(false);

            //implement queue in redis - pushing left popping right
            long insertedRecords = await _database.ListRightPushAsync(key, values.Select(x => (RedisValue)x).ToArray()).ConfigureAwait(false);

            await _database.KeyExpireAsync(key, expiry).ConfigureAwait(false);

            return insertedRecords;
        }

        /// <summary>
        /// FIFO popping from tail(right) and pushing the element back at the head(left). Done to restore the list
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A single element popped from the tail of list</returns>
        public async Task<string> ListRightPopLeftPush(string key)
        {
            RedisValue redisValue = await _database.ListRightPopLeftPushAsync(key, key);
            return redisValue.ToString();
        }

        /// <summary>
        /// LIFO popping from head(left)
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A single(first) element popped from the head of list</returns>
        public async Task<string> ListLeftPop(string key)
        {
            RedisValue redisValue = await _database.ListLeftPopAsync(key);
            return redisValue.ToString();
        }

        /// <summary>
        /// FIFO popping from rail(right)
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A single(last) element popped from the tail of list</returns>
        public async Task<string> ListRightPop(string key)
        {
            RedisValue redisValue = await _database.ListRightPopAsync(key);
            return redisValue.ToString();
        }

        /// <summary>
        /// Gets size of a list
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Size of a list using specified key</returns>
        public async Task<int> ListLength(string key) => Convert.ToInt32(await _database.ListLengthAsync(key));

        /// <summary>
        /// Deletes redis keys from database or ignored if does not exist
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Number of deleted keys</returns>
        public async Task<long> DeleteKeys(string[] keys) => await _database.KeyDeleteAsync(keys.Select(k => (RedisKey)k).ToArray()).ConfigureAwait(false);

        /// <summary>
        /// Sets a TTL on a key. Associated key will delete automatically if exists
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns>true if set, false if not or key does not exist</returns>
        public async Task<bool> SetKeyExpiry(string key, TimeSpan expiry) => await _database.KeyExpireAsync(key, expiry).ConfigureAwait(false);

        #endregion

        #region Sorted Set

        public async Task<double> InsertSortedSet(string key, string member, int score)
        {
            return await _database.SortedSetIncrementAsync(key, member, score).ConfigureAwait(false);
        }

        public async Task<long?> SortedSetRankAsync(string key, string member, Order order = Order.Ascending)
        {
            return await _database.SortedSetRankAsync(key, member, order).ConfigureAwait(false);
        }

        public async Task<SortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(string key, Order order = Order.Ascending, long skip = 0, long take = 10)
        {
            return await _database.SortedSetRangeByScoreWithScoresAsync(key, order: Order.Descending, skip: skip, take: take).ConfigureAwait(false);
        }
        #endregion

    }

}