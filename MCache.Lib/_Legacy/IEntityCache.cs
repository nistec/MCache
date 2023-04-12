using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using Nistec.Generic;

namespace Nistec.Legacy
{
    public interface IEntityCache
    {
        /// <summary>
        /// Get Keys items
        /// </summary>
        List<string> DataKeys { get; }
        /// <summary>
        /// Reset cache
        /// </summary>
        void Reset();

        /// <summary>
        /// Refresh cache
        /// </summary>
        void Refresh();
    }

    public interface IEntityCache<T> : IEntityCache
    {
        /// <summary>
        /// Get Item by key with number of options
        /// </summary>
        /// <param name="options"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        T GetItem(params string[] key);
        /// <summary>
        /// Get Item by key with number of options
        /// </summary>
        /// <param name="options"></param>
        /// <param name="key"></param>
        /// <returns>return null or empty if not exists</returns>
        T FindItem(params string[] key);
    }

}
