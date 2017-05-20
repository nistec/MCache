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

namespace Nistec.Caching
{
    /// <summary>
    /// SyncType
    /// </summary>
    public enum SyncType
    {
        /// <summary>
        /// No sync time
        /// </summary>
        None,
        /// <summary>
        /// Dal SyncType By Day 
        /// </summary>
        Daily,
        /// <summary>
        /// Dal SyncType By Interval
        /// </summary>
        Interval,
         /// <summary>
        /// Dal SyncType By Event
        /// </summary>
        Event,
        /// <summary>
        /// Remove SyncType
        /// </summary>
        Remove
    }

    /// <summary>
    /// SyncOption
    /// </summary>
    public enum SyncOption
    {
        /// <summary>
        /// Manual
        /// </summary>
        Manual,
        /// <summary>
        /// Auto
        /// </summary>
        Auto
    }

    /// <summary>
    /// Sync time struct
    /// </summary>
    [Serializable]
    public class SyncTimer
    {
     
        /// <summary>
        /// SyncType
        /// </summary>
        public SyncType SyncType { get; set; }
        /// <summary>
        /// lastRun
        /// </summary>
        private DateTime _lastRun;
        /// <summary>
        /// lastRun
        /// </summary>
        private TimeSpan _timeSpan;

        /// <summary>
        /// TimeElapsed
        /// </summary>
        public event EventHandler TimeElapsed;

        /// <summary>
        /// OnTimeElapsed
        /// </summary>
        /// <param name="e"></param>
        private void OnTimeElapsed(EventArgs e)
        {
            if (TimeElapsed != null)
                TimeElapsed(this, e);
        }

 
        /// <summary>
        /// SyncTimer constructor
        /// </summary>
        /// <param name="interval">TimeSpan</param>
        public SyncTimer(TimeSpan interval)
        {
            SyncType = SyncType.None;
            _lastRun = DateTime.Now;
            _timeSpan = interval;
            TimeElapsed = null;
        }
        /// <summary>
        /// SyncTimer constructor
        /// </summary>
        /// <param name="interval">TimeSpan</param>
        /// <param name="syncType">SyncType</param>
        public SyncTimer(TimeSpan interval, SyncType syncType)
        {
            SyncType = syncType;
            _lastRun = DateTime.Now;
            _timeSpan = interval;
            TimeElapsed = null;
        }

        /// <summary>
        /// Get or Set the time span Interval
        /// </summary>
        public TimeSpan Interval
        {
            get { return _timeSpan; }
            set
            {
                _timeSpan = value;
            }
        }

 
        /// <summary>
        /// Empty
        /// </summary>
        public static SyncTimer Empty
        {
            get
            {
                return new SyncTimer(TimeSpan.Zero);
            }
        }
        /// <summary>
        /// IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (SyncType== SyncType.None || _timeSpan.TotalMinutes==0);
            }
        }
        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals((SyncTimer) obj);
        }
        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Equals(SyncTimer obj)
        {
            return (obj.Interval == _timeSpan);
        }
        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// Total Minutes
        /// </summary>
        /// <returns></returns>
        public double ToDouble()
        {
            return (double)_timeSpan.TotalMinutes;
        }
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", _timeSpan.Days,_timeSpan.Hours,_timeSpan.Minutes);
        }
        /// <summary>
        /// GetValidTime
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public DateTime GetValidTime(DateTime d)
        {
            return new DateTime(d.Year, d.Month, d.Day, _timeSpan.Hours, _timeSpan.Minutes, _timeSpan.Seconds);
        }
        /// <summary>
        /// GetValidTime
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public DateTime GetNextValidTime(DateTime d)
        {
            DateTime nextDate = d;

            if (SyncType == SyncType.Daily)
            {
                nextDate = d.AddDays(1);
                nextDate = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, _timeSpan.Hours, _timeSpan.Minutes, _timeSpan.Seconds);
            }
            else //if (SyncType == SyncType.ByInterval)
            {
               nextDate= d.AddHours(_timeSpan.Hours).AddMinutes(_timeSpan.Minutes);
            }
            return nextDate;
        }

        /// <summary>
        /// GetNextValidTime
        /// </summary>
        /// <returns></returns>
        public DateTime GetNextValidTime()
        {
          return  GetNextValidTime(_lastRun);
        }

       
        /// <summary>
        /// Get value indicated if Has Time ToRun,when true update the next time to rnu
        /// </summary>
        /// <returns></returns>
        public bool HasTimeToRun()
        {

            if (SyncType == SyncType.None)
                return false;

            DateTime t= GetNextValidTime( _lastRun);
            
            if (t < DateTime.Now)
            {
                _lastRun =t;
                OnTimeElapsed(EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the Last Time was running
        /// </summary>
        /// <returns></returns>
        public DateTime GetLastTime()
        {
            return _lastRun;
        }
        /// <summary>
        /// Parse <see cref="SyncType"/> from string.
        /// </summary>
        /// <param name="syncType"></param>
        /// <returns></returns>
        public static SyncType SyncTypeFromString(string syncType)
        {
            try
            {
                return (SyncType)Enum.Parse(typeof(SyncType), syncType, true);
            }
            catch
            {
                return SyncType.None;
            }
        }
        /// <summary>
        /// Parse <see cref="TimeSpan"/> from string.
        /// </summary>
        /// <param name="timespan"></param>
        /// <returns></returns>
        public static TimeSpan TimeSpanFromString(string timespan)
        {
            int days=0;
            int hour = 0;
            int minute = 0;
           
            try
            {
                string[] s = timespan.Split(':');
                if (s != null && s.Length == 2)
                {
                    hour = Types.ToInt(s[0], 0);
                    minute = Types.ToInt(s[1], 0);
                }
                else if (s != null && s.Length >= 3)
                {
                    days = Types.ToInt(s[0], 0);
                    hour = Types.ToInt(s[1], 0);
                    minute = Types.ToInt(s[2], 0);
                }
                return new TimeSpan(days, hour, minute, 0);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
    }
}
