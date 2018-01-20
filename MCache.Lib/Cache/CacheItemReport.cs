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
using Nistec.Caching.Data;
using Nistec.Caching.Sync;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Caching
{
    /// <summary>
    /// Represent a cacheitem report.
    /// </summary>
    public class CacheItemReport : ISerialEntity
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CacheItemReport()
        {

        }
        /// <summary>
        /// Intilaize a new instance of <see cref="CacheItemReport"/> for <see cref="ISyncTableStream"/> item.
        /// </summary>
        /// <param name="item"></param>
        public CacheItemReport(ISyncTableStream item)
        {
            if (item == null)
                return;
            Name = item.EntityName;//.Info.Prefix;
            Count = item.Count;
            Size = item.Size;
            Modified = item.Modified;
            Data = item.GetItemsReport();
        }

        /// <summary>
        /// Intilaize a new instance of <see cref="CacheItemReport"/> for <see cref="DbTable"/> item.
        /// </summary>
        /// <param name="item"></param>
        public CacheItemReport(DbTable item)
        {
            if (item == null)
                return;
            Name = item.Name;
            Count = item.Count;
            Size = item.Size;
            Modified = item.Modified;
            Data = item.GetItemsReport();
        }

        /// <summary>
        /// Get the entity item name.
        /// </summary>
        public string Name{get;internal set;}
        /// <summary>
        /// Get items count in data report.
        /// </summary>
        public int Count { get; internal set; }
        /// <summary>
        /// Get the size of entity item in Kb.
        /// </summary>
        public long Size { get; internal set; }
        /// <summary>
        /// Get the Modified time of entity item.
        /// </summary>
        public DateTime Modified { get; internal set; }
        /// <summary>
        /// Get the data report of current entity item.
        /// </summary>
        public DataTable Data { get; internal set; }
        /// <summary>
        /// Get the title of current entity item.
        /// </summary>
        public string Caption
        {
            get { return string.Format("Name: {0}, Count: {1}, Size: {2} Kb, Modified: {3}", Name, Count, Size/1024, Modified); }
        }

        #region  IEntityFormatter
       
        /// <summary>
        /// Write entity properties to stream using <see cref="IBinaryStreamer"/> streamer.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            streamer.WriteString(Name);
            streamer.WriteValue(Count);
            streamer.WriteValue(Size);
            streamer.WriteValue(Modified);
            streamer.WriteValue(Data);
            streamer.Flush();
        }
       
        /// <summary>
        /// Read entity properties from stream using <see cref="IBinaryStreamer"/> streamer.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            Name = streamer.ReadString();
            Count = streamer.ReadValue<int>();
            Size = streamer.ReadValue<long>();
            Modified = streamer.ReadValue<DateTime>();
            Data = (DataTable)streamer.ReadValue();
        }
        #endregion

    }
}
