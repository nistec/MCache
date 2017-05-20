//licHeader
//===============================================================================================================
// System  : Nistec.Cache - Nistec.Cache Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of cache core.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nistec.Caching
{

    /// <summary>
    /// Provides a collection of unordered unique keys.
    /// </summary>
    /// <typeparam name="T">The type of keys to collection</typeparam>
    /// <remarks>
    /// KeySet is similar to a Dictionary but does not contain a value and is useful when one only cares about existence of
    /// keys and not values associated with them.
    /// </remarks>
    [DebuggerStepThrough]
    public class KeySet<T> : IDictionary<T, bool>, ICollection<T>, IEnumerable<T>, ICollection
    {
        #region Data

        private Dictionary<T, bool> _dictionary;

        #endregion

        #region Constructors
        /// <summary>
        /// ctor
        /// </summary>
        public KeySet()
            : this(0)
        {
        }
        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="comparer"></param>
        public KeySet(IEqualityComparer<T> comparer)
            : this(0, comparer)
        {
        }
        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="capactity"></param>
        public KeySet(int capactity)
            : this(capactity, null)
        {
        }
        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="capactity"></param>
        /// <param name="comparer"></param>
        public KeySet(int capactity, IEqualityComparer<T> comparer)
        {
            _dictionary = new Dictionary<T, bool>(capactity, comparer);
        }
        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="collection"></param>
        public KeySet(ICollection<T> collection)
            : this(collection, null)
        {
        }
        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="comparer"></param>
        public KeySet(ICollection<T> collection, IEqualityComparer<T>
        comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            _dictionary = new Dictionary<T, bool>(collection.Count,
            comparer);
            foreach (T key in collection)
            {
                _dictionary[key] = true;
            }
        }

        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the number of key/value pairs contained in the KeySet.
        /// </summary>
        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }
        /// <summary>
        /// Gets the number of key/value pairs contained in theKeySet.
        /// </summary>
        public ICollection<T> Keys
        {
            get
            {
                return _dictionary.Keys;
            }
        }
        /// <summary>
        /// Get indicate whether the KeySet is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the key exists in the collection, false otherwise.
        /// </summary>
        /// Key to check for.
        /// <returns>True if key exists in collection.</returns>
        /// <remarks>
        /// Setting a value to false will remove the key from the collection if it exists.
        ///
        /// Unlike IDictionary the indexer will not throw an
        ///exception on an non-existent key, it will simply return false.
        /// </remarks>
        public bool this[T key]
        {
            get
            {
                return _dictionary.ContainsKey(key);
            }
            set
            {
                if (value)
                {
                    _dictionary[key] = true;
                }
                else
                {
                    _dictionary.Remove(key);
                }
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Add key.
        /// </summary>
        /// <param name="key"></param>
        public void Add(T key)
        {
            _dictionary.Add(key, true);
        }
        /// <summary>
        /// Add collection of keys.
        /// </summary>
        /// <param name="collection"></param>
        public void Add(ICollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            foreach (T key in collection)
            {
                _dictionary.Add(key, true);
            }
        }

        ///<summary>
        ///Removes all items from the <see
        ///cref="System.Collections.Generic.ICollection{T}"></see>.
        ///</summary>
        public void Clear()
        {
            _dictionary.Clear();
        }
        /// <summary>
        /// Determines whether the KeySet contains the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(T key)
        {
            return _dictionary.ContainsKey(key);
        }
        /// <summary>
        ///  Copies the KeySet.KeyCollection
        ///     elements to an existing one-dimensional System.Array, starting at the specified
        ///     array index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _dictionary.Keys.CopyTo(array, arrayIndex);
        }
        /// <summary>
        ///  Returns an enumerator that iterates through the KeySet.KeyCollection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _dictionary.Keys.GetEnumerator();
        }
        /// <summary>
        /// Removes the value with the specified key from the KeySet.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(T key)
        {
            return _dictionary.Remove(key);
        }
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(T key, out bool value)
        {
            //if (key == null)
            //{
            //    value = false;
            //    return false;
            //}
            return _dictionary.TryGetValue(key, out value);
        }

        #endregion

        #region IDictionary<T,bool> Members

        //<summary>
        //Adds an element with the provided key and value to the <see
        //cref="System.Collections.Generic.IDictionary{T,V}"></see>.
        //</summary>
        
        //The bool to use as the value of the element to add.
        //The bool to use as the key of the element to add.
        //<exception cref="System.NotSupportedException">The <see
        //cref="System.Collections.Generic.IDictionary{T,V}"></see> is
        //read-only.</exception>
        //<exception cref="System.ArgumentException">An element with
        //the same key already exists in the <see
        //cref="System.Collections.Generic.IDictionary{T,V}"></see>.</exception>
        //<exception cref="System.ArgumentNullException">key is
        //null.</exception>
        void IDictionary<T, bool>.Add(T key, bool value)
        {
            if (!value)
            {
                throw TrueOnlyException;
            }
            _dictionary.Add(key, true);
        }

        //<summary>
        //Determines whether the <see
        //cref="System.Collections.Generic.IDictionary{T,V}"></see> contains an
        //element with the specified key.
        //</summary>
        
        //<returns>
        //true if the <see
        //cref="System.Collections.Generic.IDictionary{T,V}"></see> contains an
        //element with the key; otherwise, false.
        //</returns>
        
        //The key to locate in the <see
        //cref="System.Collections.Generic.IDictionary{T,V}"></see>.
        //<exception cref="System.ArgumentNullException">key is null.</exception>
        bool IDictionary<T, bool>.ContainsKey(T key)
        {
            return _dictionary.ContainsKey(key);
        }

        //<summary>
        //Gets an <see
        //cref="System.Collections.Generic.ICollection{T}"></see> containing
        //the values in the <see
        //cref="System.Collections.Generic.IDictionary{T,V}"></see>.
        //</summary>
        
        //<returns>
        //An <see
        //cref="System.Collections.Generic.ICollection{T}"></see> containing
        //the values in the bool that implements <see
        //cref="System.Collections.Generic.IDictionary{T,V}"></see>.
        //</returns>
        
        ICollection<bool> IDictionary<T, bool>.Values
        {
            get
            {
                return _dictionary.Values;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<T,bool>> Members

        //<summary>
        //Removes the first occurrence of a specific bool from the <see
        //cref="System.Collections.Generic.ICollection{T}"></see>.
        //</summary>
        
        //<returns>
        //true if item was successfully removed from the <see
        //cref="System.Collections.Generic.ICollection{T}"></see>; otherwise,
        //false. This method also returns false if item is not found in the
        //original <see
        //cref="System.Collections.Generic.ICollection{T}"></see>.
        //</returns>
        
        //The bool to remove from the <see
        //cref="System.Collections.Generic.ICollection{T}"></see>.
        //<exception cref="System.NotSupportedException">The <see
        //cref="System.Collections.Generic.ICollection{T}"></see> is
        //read-only.</exception>
        bool ICollection<KeyValuePair<T, bool>>.Remove(KeyValuePair<T, bool> item)
        {
            return _dictionary.Remove(item.Key);
        }

        //<summary>
        //Copies the elements of the <see
        //cref="System.Collections.Generic.ICollection{T}"></see> to an <see
        //cref="System.Array"></see>, starting at a particular <see
        //cref="System.Array"></see> index.
        //</summary>
        
        //The one-dimensional <see
        //cref="System.Array"></see> that is the destination of the elements
        //copied from <see
        //cref="System.Collections.Generic.ICollection{T}"></see>. The <see
        //cref="System.Array"></see> must have zero-based indexing.
        //The zero-based index in array at which
        //copying begins.
        //<exception
        //cref="System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        //<exception cref="System.ArgumentNullException">array is null.</exception>
        //<exception cref="System.ArgumentException">array is
        //multidimensional.-or-arrayIndex is equal to or greater than the length
        //of array.-or-The number of elements in the source <see
        //cref="System.Collections.Generic.ICollection{T}"></see> is greater
        //than the available space from arrayIndex to the end of the destination
        //array.-or-Type T cannot be cast automatically to the type of the
        //destination array.</exception>
        void ICollection<KeyValuePair<T, bool>>.CopyTo(KeyValuePair<T, bool>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<T, bool>>)_dictionary).CopyTo(array,
            arrayIndex);
        }

        //<summary>
        //Determines whether the <see
        //cref="System.Collections.Generic.ICollection{T}"></see> contains a
        //specific value.
        //</summary>
        
        //<returns>
        //true if item is found in the <see
        //cref="System.Collections.Generic.ICollection{T}"></see>; otherwise,false.
        //</returns>
        
        //The bool to locate in the <see
        //cref="System.Collections.Generic.ICollection{T}"></see>.
        bool ICollection<KeyValuePair<T, bool>>.Contains(KeyValuePair<T, bool> item)
        {
            return _dictionary.ContainsKey(item.Key);
        }

        //<summary>
        //Adds an item to the <see cref="System.Collections.Generic.ICollection{T}"></see>.
        //</summary>
        
        //The bool to add to the <see cref="System.Collections.Generic.ICollection{T}"></see>.
        //<exception cref="System.NotSupportedException">The <see cref="System.Collections.Generic.ICollection{T}"></see> is read-only.</exception>
        void ICollection<KeyValuePair<T, bool>>.Add(KeyValuePair<T, bool> item)
        {
            if (item.Value)
            {
                throw TrueOnlyException;
            }
            _dictionary.Add(item.Key, true);
        }

        #endregion

        #region ICollection Members

        //<summary>
        //Copies the elements of the <see cref="System.Collections.ICollection"></see> 
        //to an <see cref="System.Array"></see>, starting at a particular <see cref="System.Array"></see> index.
        //</summary>
        
        //The one-dimensional <see cref="System.Array"></see> that is the destination of the elements copied from <see cref="System.Collections.ICollection"></see>. The
        //The zero-based index in array at which copying begins. 
        //<exception cref="System.ArgumentNullException">array is null.</exception>
        //<exception cref="System.ArgumentOutOfRangeException">index is less than zero. </exception>
        //<exception cref="System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array.</exception>
        //<exception cref="System.InvalidCastException">The type of the source <see cref="System.Collections.ICollection"/> cannot be cast automatically to the type of the destination array.</exception>
        void ICollection.CopyTo(Array array, int index)
        {
            ((IDictionary)_dictionary).Keys.CopyTo(array, index);
        }

        //<summary>
        //Gets an object that can be used to synchronize access to the
        //</summary>
        
        //<returns>
        //An object that can be used to synchronize access to the <see
        //cref="System.Collections.ICollection"></see>.
        //</returns>
        //<filterpriority>2</filterpriority>
        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)_dictionary).SyncRoot;
            }
        }

        //<summary>
        //Gets a value indicating whether access to the <see
        //cref="System.Collections.ICollection"></see> is synchronized (thread safe).
        //</summary>
        
        //<returns>
        //true if access to the <see  cref="System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.
        //</returns>
        //<filterpriority>2</filterpriority>
        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)_dictionary).IsSynchronized;
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<T,bool>> Members

        //<summary>
        //Returns an enumerator that iterates through the collection.
        //</summary>
        
        //<returns>
        //A <see cref="System.Collections.Generic.IEnumerator{T}"></see>
        //that can be used to iterate through the collection.
        //</returns>
        //<filterpriority>1</filterpriority>
        IEnumerator<KeyValuePair<T, bool>> IEnumerable<KeyValuePair<T, bool>>.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<T, bool>>)
            _dictionary).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        //<summary>
        //Returns an enumerator that iterates through a collection.
        //</summary>
        
        //<returns>
        //An <see cref="System.Collections.IEnumerator"></see> bool
        //that can be used to iterate through the collection.
        //</returns>
        //<filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.Keys.GetEnumerator();
        }

        #endregion

        #region Private Helpers

        private static ArgumentException TrueOnlyException
        {
            get
            {
                return new ArgumentException("KeySet only supports 'true'values.");
            }
        }

        #endregion

    }
}