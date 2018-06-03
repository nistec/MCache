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
using System.Text;
using System.Data;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nistec.Serialization;
using Nistec.Generic;
using System.IO;
using Nistec.Caching.Config;
using Nistec.Caching.Server;
using Nistec.Runtime;
using System.Diagnostics;

namespace Nistec.Caching
{
        

    /// <summary>
    /// Represent a thread safe cache item performance counter.
    /// </summary>
    [Serializable]
    public class CachePerformanceCounter
    {

        ICachePerformance Owner;

        /// <summary>
        /// Initialize a new instance of performance counter.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="agentType"></param>
        /// <param name="name"></param>
        internal CachePerformanceCounter(ICachePerformance agent, CacheAgentType agentType, string name)
        {
            Owner = agent;
            MaxSize = Owner.GetMaxSize();
            AgentType = agentType;
            CounterName = name;
            StartTime = DateTime.Now;
            LastRequestTime = DateTime.Now;
            LastResponseTime = DateTime.Now;
            LastResetTime = DateTime.Now;
            AutoResetIntervalHours = CacheSettings.AutoResetIntervalHours;
            InitStateCounter();
        }

        #region members

        /// <summary>
        /// Get Cache Agent Type.
        /// </summary>
        public CacheAgentType AgentType { get; internal set; }
        /// <summary>
        /// Get Counter Name.
        /// </summary>
        public string CounterName { get; internal set; }

        long _ItemsCount;
        /// <summary>
        /// Get Items count as an atomic operation.
        /// </summary>
        public long ItemsCount { get { return Interlocked.Read(ref _ItemsCount); } }

        long _RequestCount;
        /// <summary>
        /// Get Request count as an atomic operation.
        /// </summary>
        public long RequestCount { get { return Interlocked.Read(ref _RequestCount); } }

        long _ResponseCountPerHour;
        /// <summary>
        /// Get Response count per hour as an atomic operation.
        /// </summary>
        public long ResponseCountPerHour { get { return Interlocked.Read(ref _ResponseCountPerHour); } }

        long _ResponseCountPerDay;
        /// <summary>
        /// Get Response count per day as an atomic operation.
        /// </summary>
        public long ResponseCountPerDay { get { return Interlocked.Read(ref _ResponseCountPerDay); } }

        long _ResponseCountPerMonth;
        /// <summary>
        /// Get Response count per month as an atomic operation.
        /// </summary>
        public long ResponseCountPerMonth { get { return Interlocked.Read(ref _ResponseCountPerMonth); } }

        long _SyncCount;
        /// <summary>
        /// Get Sync count as an atomic operation.
        /// </summary>
        public long SyncCount { get { return Interlocked.Read(ref _SyncCount); } }

        
        /// <summary>
        /// Get waiting task count as an atomic operation.
        /// </summary>
        public int WaitingTaskCount() 
        {
            return AgentManager.PerformanceTasker.Count;
        }


        /// <summary>
        /// Get Start Time.
        /// </summary>
        public DateTime StartTime { get; internal set; }
        /// <summary>
        /// Get the interval in hours for auto reset performance counter.
        /// </summary>
        public int AutoResetIntervalHours { get; internal set; }
        /// <summary>
        /// Get the last time of reset performance counter.
        /// </summary>
        public DateTime LastResetTime { get; internal set; }
        /// <summary>
        /// Get the last time of request action.
        /// </summary>
        public DateTime LastRequestTime { get; internal set; }
        /// <summary>
        /// Get the last time of response action.
        /// </summary>
        public DateTime LastResponseTime { get; internal set; }
        /// <summary>
        /// Get Last Sync Time.
        /// </summary>
        public DateTime LastSyncTime { get; internal set; }
        /// <summary>
        /// Get Max Hit Per Minute.
        /// </summary>
        public int MaxHitPerMinute { get; internal set; }
        /// <summary>
        /// Get the avarage hit Per Minute.
        /// </summary>
        public int AvgHitPerMinute { get; internal set; }

        float _AvgResponseTime;
        /// <summary>
        /// Get avarage response time.
        /// </summary>
        public float AvgResponseTime
        {
            get { return _AvgResponseTime; }
        }

        /// <summary>
        /// Get avarage sync time.
        /// </summary>
        public float AvgSyncTime
        {
            get { return (float)(SyncCount == 0 ? 0 : (float)(m_SyncTimeSum / SyncCount)); }
        }

        #endregion

        DateTime m_LastResponseSycle;
        DateTime m_LastMinuteSycle;
        DateTime m_LastHourSycle;
        DateTime m_LastDaySycle;
        DateTime m_LastMonthSycle;
        double m_ResponseTimeSum;
        int m_HitMinuteCounter;
        double m_SyncTimeSum;
        ConcurrentDictionary<CacheState, int> m_StateCounter;

        internal void InitCounter()
        {
            InitStateCounter();
            _ItemsCount = 0;
            _RequestCount = 0;
            _ResponseCountPerHour = 0;
            _ResponseCountPerDay = 0;
            _ResponseCountPerMonth = 0;
            MaxSize = 0;
            _MemoSize = 0;
            _SyncCount = 0;
            _AvgResponseTime = 0;
            m_SyncTimeSum = 0;
            AvgHitPerMinute = 0;
            MaxHitPerMinute = 0;
            LastResetTime = DateTime.Now;
            StartTime = DateTime.Now.AddHours(-1);
            LastRequestTime = DateTime.Now.AddHours(-1);
            LastResponseTime = DateTime.Now.AddHours(-1);
            LastSyncTime = DateTime.Now.AddHours(-1);
        }

        void InitStateCounter()
        {
            m_StateCounter = new ConcurrentDictionary<CacheState, int>();

            foreach (var state in Enum.GetValues(typeof(CacheState)))
            {
                m_StateCounter[(CacheState)state] = 0;
            }
        }
        internal void ClearStateCounter()
        {
            if (m_StateCounter != null)
            {
                m_StateCounter.Clear();
            }
        }

        internal void ResetCounter()
        {
            ClearStateCounter();
            InitCounter();
            RefreshSize();
            CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.Info, "CachePerformanceCounter was reset all counters.");
        }

        internal IDictionary<CacheState, int> StateCounter()
        {
            return m_StateCounter;
        }

        internal GenericNameValue GetStateCounter()
        {
            GenericNameValue gnv = new GenericNameValue();
            foreach (var entry in m_StateCounter)
            {
                gnv.Add(entry.Key.ToString(),entry.Value);
            }
            return gnv;
        }

        void AddStateCounter(CacheState state)
        {
            m_StateCounter[state] += 1;
        }

        internal void AddRequest()
        {
            Interlocked.Increment(ref _RequestCount);
            if (AutoResetIntervalHours > 0 && LastResetTime.Subtract(DateTime.Now).TotalHours > AutoResetIntervalHours)
            {
                InitCounter();
            }
            LastRequestTime = DateTime.Now;
        }

        internal void AddResponseAsync(DateTime requestTime, CacheState state, bool addRequest)
        {
            if (CacheSettings.EnableAsyncTask)
                AgentManager.PerformanceTasker.Add(new Nistec.Threading.TaskItem(() => AddResponse(requestTime, state, addRequest), CacheDefaults.DefaultTaskTimeout));
            else
                Task.Factory.StartNew(() => AddResponse(requestTime, state, addRequest));
        }

        void AddResponse(DateTime requestTime, CacheState state, bool addRequest)
        {
            DateTime responseTime = DateTime.Now;
            if (addRequest)
                AddRequest();
            AddResponse(responseTime.Subtract(requestTime).TotalSeconds, state);
        }

        void AddResponse(double responseTime, CacheState state)
        {
            try
            {
                DateTime now = DateTime.Now;
                long responseCountPerHour = ResponseCountPerHour;

                if (now.Month != m_LastMonthSycle.Month)
                {
                    m_LastMonthSycle = now;
                    Interlocked.Exchange(ref _ResponseCountPerMonth, 0);
                }

                if (now.Day != m_LastDaySycle.Day)
                {
                    m_LastDaySycle = now;
                    Interlocked.Exchange(ref _ResponseCountPerDay, 0);
                }

                if (now.Hour != m_LastHourSycle.Hour)
                {
                    Interlocked.Exchange(ref _ResponseCountPerHour, 0);
                    Interlocked.Exchange(ref m_ResponseTimeSum, 0);
                    m_LastHourSycle = now;
                }

                LastResponseTime = now;
                m_ResponseTimeSum += responseTime;
                if (responseCountPerHour > 0)
                {
                    _AvgResponseTime = (float)(m_ResponseTimeSum / responseCountPerHour);
                }
                Interlocked.Increment(ref _ResponseCountPerHour);
                Interlocked.Increment(ref _ResponseCountPerDay);
                Interlocked.Increment(ref _ResponseCountPerMonth);
                Interlocked.Increment(ref m_HitMinuteCounter);
                AddStateCounter(state);
                //int avg = AvgHitPerMinute;

                if (now.Subtract(m_LastResponseSycle).TotalSeconds > 1)
                {


                    int totalMinute = (int)(now - m_LastHourSycle).TotalMinutes;
                    if (totalMinute <= 0)
                        totalMinute = 1;

                    AvgHitPerMinute = (int)(ResponseCountPerHour / totalMinute);


                    MaxHitPerMinute = Math.Max(Math.Max(m_HitMinuteCounter, AvgHitPerMinute), MaxHitPerMinute);
                    if (now.Subtract(m_LastMinuteSycle).TotalMinutes > 1)
                    {
                        Interlocked.Exchange(ref m_HitMinuteCounter, 0);
                        m_LastMinuteSycle = now;
                    }


                    m_LastResponseSycle = DateTime.Now;
                }
            }
            catch(OverflowException oex)
            {
                ResetCounter();
                CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.Error, "CachePerformanceCounter.AddResponse OverflowException: " + oex.Message);
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.Error, "CachePerformanceCounter.AddResponse error: " + ex.Message);
            }
        }

        internal void AddSync(DateTime startTime)
        {
            LastSyncTime = DateTime.Now;
            Interlocked.Increment(ref _SyncCount);
            m_SyncTimeSum += LastSyncTime.Subtract(startTime).TotalMinutes;
        }


        /// <summary>
        /// Cache Performance Item Schema as <see cref="DataTable"/> class.
        /// </summary>
        /// <returns></returns>
        internal static DataTable CachePerformanceSchema()
        {

            DataTable dt = new DataTable("CachePerformance");
            dt.Columns.Add("AgentType", typeof(string));
            dt.Columns.Add("CounterName", typeof(string));
            dt.Columns.Add("ItemsCount", typeof(long));
            dt.Columns.Add("RequestCount", typeof(long));
            dt.Columns.Add("ResponseCountPerHour", typeof(long));
            dt.Columns.Add("ResponseCountPerDay", typeof(long));
            dt.Columns.Add("ResponseCountPerMonth", typeof(long));
            dt.Columns.Add("SyncCount", typeof(long));
            dt.Columns.Add("StartTime", typeof(DateTime));
            dt.Columns.Add("LastRequestTime", typeof(DateTime));
            dt.Columns.Add("LastResponseTime", typeof(DateTime));
            dt.Columns.Add("LastSyncTime", typeof(DateTime));
            dt.Columns.Add("MaxHitPerMinute", typeof(int));
            dt.Columns.Add("AvgHitPerMinute", typeof(int));
            dt.Columns.Add("AvgResponseTime", typeof(float));
            dt.Columns.Add("AvgSyncTime", typeof(float));
            dt.Columns.Add("MaxSize", typeof(long));
            dt.Columns.Add("MemorySize", typeof(long));
            dt.Columns.Add("FreeSize", typeof(long));
            //dt.Columns.Add("MemoryUsage", typeof(long));
            dt.Columns.Add("UnitSize", typeof(string));
            dt.Columns.Add("WaitingTaskCount", typeof(int));
            return dt.Clone();
        }

        /// <summary>
        /// Get cache properties as dictionary.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetPerformanceProperties()
        {
            Dictionary<string, object> prop = new Dictionary<string, object>();

            string unitSize = "Byte";
            int factor = 1;
            CachePerformanceCounter.GetFactorSize(MemoSize, out factor, out unitSize);
            UnitSize = unitSize;

            prop["AgentType"] = AgentType;
            prop["CounterName"] = CounterName;
            prop["ItemsCount"] = ItemsCount;
            prop["RequestCount"] = RequestCount;
            prop["ResponseCountPerHour"] = ResponseCountPerHour;
            prop["ResponseCountPerDay"] = ResponseCountPerDay;
            prop["ResponseCountPerMonth"] = ResponseCountPerMonth;
            prop["SyncCount"] = SyncCount;
            prop["StartTime"] = StartTime;
            prop["LastRequestTime"] = LastRequestTime;
            prop["LastResponseTime"] = LastResponseTime;
            prop["LastSyncTime"] = LastSyncTime;
            prop["MaxHitPerMinute"] = MaxHitPerMinute;
            prop["AvgHitPerMinute"] = AvgHitPerMinute;
            prop["AvgResponseTime"] = AvgResponseTime;
            prop["AvgSyncTime"] = AvgSyncTime;
            prop["MaxSize"] = MaxSize/factor;
            prop["MemorySize"] = MemoSize / factor;
            prop["FreeSize"] = FreeSize / factor;
            //prop["MemoryUsage"] = GetMemoryUsage(factor);
            prop["UnitSize"] = UnitSize;
            prop["WaitingTaskCount"] = WaitingTaskCount();
           

            prop["IntervalMinute"] = Owner.IntervalSeconds;
            prop["Initialized"] = Owner.Initialized;
            prop["IsRemote"] = Owner.IsRemote;

            return prop;
        }

        /// <summary>
        /// Get view as <see cref="DataTable"/>
        /// </summary>
        public DataTable GetDataView()
        {
            DataTable dt = CachePerformanceSchema();
            dt.Rows.Add(GetItemArray());
            return dt;
        }

        /// <summary>
        /// Get array of performance properties.
        /// </summary>
        /// <returns></returns>
        public object[] GetItemArray()
        {
            string unitSize = "Byte";
            int factor = 1;
            CachePerformanceCounter.GetFactorSize(MemoSize, out factor, out unitSize);
            UnitSize = unitSize;

            return new object[]{
            AgentType.ToString(),
            CounterName,
            ItemsCount,
            RequestCount,
            ResponseCountPerHour,
            ResponseCountPerDay,
            ResponseCountPerMonth,
            SyncCount,
            StartTime,
            LastRequestTime,
            LastResponseTime,
            LastSyncTime,
            MaxHitPerMinute,
            AvgHitPerMinute,
            AvgResponseTime,
            AvgSyncTime,
            MaxSize/ factor,
            MemoSize/ factor,
            FreeSize/ factor,
            //GetMemoryUsage(factor),
            UnitSize,
            WaitingTaskCount()};

        }

        #region Size properties

        const Int64 MAX_LONG = Int64.MaxValue-1000000;
        const int MAX_INT = int.MaxValue-100000;

        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        public long MaxSize { get; internal set; }

        //long _ByteSize;

        long _MemoSize;
        

        /// <summary>
        /// Get memory size for current item in bytes as an atomic operation.
        /// </summary>
        public long MemoSize
        {
            get { return Interlocked.Read(ref _MemoSize); }
        }
        /// <summary>
        /// Get the free size memory in bytes for current item as an atomic operation.
        /// </summary>
        public long FreeSize
        {
            get { return MaxSize > 0 ? MaxSize - MemoSize : long.MaxValue; }
        }

        /// <summary>
        /// Get the unit size (byte|Kb|Mb)
        /// </summary>
        public string UnitSize { get; set; }

        //Count = cache.Count;
        //   FreeSize = cache.FreeSize;// / 1024;
        //   MaxSize = cache.MaxSize;// / 1024;
        //   Usage = cache.Usage;// / 1024;

        /// <summary>
        /// Refresh memory size async.
        /// </summary>
        public void RefreshSize()
        {
            Task.Factory.StartNew(() => RefreshSizeInternal());
        }

        /// <summary>
        /// Refresh memory size.
        /// </summary>
        void RefreshSizeInternal()
        {
            Owner.MemorySizeExchange(ref _MemoSize);
            //Interlocked.Exchange(ref _MemoSize, size);
        }

        internal CacheState SizeValidate(long newSize)
        {
            long freeSize = FreeSize;
            return freeSize > newSize ? CacheState.Ok : CacheState.CacheIsFull;
        }

        internal void ExchangeSizeAndCountAsync(long oldSize, long newSize, int oldCount, int newCount, bool exchange, bool enableSizeHandler)
        {
            Task.Factory.StartNew(() => ExchangeSizeAndCount(oldSize, newSize, oldCount, newCount, exchange, enableSizeHandler));
        }

        CacheState ExchangeSizeAndCount(long oldSize, long newSize, int oldCount, int newCount, bool exchange, bool enableSizeHandler)
        {
            try
            {
                //long originalMemo = 0;

                if (enableSizeHandler)
                    ValidateSize(newSize - oldSize);// 1024);

                if (((MAX_LONG - _ItemsCount) <= newCount) || ((MAX_LONG - _MemoSize) <= newSize))
                {
                    ResetCounter();
                }

                if (exchange)
                {
                    Interlocked.Exchange(ref _ItemsCount, newCount);
                    Interlocked.Exchange(ref _MemoSize, newSize);

                    //originalMemo = Interlocked.Exchange(ref _ByteSize, newSize);
                    //Interlocked.Exchange(ref _MemoSize, _ByteSize);/// 1024);
                }
                else
                {
                    Interlocked.Add(ref _ItemsCount, newCount - oldCount);
                    Interlocked.Add(ref _MemoSize, (newSize - oldSize));

                    //originalMemo = Interlocked.Add(ref _ByteSize, (newSize - oldSize));
                    //Interlocked.Exchange(ref _MemoSize, _ByteSize);// / 1024);
                }

                return CacheState.Ok;
            }
            catch (CacheException cex)
            {
                CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.Error, "ExchangeSizeAndCount CacheException: " + cex.Message);
                if (cex.State == CacheState.CacheIsFull)
                {
                    //throw cex;
                }
                return CacheState.CacheIsFull;
            }

            catch (OverflowException oex)
            {
                ResetCounter();
                CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.Error, "ExchangeSizeAndCount OverflowException: " + oex.Message);
                //throw new CacheException(CacheState.CacheIsFull, this.CounterName + " memory is full!!!");
                return CacheState.CacheIsFull;
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.MemorySizeExchange, CacheActionState.Error, "ExchangeSizeAndCount error: " + ex.Message);
                return CacheState.UnexpectedError;
            }
        }

        void ValidateSize(long sizeToAdd)
        {

            long memo = Interlocked.Read(ref _MemoSize);
            if (memo < 0)
            {
                Interlocked.Exchange(ref _MemoSize, 0);
                RefreshSizeInternal();
            }
            else if (sizeToAdd > 0 && (sizeToAdd + memo) > MaxSize && MaxSize > 0)
            {
                RefreshSizeInternal();
                //double check
                if (sizeToAdd > 0 && (sizeToAdd + memo) > MaxSize && MaxSize > 0)
                {
                    //IsFull = true;
                    throw new CacheException(CacheState.CacheIsFull, this.CounterName + " memory is full!!!");
                }
            }
        }


        #endregion


        #region static

        /// <summary>
        /// Convert name argument to <see cref="CacheAgentType"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CacheAgentType GetAgent(string name)
        {
           return EnumExtension.Parse<CacheAgentType>(name, CacheAgentType.Cache);
        }

        public static void GetReadableSize(long MemoSize, out long size, out string unitSize)
        {
            int factor = 1;
            unitSize = "Byte";

            if (MemoSize > 1024)
            {
                factor = 1024;
                unitSize = "Kb";
            }
            if (MemoSize > 1048576)
            {
                factor = 1048576;
                unitSize = "Mb";
            }
            size = MemoSize / factor;
        }

        public static void GetFactorSize(long MemoSize, out int factor, out string unitSize)
        {
            unitSize = "Byte";
            factor = 1;
            if (MemoSize > 1024)
            {
                factor = 1024;
                unitSize = "Kb";
            }
            if (MemoSize > 1048576)
            {
                factor = 1048576;
                unitSize = "Mb";
            }
        }


        /// <summary>
        /// Get memory usage in bytes.
        /// </summary>
        /// <returns></returns>
        public static long GetMemoryUsage(int factor=1)
        {
            //string execName = SysNet.GetExecutingAssemblyName();
            //System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName(execName);
            //long usage = 0;
            //if (process == null)
            //    return 0;
            //for (int i = 0; i < process.Length; i++)
            //{
            //    usage += (int)((int)process[i].WorkingSet64);
            //}

            var process= Process.GetCurrentProcess();
            long usage = process.WorkingSet64;
            return usage/ factor;
        }
        #endregion
    }

     /// <summary>
    /// Represent cache performance counter report.
    /// </summary>
    [Serializable]
    public class CacheStateCounterReport : ISerialEntity
    {
        /// <summary>
        /// Init a new instance of <see cref="CacheStateCounterReport"/>
        /// </summary>
        public CacheStateCounterReport()
        {
            dtReport = CacheStateCounterSchema();
        }


        internal void AddItemReport(CachePerformanceCounter agent)
        {
            foreach (var entry in agent.StateCounter())
            {
                if (entry.Value > 0)
                    dtReport.Rows.Add(agent.AgentType.ToString(), entry.Key.ToString(), entry.Value);
            }
        }

        DataTable dtReport;
        /// <summary>
        /// Get Cache prformance report
        /// </summary>
        /// <returns></returns>
        [Serialize]
        public DataTable StateReport
        {
            get { return dtReport; }
        }

        internal static DataTable CacheStateCounterSchema()
        {

            DataTable dt = new DataTable("CacheStateCounter");
            dt.Columns.Add("AgentType", typeof(string));
            dt.Columns.Add("StateName", typeof(string));
            dt.Columns.Add("StateCount", typeof(long));
            //dt.Columns.Add("LastState", typeof(DateTime));
            return dt.Clone();
        }

        #region  ISerialEntity


        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            streamer.WriteValue(dtReport);
            streamer.Flush();
        }


        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            dtReport = streamer.ReadValue<DataTable>();
            if (streamer.BaseStream != null && streamer.BaseStream.CanSeek)
                streamer.BaseStream.Position = 0;

        }
        #endregion

    }

    /// <summary>
    /// Represent cache performance counter report.
    /// </summary>
    [Serializable]
    public class CachePerformanceReport:ICachePerformanceReport,ISerialEntity
    {
        /// <summary>
        /// Initialize a new instance of cache performance report.
        /// </summary>
        public CachePerformanceReport()
        {
            dtReport = CachePerformanceCounter.CachePerformanceSchema();
            CounterName = "SummarizeReport";
        }

        /// <summary>
        /// Initialize a new instance of cache performance report.
        /// </summary>
        /// <param name="agent"></param>
        public CachePerformanceReport(CacheAgentType agent)
        {
            dtReport = CachePerformanceCounter.CachePerformanceSchema();
            CounterName = agent.ToString();
        }

        internal void InitReport()
        {
            dtReport.Clear();
            ItemsCount = 0;
            RequestCount = 0;
            ResponseCountPerHour = 0;
            ResponseCountPerDay = 0;
            ResponseCountPerMonth = 0;
            MaxSize = CacheSettings.MaxSize;
            FreeSize = MaxSize;
            MemoSize = 0;
            SyncCount = 0;
            AvgResponseTime = 0;
            AvgSyncTime = 0;
            AvgHitPerMinute = 0;
            MaxHitPerMinute = 0;
            StartTime = DateTime.Now.AddHours(-1);
            LastRequestTime = DateTime.Now.AddHours(-1);
            LastResponseTime = DateTime.Now.AddHours(-1);
            LastSyncTime = DateTime.Now.AddHours(-1);
            WaitingTaskCount = AgentManager.PerformanceTasker.Count;
        }

        internal void AddItemReport(CachePerformanceCounter agent)
        {
           
            dtReport.Rows.Add(agent.GetItemArray());

            ItemsCount += agent.ItemsCount;
            RequestCount += agent.RequestCount;
            ResponseCountPerHour += agent.ResponseCountPerHour;
            ResponseCountPerDay += agent.ResponseCountPerDay;
            ResponseCountPerMonth += agent.ResponseCountPerMonth;
            //FreeSize += agent.FreeSize;
            //MaxSize += agent.MaxSize;
            MemoSize += agent.MemoSize;
            //FreeSize = MaxSize - MemoSize;
            SyncCount += agent.SyncCount;

            if (AvgResponseTime > 0 && agent.AvgResponseTime > 0)
                AvgResponseTime = (AvgResponseTime + agent.AvgResponseTime) / 2;
            else if (agent.AvgResponseTime > 0)
                AvgResponseTime = agent.AvgResponseTime;

            if (AvgSyncTime > 0 && agent.AvgSyncTime > 0)
                AvgSyncTime = (AvgSyncTime + agent.AvgSyncTime) / 2;
            else if (agent.AvgSyncTime > 0)
                AvgSyncTime = agent.AvgSyncTime;

            if (AvgHitPerMinute > 0 && agent.AvgHitPerMinute > 0)
                AvgHitPerMinute = (AvgHitPerMinute + agent.AvgHitPerMinute) / 2;
            else if (agent.AvgHitPerMinute > 0)
                AvgHitPerMinute = agent.AvgHitPerMinute;

            MaxHitPerMinute = Math.Max(MaxHitPerMinute, agent.MaxHitPerMinute);

            StartTime = agent.StartTime > StartTime ? StartTime : agent.StartTime;
            LastRequestTime = agent.LastRequestTime < LastRequestTime ? LastRequestTime : agent.LastRequestTime;
            LastResponseTime = agent.LastResponseTime < LastResponseTime ? LastResponseTime : agent.LastResponseTime;
            LastSyncTime = agent.LastSyncTime < LastSyncTime ? LastSyncTime : agent.LastSyncTime;
          
        }

        internal void ResetCounter(CachePerformanceCounter agent)
        {
            dtReport.Rows.Clear();
            agent.ClearStateCounter();
            agent.InitCounter();
            agent.RefreshSize();
        }

        internal void AddStateCountReport(CachePerformanceCounter agent)
        {

        }

        internal void AddTotalReport()
        {

            string unitSize = "Byte";
            int factor = 1;
            CachePerformanceCounter.GetFactorSize(MemoSize, out factor, out unitSize);
            UnitSize = unitSize;


            dtReport.Rows.Add(new object[]{
            "Report",
            "Summarize",
            ItemsCount,
            RequestCount,
            ResponseCountPerHour,
            ResponseCountPerDay,
            ResponseCountPerMonth,
            SyncCount,
            StartTime,
            LastRequestTime,
            LastResponseTime,
            LastSyncTime,
            MaxHitPerMinute,
            AvgHitPerMinute,
            AvgResponseTime,
            AvgSyncTime,
            MaxSize/factor,
            MemoSize/factor,
            FreeSize/factor,
            //CachePerformanceCounter.GetMemoryUsage(factor),
            UnitSize,
            WaitingTaskCount});
        }

        #region members
        internal int WaitingTaskCount;

        /// <summary>
        /// Get Counter Name.
        /// </summary>
        public string CounterName { get; internal set; }
                
        /// <summary>
        /// Get Items count as an atomic operation.
        /// </summary>
        public long ItemsCount { get; private set;  }

        
        /// <summary>
        /// Get Request count as an atomic operation.
        /// </summary>
        public long RequestCount {  get; private set; }

        /// <summary>
        /// Get Response count per hour as an atomic operation.
        /// </summary>
        public long ResponseCountPerHour { get; private set; }
      
        /// <summary>
        /// Get Response count per day as an atomic operation.
        /// </summary>
        public long ResponseCountPerDay { get; private set; }

        
        /// <summary>
        /// Get Response count per month as an atomic operation.
        /// </summary>
        public long ResponseCountPerMonth { get; private set; }

        /// <summary>
        /// Get Sync count as an atomic operation.
        /// </summary>
        public long SyncCount { get; private set; }

        /// <summary>
        /// Get Start Time.
        /// </summary>
        public DateTime StartTime { get; internal set; }
        /// <summary>
        /// Get the last time of request action.
        /// </summary>
        public DateTime LastRequestTime { get; internal set; }
        /// <summary>
        /// Get the last time of response action.
        /// </summary>
        public DateTime LastResponseTime { get; internal set; }
        /// <summary>
        /// Get Last Sync Time.
        /// </summary>
        public DateTime LastSyncTime { get; internal set; }
        /// <summary>
        /// Get Max Hit Per Minute.
        /// </summary>
        public int MaxHitPerMinute { get; internal set; }
        /// <summary>
        /// Get the avarage hit Per Minute.
        /// </summary>
        public int AvgHitPerMinute { get; internal set; }
        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        public long MaxSize { get; internal set; }

       
        /// <summary>
        /// Get memory size for current item in bytes as an atomic operation.
        /// </summary>
        public long MemoSize
        {
            get;
            internal set; 
        }
        /// <summary>
        /// Get the unit size (byte|Kb|Mb)
        /// </summary>
        public string UnitSize { get; set; }

        /// <summary>
        /// Get the free size memory in bytes for current item as an atomic operation.
        /// </summary>
        public long FreeSize
        {
            get { return MaxSize - MemoSize; }
            internal set { }
        }

        /// <summary>
        /// Get avarage response time.
        /// </summary>
        public float AvgResponseTime
        {
            get; internal set; 
        }

        /// <summary>
        /// Get avarage sync time.
        /// </summary>
        public float AvgSyncTime
        {
           get; internal set; 
        }

        #endregion

        DataTable dtReport;
        /// <summary>
        /// Get Cache prformance report
        /// </summary>
        /// <returns></returns>
        [Serialize]
        public DataTable PerformanceReport
        {
            get { return dtReport; }
        }


        #region  ISerialEntity


        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            streamer.WriteString(CounterName);
            streamer.WriteValue(ItemsCount);
            streamer.WriteValue(RequestCount);
            streamer.WriteValue(ResponseCountPerHour);
            streamer.WriteValue(ResponseCountPerDay);
            streamer.WriteValue(ResponseCountPerMonth);
            streamer.WriteValue(SyncCount);
            streamer.WriteValue(StartTime);
            streamer.WriteValue(LastRequestTime);
            streamer.WriteValue(LastResponseTime);
            streamer.WriteValue(LastSyncTime);
            streamer.WriteValue(MaxHitPerMinute);
            streamer.WriteValue(AvgHitPerMinute);
            streamer.WriteValue(MaxSize);
            streamer.WriteValue(MemoSize);
            //streamer.WriteValue(FreeSize);
            streamer.WriteString(UnitSize);
            streamer.WriteValue(AvgResponseTime);
            streamer.WriteValue(AvgSyncTime);
            streamer.WriteValue(dtReport);
            streamer.Flush();
        }


        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            CounterName = streamer.ReadString();
            ItemsCount = streamer.ReadValue<long>();
            RequestCount = streamer.ReadValue<long>();
            ResponseCountPerHour = streamer.ReadValue<long>();
            ResponseCountPerDay = streamer.ReadValue<long>();
            ResponseCountPerMonth = streamer.ReadValue<long>();
            SyncCount = streamer.ReadValue<long>();
            StartTime = streamer.ReadValue<DateTime>();
            LastRequestTime = streamer.ReadValue<DateTime>();
            LastResponseTime = streamer.ReadValue<DateTime>();
            LastSyncTime = streamer.ReadValue<DateTime>();
            MaxHitPerMinute = streamer.ReadValue<int>();
            AvgHitPerMinute = streamer.ReadValue<int>();
            MaxSize = streamer.ReadValue<long>();
            MemoSize = streamer.ReadValue<long>();
            //FreeSize = streamer.ReadValue<long>();
            UnitSize = streamer.ReadString();
            AvgResponseTime = streamer.ReadValue<long>();
            AvgSyncTime = streamer.ReadValue<long>();
            dtReport = streamer.ReadValue<DataTable>();
            if (streamer.BaseStream != null && streamer.BaseStream.CanSeek)
                streamer.BaseStream.Position = 0;

        }
         #endregion


    }

}
