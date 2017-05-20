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

namespace Nistec.Caching
{
    /// <summary>
    /// Represent entity known types.
    /// </summary>
    public class KnownEntityTypes
    {
        /// <summary>Represent <see cref="GenericEntity"/> type.</summary>
        public const string GenericEntity = "GenericEntity";
        /// <summary>Represent EntityContext type.</summary>
        public const string EntityContext = "EntityContext";
        /// <summary>Represent <see cref="System.Collections.IDictionary"/> type also known as GenericRecord.</summary>
        public const string IDictionary = "IDictionary";
        /// <summary>Represent <see cref="Nistec.IO.NetStream"/> type also known as NetStream.</summary>
        public const string BodyStream = "BodyStream";
        /// <summary>Represent any entity type, mean unknown type.</summary>
        public const string AnyType = "AnyType";
        
    }

    /// <summary>
    /// Represent cache entity known types.
    /// </summary>
    public enum CacheEntityTypes
    {
        /// <summary>Represent <see cref="GenericEntity"/> type.</summary>
        GenericEntity,
        /// <summary>Represent <see cref="EntityContext"/> type.</summary>
        EntityContext,
        /// <summary>Represent <see cref="System.Collections.IDictionary"/> type also known as GenericRecord.</summary>
        IDictionary,
        /// <summary>Represent <see cref="Nistec.IO.NetStream"/> type also known as NetStream.</summary>
        BodyStream,
        /// <summary>Represent any entity type, mean unknown type.</summary>
        AnyType
    }

}
