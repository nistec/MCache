using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Nistec.Caching
{
    public class GenericDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {

        #region ctor

        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        //     class that is empty, has the default initial capacity, and uses the default
        //     equality comparer for the key type.
        public GenericDictionary() { }
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        //     class that contains elements copied from the specified System.Collections.Generic.IDictionary<TKey,TValue>
        //     and uses the default equality comparer for the key type.
        //
        // Parameters:
        //   dictionary:
        //     The System.Collections.Generic.IDictionary<TKey,TValue> whose elements are
        //     copied to the new System.Collections.Generic.Dictionary<TKey,TValue>.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     dictionary is null.
        //
        //   System.ArgumentException:
        //     dictionary contains one or more duplicate keys.
        public GenericDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        //     class that is empty, has the default initial capacity, and uses the specified
        //     System.Collections.Generic.IEqualityComparer<T>.
        //
        // Parameters:
        //   comparer:
        //     The System.Collections.Generic.IEqualityComparer<T> implementation to use
        //     when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer<T>
        //     for the type of the key.
        public GenericDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        //     class that is empty, has the specified initial capacity, and uses the default
        //     equality comparer for the key type.
        //
        // Parameters:
        //   capacity:
        //     The initial number of elements that the System.Collections.Generic.Dictionary<TKey,TValue>
        //     can contain.
        //
        // Exceptions:
        //   System.ArgumentOutOfRangeException:
        //     capacity is less than 0.
        public GenericDictionary(int capacity) : base(capacity) { }
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        //     class that contains elements copied from the specified System.Collections.Generic.IDictionary<TKey,TValue>
        //     and uses the specified System.Collections.Generic.IEqualityComparer<T>.
        //
        // Parameters:
        //   dictionary:
        //     The System.Collections.Generic.IDictionary<TKey,TValue> whose elements are
        //     copied to the new System.Collections.Generic.Dictionary<TKey,TValue>.
        //
        //   comparer:
        //     The System.Collections.Generic.IEqualityComparer<T> implementation to use
        //     when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer<T>
        //     for the type of the key.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     dictionary is null.
        //
        //   System.ArgumentException:
        //     dictionary contains one or more duplicate keys.
        public GenericDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        //     class that is empty, has the specified initial capacity, and uses the specified
        //     System.Collections.Generic.IEqualityComparer<T>.
        //
        // Parameters:
        //   capacity:
        //     The initial number of elements that the System.Collections.Generic.Dictionary<TKey,TValue>
        //     can contain.
        //
        //   comparer:
        //     The System.Collections.Generic.IEqualityComparer<T> implementation to use
        //     when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer<T>
        //     for the type of the key.
        //
        // Exceptions:
        //   System.ArgumentOutOfRangeException:
        //     capacity is less than 0.
        public GenericDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary<TKey,TValue>
        //     class with serialized data.
        //
        // Parameters:
        //   info:
        //     A System.Runtime.Serialization.SerializationInfo object containing the information
        //     required to serialize the System.Collections.Generic.Dictionary<TKey,TValue>.
        //
        //   context:
        //     A System.Runtime.Serialization.StreamingContext structure containing the
        //     source and destination of the serialized stream associated with the System.Collections.Generic.Dictionary<TKey,TValue>.
        public GenericDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

    }
}
