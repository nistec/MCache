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
using System.Runtime.InteropServices;

namespace Nistec.Caching
{
    /// <summary>
    /// Represent a memory packet for serialization.
    /// </summary>
    [Serializable()]
    public struct Packet
    {
        /// <summary>
        /// data.
        /// </summary>
        public object Data;
        /// <summary>
        /// item type.
        /// </summary>
        public Type ItemType;
        /// <summary>
        /// Create a new <see cref="Packet"/> item.
        /// </summary>
        public static Packet Empty
        {
            get { return new Packet(); }
        }
        /// <summary>
        /// Get indicate whether the item is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return Data == null; }
        }
        /// <summary>
        /// Initialize a new instance of <see cref="Packet"/> item.
        /// </summary>
        /// <param name="o"></param>
        public Packet(object o)
        {
            Data = o;
            if (o == null)
                ItemType = typeof(object);
            else
                ItemType = o.GetType();
        }
        /// <summary>
        /// Get the size of current item in bytes.
        /// </summary>
        public int Size
        {
            get { return Marshal.SizeOf(this) / 1024; }
        }
        /// <summary>
        /// Serialize item to base64 string.
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return Convert.ToBase64String(ToByteArray());
        }
        /// <summary>
        /// Convert item to byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            int Length = Marshal.SizeOf(this);
            byte[] bytearray = new byte[Length];
            IntPtr ptr = Marshal.AllocHGlobal(Length);
            Marshal.StructureToPtr(this, ptr, false);
            Marshal.Copy(ptr, bytearray, 0, Length);
            Marshal.FreeHGlobal(ptr);
            return bytearray;
        }
        /// <summary>
        /// Create <see cref="Packet"/> fro byte arrat.
        /// </summary>
        /// <param name="bytearray"></param>
        /// <returns></returns>
        public static Packet ToPacket(byte[] bytearray)
        {
            int Length = bytearray.Length;// Marshal.SizeOf(obj);
            IntPtr ptr = Marshal.AllocHGlobal(Length);
            Marshal.Copy(bytearray, 0, ptr, Length);
            Packet result = (Packet)Marshal.PtrToStructure(ptr, typeof(Packet));
            Marshal.FreeHGlobal(ptr);
            return result;
        }
        /// <summary>
        /// Deserialize item from base64 string.
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static Packet DeserializePacket(string base64)
        {
            try
            {
                return ToPacket(Convert.FromBase64String(base64));
            }
            catch
            {
                return new Packet();
            }
        }
    
        /// <summary>
        /// Deserialize Packet value from base64 string.
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static object DeserializeValue(string base64)
        {
            Packet p = (Packet)DeserializePacket(base64);
            if (p.IsEmpty)
                return null;
            return p.Data;
        }
        /// <summary>
        /// Serialize object to base64 string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            return Convert.ToBase64String(ToByteArray(obj));
        }
        /// <summary>
        /// Deserialize object from base64 string.
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static object Desrialize(string base64)
        {
            return ToStructure(Convert.FromBase64String(base64));
        }
        /// <summary>
        /// Serialize Object to byte array.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(object obj)
        {
            Packet p = new Packet(obj);
            int Length = Marshal.SizeOf(p);
            byte[] bytearray = new byte[Length];
            IntPtr ptr = Marshal.AllocHGlobal(Length);
            Marshal.StructureToPtr(p, ptr, false);
            Marshal.Copy(ptr, bytearray, 0, Length);
            Marshal.FreeHGlobal(ptr);
            return bytearray;
        }
        /// <summary>
        /// Deserialize object from byte array.
        /// </summary>
        /// <param name="bytearray"></param>
        /// <returns></returns>
        public static object ToStructure(byte[] bytearray)
        {
            int Length = bytearray.Length;// Marshal.SizeOf(obj);
            IntPtr ptr = Marshal.AllocHGlobal(Length);
            Marshal.Copy(bytearray, 0, ptr, Length);
            Packet result = (Packet)Marshal.PtrToStructure(ptr, typeof(Packet));
            Marshal.FreeHGlobal(ptr);
            return result.Data;
        }

    }

}
