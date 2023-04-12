using System;
using System.Collections; 
using System.Reflection;
using System.Data;

using System.Runtime.InteropServices;
//using Nistec.Data.Common;

namespace Nistec.Legacy
{
    #region Record
    /// <summary>
    /// Record struct
    /// </summary>
    public struct Record
    {
        /// <summary>
        /// Empty Record
        /// </summary>
        public static readonly Record Empty = new Record(null, null);

        //public Record() { }
        /// <summary>
        /// Record ctor
        /// </summary>
        /// <param name="displayMember"></param>
        /// <param name="valueMember"></param>
        public Record(object displayMember, object valueMember)
        {
            DisplayMember = displayMember;
            ValueMember = valueMember;
        }

        /// <summary>
        /// ValueMember
        /// </summary>
        public object ValueMember;
        /// <summary>
        /// DisplayMember
        /// </summary>
        public object DisplayMember;

        /// <summary>
        /// IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            get { return DisplayMember == null; }
        }
    }
    #endregion

    [StructLayout(LayoutKind.Sequential)]
    public struct Field
    {
        public readonly string Column;
        public readonly object Value;
        public Field(string column, object value)
        {
            this.Column = column;
            this.Value = value;
        }

        public bool Equals(Field field)
        {
            return field.Column.Equals(Column) && field.Value.Equals(Value);
        }

        public bool Equals(string column, object value)
        {
            return column.Equals(Column) && value.Equals(Value);
        }

     }

  
}
