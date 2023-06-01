using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using StackExchange.Redis;

namespace Core.Management.Interfaces
{

    public interface IRedisRepository
    {
        #region Hashes    
        Task<bool> UpsertHashRecord<T>(string entityType, string entityId, Dictionary<string, T> dictionary);
        string GetHashField(string entityType, string entityId, string field);
        HashEntry[] GetHashRecord(string entityType, string entityId);
        Task<bool> RemoveHashKeyOrFields(string entityType, string entityId, List<string> hashFields);

        #endregion

        #region Sets
        Task InsertSets<TValue>(Dictionary<string, TValue[]> sets);

        Task<string[]> GetSetMembers(string key);

        Task<string[]> GetRandomSetMembers(string key, int count);

        #endregion

        #region Lists

        /// <summary>
        /// Insert elements into a list from the head(left).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>Total number of inserted elements</returns>
        Task<long> ListLeftPush(string key, string[] values);

        /// <summary>
        /// Insert elements into a list from the head(left) with a timespan time to live(expiry) on the key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>Total number of inserted elements</returns>
        Task<long> ListLeftPush(string key, string[] values, TimeSpan expiry);

        /// <summary>
        /// Insert elements into a list at the tail(right).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>Total number of inserted elements</returns>
        Task<long> ListRightPush(string key, string[] values);

        /// <summary>
        /// Insert elements into a list at the tail(right) with a timespan time to live(expiry) on the key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>Total number of inserted elements</returns>
        Task<long> ListRightPush(string key, string[] values, TimeSpan expiry);

        /// <summary>
        /// FIFO popping from tail(right) and pushing the element back at the head(left). Done to restore the list
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A single element popped from the tail of list</returns>
        Task<string> ListRightPopLeftPush(string key);

        /// <summary>
        /// LIFO popping from head(left)
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A single element popped from the head of list</returns>
        Task<string> ListLeftPop(string key);

        /// <summary>
        /// Gets size of a list
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Size of a list using specified key</returns>
        Task<int> ListLength(string key);

        /// <summary>
        /// Deletes redis keys from database or ignored if does not exist
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Number of deleted keys</returns>
        Task<long> DeleteKeys(string[] keys);

        /// <summary>
        /// Sets a TTL on a key. Associated key will delete automatically if exists
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns>true if set, false if not or key does not exist</returns>
        Task<bool> SetKeyExpiry(string key, TimeSpan expiry);

        #endregion

        #region Sorted Set
        Task<double> InsertSortedSet(string key, string member, int score);

        Task<long?> SortedSetRankAsync(string key, string member, Order order = Order.Ascending);

        Task<SortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(string key, Order order = Order.Ascending, long skip = 0, long take = 10);

        #endregion
    }
}