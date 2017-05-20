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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;

namespace Nistec.Caching
{
    /// <summary>
    /// Represent an generic hash table for serialization action.
    /// </summary>
    public class GenericHashtable : Hashtable
    {
     

        /// <summary>
        ///     Initializes a new, empty instance of the System.Collections.Hashtable class
        ///     using the default initial capacity, load factor, hash code provider, and
        ///     comparer.
        /// </summary>
        public GenericHashtable() { }
        /// <summary>
        ///     Initializes a new instance of the System.Collections.Hashtable class by copying
        ///     the elements from the specified dictionary to the new System.Collections.Hashtable
        ///     object. The new System.Collections.Hashtable object has an initial capacity
        ///     equal to the number of elements copied, and uses the default load factor,
        ///     hash code provider, and comparer.
        ///
        /// Parameters:
        ///   d:
        ///     The System.Collections.IDictionary object to copy to a new System.Collections.Hashtable
        ///     object.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     d is null.
        /// </summary>
        /// <param name="d"></param>
        public GenericHashtable(IDictionary d) : base(d) { }
        /// <summary>
        ///     Initializes a new, empty instance of the System.Collections.Hashtable class
        ///     using the default initial capacity and load factor, and the specified System.Collections.IEqualityComparer
        ///     object.
        ///
        /// Parameters:
        ///   equalityComparer:
        ///     The System.Collections.IEqualityComparer object that defines the hash code
        ///     provider and the comparer to use with the System.Collections.Hashtable object.
        ///      -or- null to use the default hash code provider and the default comparer.
        ///     The default hash code provider is each key's implementation of System.Object.GetHashCode()
        ///     and the default comparer is each key's implementation of System.Object.Equals(System.Object).
        /// </summary>
        /// <param name="equalityComparer"></param>
        public GenericHashtable(IEqualityComparer equalityComparer) : base(equalityComparer) { }
        /// <summary>
        ///     Initializes a new, empty instance of the System.Collections.Hashtable class
        ///     using the specified initial capacity, and the default load factor, hash code
        ///     provider, and comparer.
        ///
        /// Parameters:
        ///   capacity:
        ///     The approximate number of elements that the System.Collections.Hashtable
        ///     object can initially contain.
        ///
        /// Exceptions:
        ///   System.ArgumentOutOfRangeException:
        ///     capacity is less than zero.
        /// </summary>
        /// <param name="capacity"></param>
        public GenericHashtable(int capacity) : base(capacity) { }
        /// <summary>
        ///     Initializes a new instance of the System.Collections.Hashtable class by copying
        ///     the elements from the specified dictionary to the new System.Collections.Hashtable
        ///     object. The new System.Collections.Hashtable object has an initial capacity
        ///     equal to the number of elements copied, and uses the specified load factor,
        ///     and the default hash code provider and comparer.
        ///
        /// Parameters:
        ///   d:
        ///     The System.Collections.IDictionary object to copy to a new System.Collections.Hashtable
        ///     object.
        ///
        ///   loadFactor:
        ///     A number in the range from 0.1 through 1.0 that is multiplied by the default
        ///     value which provides the best performance. The result is the maximum ratio
        ///     of elements to buckets.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     d is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     loadFactor is less than 0.1.  -or- loadFactor is greater than 1.0.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="loadFactor"></param>
        public GenericHashtable(IDictionary d, float loadFactor) : base(d, loadFactor) { }
        /// <summary>
        ///     Initializes a new instance of the System.Collections.Hashtable class by copying
        ///     the elements from the specified dictionary to a new System.Collections.Hashtable
        ///     object. The new System.Collections.Hashtable object has an initial capacity
        ///     equal to the number of elements copied, and uses the default load factor
        ///     and the specified System.Collections.IEqualityComparer object.
        ///
        /// Parameters:
        ///   d:
        ///     The System.Collections.IDictionary object to copy to a new System.Collections.Hashtable
        ///     object.
        ///
        ///   equalityComparer:
        ///     The System.Collections.IEqualityComparer object that defines the hash code
        ///     provider and the comparer to use with the System.Collections.Hashtable. 
        ///     -or- null to use the default hash code provider and the default comparer.
        ///     The default hash code provider is each key's implementation of System.Object.GetHashCode()
        ///     and the default comparer is each key's implementation of System.Object.Equals(System.Object).
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     d is null.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="equalityComparer"></param>
        public GenericHashtable(IDictionary d, IEqualityComparer equalityComparer) : base(d, equalityComparer) { }
        /// <summary>
        ///     Initializes a new, empty instance of the System.Collections.Hashtable class
        ///     using the specified initial capacity and load factor, and the default hash
        ///     code provider and comparer.
        ///
        /// Parameters:
        ///   capacity:
        ///     The approximate number of elements that the System.Collections.Hashtable
        ///     object can initially contain.
        ///
        ///   loadFactor:
        ///     A number in the range from 0.1 through 1.0 that is multiplied by the default
        ///     value which provides the best performance. The result is the maximum ratio
        ///     of elements to buckets.
        ///
        /// Exceptions:
        ///   System.ArgumentOutOfRangeException:
        ///     capacity is less than zero.  -or- loadFactor is less than 0.1.  -or- loadFactor
        ///     is greater than 1.0.
        ///
        ///   System.ArgumentException:
        ///     capacity is causing an overflow.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="loadFactor"></param>
        public GenericHashtable(int capacity, float loadFactor) : base(capacity, loadFactor) { }
        /// <summary>
        ///     Initializes a new, empty instance of the System.Collections.Hashtable class
        ///     using the specified initial capacity and System.Collections.IEqualityComparer,
        ///     and the default load factor.
        ///
        /// Parameters:
        ///   capacity:
        ///     The approximate number of elements that the System.Collections.Hashtable
        ///     object can initially contain.
        ///
        ///   equalityComparer:
        ///     The System.Collections.IEqualityComparer object that defines the hash code
        ///     provider and the comparer to use with the System.Collections.Hashtable. 
        ///     -or- null to use the default hash code provider and the default comparer.
        ///     The default hash code provider is each key's implementation of System.Object.GetHashCode()
        ///     and the default comparer is each key's implementation of System.Object.Equals(System.Object).
        ///
        /// Exceptions:
        ///   System.ArgumentOutOfRangeException:
        ///     capacity is less than zero.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="equalityComparer"></param>
        public GenericHashtable(int capacity, IEqualityComparer equalityComparer) : base(capacity, equalityComparer) { }
        /// <summary>
        ///     Initializes a new, empty instance of the System.Collections.Hashtable class
        ///     that is serializable using the specified System.Runtime.Serialization.SerializationInfo
        ///     and System.Runtime.Serialization.StreamingContext objects.
        ///
        /// Parameters:
        ///   info:
        ///     A System.Runtime.Serialization.SerializationInfo object containing the information
        ///     required to serialize the System.Collections.Hashtable object.
        ///
        ///   context:
        ///     A System.Runtime.Serialization.StreamingContext object containing the source
        ///     and destination of the serialized stream associated with the System.Collections.Hashtable.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     info is null.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public GenericHashtable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            //Retrieve you data.
            //format = info.GetString(“format”);
            //args = (string[])info.GetValue(“args”,tyoeof(string[]));
        }
       /// <summary>
        ///     Initializes a new instance of the System.Collections.Hashtable class by copying
        ///     the elements from the specified dictionary to the new System.Collections.Hashtable
        ///     object. The new System.Collections.Hashtable object has an initial capacity
        ///     equal to the number of elements copied, and uses the specified load factor
        ///     and System.Collections.IEqualityComparer object.
        ///
        /// Parameters:
        ///   d:
        ///     The System.Collections.IDictionary object to copy to a new System.Collections.Hashtable
        ///     object.
        ///
        ///   loadFactor:
        ///     A number in the range from 0.1 through 1.0 that is multiplied by the default
        ///     value which provides the best performance. The result is the maximum ratio
        ///     of elements to buckets.
        ///
        ///   equalityComparer:
        ///     The System.Collections.IEqualityComparer object that defines the hash code
        ///     provider and the comparer to use with the System.Collections.Hashtable. 
        ///     -or- null to use the default hash code provider and the default comparer.
        ///     The default hash code provider is each key's implementation of System.Object.GetHashCode()
        ///     and the default comparer is each key's implementation of System.Object.Equals(System.Object).
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     d is null.
        ///
        ///   System.ArgumentOutOfRangeException:
        ///     loadFactor is less than 0.1.  -or- loadFactor is greater than 1.0.
       /// </summary>
       /// <param name="d"></param>
       /// <param name="loadFactor"></param>
       /// <param name="equalityComparer"></param>
        public GenericHashtable(IDictionary d, float loadFactor, IEqualityComparer equalityComparer) : base(d, loadFactor, equalityComparer) { }
        /// <summary>
        ///     Initializes a new, empty instance of the System.Collections.Hashtable class
        ///     using the specified initial capacity, load factor, and System.Collections.IEqualityComparer
        ///     object.
        ///
        /// Parameters:
        ///   capacity:
        ///     The approximate number of elements that the System.Collections.Hashtable
        ///     object can initially contain.
        ///
        ///   loadFactor:
        ///     A number in the range from 0.1 through 1.0 that is multiplied by the default
        ///     value which provides the best performance. The result is the maximum ratio
        ///     of elements to buckets.
        ///
        ///   equalityComparer:
        ///     The System.Collections.IEqualityComparer object that defines the hash code
        ///     provider and the comparer to use with the System.Collections.Hashtable. 
        ///     -or- null to use the default hash code provider and the default comparer.
        ///     The default hash code provider is each key's implementation of System.Object.GetHashCode()
        ///     and the default comparer is each key's implementation of System.Object.Equals(System.Object).
        ///
        /// Exceptions:
        ///   System.ArgumentOutOfRangeException:
        ///     capacity is less than zero.  -or- loadFactor is less than 0.1.  -or- loadFactor
        ///     is greater than 1.0.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="loadFactor"></param>
        /// <param name="equalityComparer"></param>
        public GenericHashtable(int capacity, float loadFactor, IEqualityComparer equalityComparer) : base(capacity, loadFactor, equalityComparer) { }
        
    }
}
