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
using System.Collections;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Data;
using System.Runtime.InteropServices;

using Nistec;
using Nistec.Data;
using Nistec.Drawing;
using Nistec.Data.Advanced;
using Nistec.Runtime;
using Nistec.Serialization;
using Nistec.Channels;

namespace Nistec.Caching
{
    /// <summary>
    /// Represent utility functions for cache.
    /// </summary>
    public static class CacheUtil
    {
        internal static TransType ToTransType(CacheState state)
        {
            if ((int)state >= 500)
                return TransType.Error;
            else
                return TransType.Info;
        }
        internal static MessageState ToMessageState(CacheState state)
        {

            switch (state)
            {
                case CacheState.ItemAdded:
                case CacheState.ItemChanged:
                case CacheState.ItemRemoved:
                case CacheState.Ok:
                    return MessageState.Ok;

                case CacheState.NotFound:
                    return MessageState.ItemNotFound;
                case CacheState.UnexpectedError:
                    return MessageState.UnexpectedError;
                case CacheState.SerializationError:
                    return MessageState.SerializeError;
                case CacheState.CommandNotSupported:
                    return MessageState.NotSupportedError;


                case CacheState.AddItemFailed:
                case CacheState.MergeItemFailed:
                case CacheState.SetItemFailed:
                case CacheState.RemoveItemFailed:
                case CacheState.ItemAllreadyExists:
                    return MessageState.Failed;



                case CacheState.InvalidItem:
                case CacheState.InvalidSession:
                case CacheState.ArgumentsError:
                    return MessageState.ArgumentsError;

                case CacheState.CacheNotReady:
                case CacheState.CacheIsFull:
                    return MessageState.OperationError;


                default:

                    if ((int)state >= 500)
                        return MessageState.Failed;
                    else
                        return MessageState.Ok;
            }
        }
        /// <summary>
        /// Split str with trim.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitter"></param>
        /// <returns></returns>
        public static string[] SplitStrTrim(string str, char splitter=',')
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException("SplitStrTrim.str");
            }
            string[] args = str.Split(splitter);
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = args[i].Trim();
            }
            return args;
        }
        /// <summary>
        /// Split str with trim.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <param name="splitter"></param>
        /// <returns></returns>
        public static string[] SplitStrTrim(string str, string defaultValue, char splitter = ',')
        {
            string[] list = string.IsNullOrEmpty(str) ? new string[] { defaultValue } : CacheUtil.SplitStrTrim(str);
            return list;
        }

        /// <summary>
        /// GetUsage in KB
        /// </summary>
        /// <returns></returns>
        public static int GetUsage()
        {
            string execName = SysNet.GetExecutingAssemblyName();
            System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName(execName);
            int usage = 0;
            if (process == null)
                return 0;
            for (int i = 0; i < process.Length; i++)
            {
                usage += (int)((int)process[i].WorkingSet64) / 1024;
            }

            return usage;
        }
   
        /// <summary>
        /// Load file.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static object LoadFileSource(CacheObjType type, string filename)
        {
            switch (type)
            {
                case CacheObjType.ImageFile:
                    return (string)CacheUtil.LoadImageFileSource(filename);
                case CacheObjType.BinaryFile:
                    return (string)CacheUtil.LoadBinaryFileSource(filename);
                case CacheObjType.TextFile:
                    return (string)CacheUtil.LoadTextFileSource(filename);
                case CacheObjType.HtmlFile:
                    return (string)CacheUtil.LoadHtmlSource(filename);
                case CacheObjType.XmlDocument:
                    return (string)CacheUtil.LoadXmlSource(filename);
            }
            return null;
        }
        /// <summary>
        /// Load file as byte array.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static byte[] LoadFileSourceToBytes(CacheObjType type, string filename)
        {
            switch (type)
            {
                case CacheObjType.ImageFile:
                    return CacheUtil.LoadImageFileSourceToBytes(filename);
                case CacheObjType.BinaryFile:
                    return CacheUtil.LoadBinaryFileSourceToBytes(filename);
                case CacheObjType.TextFile:
                    return CacheUtil.LoadTextFileSourceToBytes(filename);
                case CacheObjType.HtmlFile:
                    return CacheUtil.LoadHtmlSourceToBytes(filename);
                case CacheObjType.XmlDocument:
                    return CacheUtil.LoadXmlSourceToBytes(filename);
            }
            return null;
        }
        /// <summary>
        /// Deserialize cache object.
        /// </summary>
        /// <param name="cacheObjType"></param>
        /// <param name="source"></param>
        /// <param name="value"></param>
        public static void Deserialize(CacheObjType cacheObjType, string source, ref object value)
        {
            switch (cacheObjType)
            {
                case CacheObjType.ImageFile:
                    value = CacheUtil.DeserializeImage(source);
                    break;
                case CacheObjType.BinaryFile:
                case CacheObjType.TextFile:
                case CacheObjType.HtmlFile:
                    break;
                case CacheObjType.XmlDocument:
                    value = CacheUtil.DeserializeXml(source);
                    break;
                case CacheObjType.Default:
                    break;
                case CacheObjType.RemotingData:
                    value = BinarySerializer.DeserializeFromBase64(source);
                    break;
                case CacheObjType.SerializeClass:
                    value = CacheUtil.DeserializeClass(source);
                    break;
            }
        }
        /// <summary>
        /// Serialize cache object type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static object Serialize(object value,/*ref int size,*/ref CacheObjType objType)
        {
            Type typ = value.GetType();
            string result = null;

            if (typ.IsPrimitive || typ == typeof(string) || typ == typeof(Guid) || typ == typeof(Decimal))
            {
                objType = CacheObjType.Default;
                
                return value;
            }
            else if (typ.IsValueType)
            {
                objType = CacheObjType.SerializeClass;
                result = NetSerializer.StructureToBase64(value);
                
                return result;
            }
            else if (typ == typeof(DataTable) || typ == typeof(DataSet))
            {
                objType = CacheObjType.RemotingData;
                result = NetSerializer.SerializeToBase64(value);
                return result;
            }
            else if (typ.IsSerializable)
            {
                objType = CacheObjType.SerializeClass;
                result = NetSerializer.SerializeToBase64(value);
                
                return result;
            }
            else
            {
                Packet p = new Packet(value);
                
                objType = CacheObjType.SerializeClass;

                return p.Serialize();
            }
        }

     /// <summary>
        /// Structure To Byte Array
     /// </summary>
     /// <param name="obj"></param>
     /// <returns></returns>
        public static byte[] StructureToByteArray(object obj)
        {
            int Length = Marshal.SizeOf(obj);
            byte[] bytearray = new byte[Length];
            IntPtr ptr = Marshal.AllocHGlobal(Length);
            Marshal.StructureToPtr(obj, ptr, false);
            Marshal.Copy(ptr, bytearray, 0, Length);
            Marshal.FreeHGlobal(ptr);
            return bytearray;
        }
        /// <summary>
        /// Byte Array To Structure
        /// </summary>
        /// <param name="bytearray"></param>
        /// <param name="obj"></param>
        public static void ByteArrayToStructure(byte[] bytearray, ref object obj)
        {
            int Length = bytearray.Length;// Marshal.SizeOf(obj);
            IntPtr ptr = Marshal.AllocHGlobal(Length);
            Marshal.Copy(bytearray, 0, ptr, Length);
            obj = Marshal.PtrToStructure(ptr, obj.GetType());
            Marshal.FreeHGlobal(ptr);
        }
        /// <summary>
        /// Byte Array To Structure
        /// </summary>
        /// <param name="bytearray"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ByteArrayToStructure(byte[] bytearray, Type type)
        {
            int Length =bytearray.Length;// Marshal.SizeOf(obj);
            IntPtr ptr = Marshal.AllocHGlobal(Length);
            Marshal.Copy(bytearray, 0, ptr, Length);
            object result = Marshal.PtrToStructure(ptr, type);
            Marshal.FreeHGlobal(ptr);
            return result;
        }


        /// <summary>
        /// Get object size in Kbytes.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int GetItemSize(object obj)
        {
            try
            {
                if (obj == null)
                    return 0;
                Type type = obj.GetType();

                if (type == typeof(string))
                    return Encoding.UTF8.GetByteCount(obj.ToString()) / 1024;
                if (type == typeof(DataSet))
                    return DataSetUtil.DataSetToByteCount((DataSet)obj, false) / 1024;
                if (type == typeof(DataTable))
                {
                    DataSet ds = new DataSet();
                    ds.Tables.Add((DataTable)obj);
                    return DataSetUtil.DataSetToByteCount(ds, false) / 1024;
                }
                if (type.IsValueType)
                {
                    return Marshal.SizeOf(obj) / 1024;
                }
                if (type.IsSerializable)
                {
                    return NetSerializer.SizeOf(obj) / 1024;
                }
                return Marshal.SizeOf(obj) / 1024;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Get object size in bytes.
        /// </summary>
        /// <param name="objA"></param>
        /// <param name="objB"></param>
        /// <returns></returns>
        public static int[] SizeOf(object objA, object objB)
        {
            return new int[] { SizeOf(objA), SizeOf(objB) };
        }

        /// <summary>
        /// Get object size in bytes.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int SizeOf(object obj)
        {
            try
            {
                if (obj == null)
                    return 0;
                Type type = obj.GetType();

                if (type == typeof(string))
                    return Encoding.UTF8.GetByteCount(obj.ToString());
                if (type.IsValueType)
                    return Marshal.SizeOf(obj);
                if (type.IsSerializable)
                    return NetSerializer.SizeOf(obj);

                return BinarySerializer.SizeOf(obj);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        
        /// <summary>
        /// Deserialize Class FromBase64
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static object DeserializeClass(string source)
        {
            try
            {
                return NetSerializer.DeserializeFromBase64(source);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Serialize Class  to Base64
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string SerializeClass(object source)
        {
            try
            {
                return NetSerializer.SerializeToBase64(source);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Serialize object to base64 string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Serialize(object value)
        {
            Type typ = value.GetType();
            if (typ.IsPrimitive || typ == typeof(string) || typ == typeof(Guid) || typ == typeof(Decimal))
            {
                return value.ToString();
            }
            else if (typ.IsValueType)
            {
                return NetSerializer.StructureToBase64(value);
            }
            else if (typ == typeof(DataTable) || typ == typeof(DataSet))
            {
                return NetSerializer.SerializeToBase64(value);
            }
            else if (typ.IsSerializable)
            {
                return NetSerializer.SerializeToBase64(value);
            }
            else
            {
                Packet p = new Packet(value);
                return p.Serialize();
            }
        }

        /// <summary>
        /// Deserialize Image
        /// </summary>
        /// <param name="base64Stream"></param>
        /// <returns></returns>
        public static System.Drawing.Image DeserializeImage(string base64Stream)
        {
            try
            {
                Nistec.Drawing.ImageUtil iu = new Nistec.Drawing.ImageUtil();
                iu.LoadFromBase64Stream(base64Stream);
                return iu.Image;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Deserialize Xml
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static System.Xml.XmlDocument DeserializeXml(string source)
        {
            try
            {
                System.Xml.XmlDocument doc = new XmlDocument();
                doc.LoadXml(source);
                return doc;
            }
            catch
            {
                return null;
            }
        }

        #region Load files

        /// <summary>
        /// Load Text File Source
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string LoadTextFileSource(string filename)
        {
            return IoHelper.FileToString(filename);

        }
        /// <summary>
        /// Load Binary File Source
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string LoadBinaryFileSource(string filename)
        {
            byte[] bytes = IoHelper.ReadBinaryStream(filename);
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// Load Image File Source
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string LoadImageFileSource(string url)
        {
            ImageUtil iu = new ImageUtil();
            iu.Load(url);
            return iu.ImageStream;
        }
        /// <summary>
        /// Load Image File
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Image LoadImageFile(string url)
        {
            ImageUtil iu = new ImageUtil();
            iu.Load(url);
            return iu.Image;
        }
        /// <summary>
        /// Load Xml File
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static System.Xml.XmlDocument LoadXmlFile(string filename)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(filename);
            return doc;
        }
        /// <summary>
        /// Load Xml Source
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string LoadXmlSource(string filename)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(filename);
            return doc.OuterXml;
        }
        /// <summary>
        /// Load Html Source
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string LoadHtmlSource(string filename)
        {
            string text = null;
            using (System.IO.StreamReader reader = System.IO.File.OpenText(filename))
            {
                text = reader.ReadToEnd();
                reader.Close();
            }
            return text;
        }

        #endregion

        #region load file to binary
        /// <summary>
        /// Load Text File Source To Bytes
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static byte[] LoadTextFileSourceToBytes(string filename)
        {
            string s= IoHelper.FileToString(filename);
            return UTF8Encoding.UTF8.GetBytes(s);

        }
        /// <summary>
        /// Load Binary File Source To Bytes
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static byte[] LoadBinaryFileSourceToBytes(string filename)
        {
            return IoHelper.ReadBinaryStream(filename);
        }
        /// <summary>
        /// Load Image File Source To Bytes
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static byte[] LoadImageFileSourceToBytes(string url)
        {
            ImageUtil iu = new ImageUtil();
            iu.Load(url);
            return iu.ImageToBytes();
        }
        /// <summary>
        /// Load Xml Source To Bytes
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static byte[] LoadXmlSourceToBytes(string filename)
        {
            string s = LoadXmlSource(filename);
            return UTF8Encoding.UTF8.GetBytes(s);
        }
        /// <summary>
        /// Load Html Source To Bytes
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static byte[] LoadHtmlSourceToBytes(string filename)
        {
            string s = LoadHtmlSource(filename);
            return UTF8Encoding.UTF8.GetBytes(s);
        }


        #endregion

        #region Merge
        /// <summary>
        /// Merge collection items.
        /// </summary>
        /// <param name="srcValue"></param>
        /// <param name="destValue"></param>
        /// <param name="count"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static object Merge(object srcValue ,object destValue,ref int count, ref int size)
        {

            if (srcValue.GetType() == typeof(Hashtable) && destValue.GetType() == typeof(Hashtable))
            {
                Hashtable src = (Hashtable)srcValue;
                Hashtable dest = (Hashtable)destValue;
                if (dest == null || src == null)
                    return -1;

                IEnumerator en = dest.Keys.GetEnumerator();
                while (en.MoveNext())
                {
                    src[en.Current] = dest[en.Current];
                }
                count = src.Count;
                return NetSerializer.SerializeToBase64(src, ref size);

            }
            else  if (srcValue.GetType() == typeof(ArrayList) && destValue.GetType() == typeof(ArrayList))
            {
                ArrayList src = (ArrayList)srcValue;
                ArrayList dest = (ArrayList)destValue;
                if (dest == null || src == null)
                    return -1;

                IEnumerator en = dest.GetEnumerator();
                while (en.MoveNext())
                {
                    src.Add(en.Current);
                }
                count = src.Count;
                return NetSerializer.SerializeToBase64(src, ref size);
            }
            else if (srcValue.GetType() == typeof(DataTable) && destValue.GetType() == typeof(DataTable))
            {
                DataTable src = (DataTable)srcValue;
                DataTable dest = (DataTable)destValue;
                if (dest == null || src == null)
                    return -1;
                src.Merge(dest);
                size = CacheUtil.GetItemSize(src);

                return NetSerializer.SerializeToBase64(src, ref size);
            }

            throw new Exception("item type not supported");
        }
        /// <summary>
        /// Merge remove collection items.
        /// </summary>
        /// <param name="srcValue"></param>
        /// <param name="destValue"></param>
        /// <param name="count"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static object Remove(object srcValue, object destValue, ref int count, ref int size)
        {

            if (srcValue.GetType() == typeof(Hashtable) && destValue.GetType() == typeof(Hashtable))
            {
                Hashtable src = (Hashtable)srcValue;
                Hashtable dest = (Hashtable)destValue;
                if (dest == null || src == null)
                    return -1;

                IEnumerator en = dest.Keys.GetEnumerator();
                while (en.MoveNext())
                {
                    src.Remove(en.Current);
                }
                count = src.Count;
                return NetSerializer.SerializeToBase64(src, ref size);

            }
            else if (srcValue.GetType() == typeof(ArrayList) && destValue.GetType() == typeof(ArrayList))
            {
                ArrayList src = (ArrayList)srcValue;
                ArrayList dest = (ArrayList)destValue;
                if (dest == null || src == null)
                    return -1;

                IEnumerator en = dest.GetEnumerator();
                while (en.MoveNext())
                {
                    src.Remove(en.Current);
                }
                count = src.Count;
                return NetSerializer.SerializeToBase64(src, ref size);
            }
            else if (srcValue.GetType() == typeof(DataTable) && destValue.GetType() == typeof(DataTable))
            {
                DataTable src = (DataTable)srcValue;
                DataTable dest = (DataTable)destValue;
                if (dest == null || src == null)
                    return -1;

                foreach (DataRow dr in dest.Rows)
                {
                    src.Rows.Remove(dr);
                }
                size = CacheUtil.GetItemSize(src);
                return src;
            }

            throw new Exception("item type not supported");
        }
        #endregion
    }
}
