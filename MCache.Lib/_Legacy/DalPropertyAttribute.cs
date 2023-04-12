using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

using Debug = System.Diagnostics.Debug;

namespace Nistec.Legacy
{

	#region DalPropertyAttribute

	/// <summary>
	/// This attribute defines properties of method's parameters
	/// </summary>
	[ AttributeUsage(AttributeTargets.Property) ]
	public class DalPropertyAttribute : Attribute
	{
		/// <summary>
		///  Null Value Return
		/// </summary>
		public const string NullValueToken = "NullValue";

		#region Private members
		private string m_name = "";
        private const DbType ParamTypeNotDefinedValue = (DbType)1000000;
		private DbType m_sqlDbType = ParamTypeNotDefinedValue;
		private int m_size = 0;
		private byte m_precision = 0;
		private byte m_scale = 0;
		private object m_AsNull = NullValueToken;
        //private object m_defaultValue = null;
        private bool m_allowNull = true;
        private DalPropertyType m_parameterType = DalPropertyType.Default;
		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class
		/// </summary>
		public DalPropertyAttribute() {}

		/// <summary>
        /// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class
		/// </summary>
		/// <param name="name">Is a value of <see cref="Name"/> property</param>
        public DalPropertyAttribute(string name)
		{
			Name = name;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class
		/// </summary>
        /// <param name="size">Is a value of <see cref="Size"/> property</param>
        public DalPropertyAttribute(int size)
		{
			Size = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class
		/// </summary>
		/// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        public DalPropertyAttribute(DalPropertyType parameterType)
		{
			ParameterType = parameterType;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class
		/// </summary>
		/// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        /// <param name="size">Is a value of <see cref="Size"/> property</param>
        public DalPropertyAttribute(DalPropertyType parameterType, int size)
		{
			ParameterType = parameterType;
            Size = size;
		}
        /// <summary>
        /// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class
        /// </summary>
        /// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        /// <param name="allowNull">Is a value of <see cref="AllowNull"/> property</param>
        public DalPropertyAttribute(DalPropertyType parameterType, bool allowNull)
        {
            ParameterType = parameterType;
            m_allowNull = allowNull;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class
        /// </summary>
        /// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        /// <param name="size">Is a value of <see cref="Size"/> property</param>
        /// <param name="allowNull">Is a value of <see cref="AllowNull"/> property</param>
        public DalPropertyAttribute(DalPropertyType parameterType, int size, bool allowNull)
        {
            ParameterType = parameterType;
            Size = size;
            m_allowNull = allowNull;
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class
        ///// </summary>
        ///// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        ///// <param name="size">Is a value of <see cref="Size"/> property</param>
        ///// <param name="defaultValue">Is a value of <see cref="DefaultValue"/> property</param>
        //public DalPropertyAttribute(DalPropertyType parameterType, int size, object defaultValue)
        //{
        //    ParameterType = parameterType;
        //    m_defaultValue = defaultValue;
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class
        ///// </summary>
        ///// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        ///// <param name="defaultValue">Is a value of <see cref="DefaultValue"/> property</param>
        //public DalPropertyAttribute(DalPropertyType parameterType, object defaultValue)
        //{
        //    ParameterType = parameterType;
        //    m_defaultValue = defaultValue;
        //}

		/// <summary>
		/// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class
		/// </summary>
		/// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
		/// <param name="name">Is a value of <see cref="Name"/> property</param>
        public DalPropertyAttribute(DalPropertyType parameterType, string name)
		{
			ParameterType = parameterType;
			m_name = name;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class
		/// </summary>
		/// <param name="name">Is a value of <see cref="Name"/> property</param>
        /// <param name="size">Is a value of <see cref="Size"/> property</param>
        public DalPropertyAttribute(string name, int size)
		{
			Name = name;
            Size = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DalPropertyAttribute"/> class with the specified
		/// <see cref="Name"/>, <see cref="SqlDbType"/>, <see cref="Size"/>, <see cref="Precision"/>, 
		/// <see cref="Scale"/>, <see cref="AsNull"/> and <see cref="ParameterType"/> values
		/// </summary>
		/// <param name="name">Is a value of <see cref="Name"/> property</param>
        /// <param name="allowNull">Is a value of <see cref="AllowNull"/> property</param>
        /// <param name="sqlDbType">Is a value of <see cref="DbType"/> property</param>
		/// <param name="size">Is a value of <see cref="Size"/> property</param>
		/// <param name="precision">Is a value of <see cref="Precision"/> property</param>
		/// <param name="scale">Is a value of <see cref="Scale"/> property</param>
		/// <param name="AsNull">Is a value of <see cref="AsNull"/> property</param>
		/// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        public DalPropertyAttribute(string name, bool allowNull, DbType sqlDbType, int size, byte precision, byte scale, object asNull, DalPropertyType parameterType)
		{
			Name = name;
            //Caption = caption;
            AllowNull=allowNull;
            //DefaultValue=defaultValue;
			SqlDbType = sqlDbType;
			Size = size;
			Precision = precision;
			Scale = scale;
            m_AsNull = asNull;
			ParameterType = parameterType;
		}

		/// <summary>
		/// An attribute builder method
		/// </summary>
		/// <param name="attr"></param>
		/// <returns></returns>
        public static CustomAttributeBuilder GetAttributeBuilder(DalPropertyAttribute attr)
		{
			string name = attr.m_name;
            Type[] arrParamTypes = new Type[] { typeof(string),typeof(bool),typeof(object), typeof(DbType), typeof(int), typeof(byte), typeof(byte), typeof(object), typeof(DalPropertyType) };
			object[] arrParamValues = new object[] {name,attr.m_allowNull, attr.m_sqlDbType, attr.m_size, attr.m_precision, attr.m_scale, attr.m_AsNull, attr.m_parameterType};
            ConstructorInfo ctor = typeof(DalPropertyAttribute).GetConstructor(arrParamTypes);
			return new CustomAttributeBuilder(ctor, arrParamValues);
		}
		#endregion

		#region Properties

		/// <summary>
		/// Sql parameter name. If this property is not defined 
		/// then a method parameter name is used.
		/// </summary>
		public string Name
		{
			get { return m_name == null ? string.Empty : m_name; }
			set { m_name = value; }
		}
        ///// <summary>
        ///// Parameter caption. usefull for UI validation
        ///// </summary>
        //public string Caption
        //{
        //    get { return m_caption == null ? Name : m_caption; }
        //    set { m_caption = value; }
        //}
		/// <summary>
		/// Sql parameter size. 
		/// It is strongly recomended to define this property for string parameters
		/// so that they could be trimmed to the size specified.
		/// </summary>
		public int Size
		{
			get { return m_size; }
			set { m_size = value; }
		}

		/// <summary>
		/// Sql parameter precision. It has not sense for non-numeric parameters.
		/// </summary>
		public byte Precision
		{
			get { return m_precision; }
			set { m_precision = value; }
		}

		/// <summary>
		/// Sql parameter scale. It has not sense for non-numeric parameters.
		/// </summary>
		public byte Scale
		{
			get { return m_scale; }
			set { m_scale = value; }
		}

		/// <summary>
		/// This parameter contains a value that will be interpreted as null. 
		/// This is usefull if you want to pass a null to a value type parameter.
		/// </summary>
		public object AsNull
		{
			get { return m_AsNull; }
			set { m_AsNull = value; }
		}

        ///// <summary>
        ///// This parameter contains a value that will be interpreted when value is null. 
        ///// </summary>
        //public object DefaultValue
        //{
        //    get { return m_defaultValue; }
        //    set { m_defaultValue = value; }
        //}

		/// <summary>
		/// Parameter Type
		/// </summary>
        public DalPropertyType ParameterType
		{
			get { return m_parameterType; }
			set { m_parameterType = value; }
		}

		/// <summary>
		/// Sql parameter type. 
		/// If not defined then method parameter type is converted to <see cref="DbType"/> type
		/// </summary>
		public DbType SqlDbType
		{
			get 
			{ 
				return m_sqlDbType; 
			}

			set 
			{ 
				m_sqlDbType = value; 
			}
		}

        /// <summary>
        /// Indicate if parameter allow null value.
        /// </summary>
        public bool AllowNull
        {
            get { return m_allowNull; }
            set { m_allowNull = value; }
        }

		/// <summary>
		/// Is Name Defined
		/// </summary>
        public bool IsNameDefined
		{
			get { return m_name != null && m_name.Length != 0; }
		}

		/// <summary>
		/// Is Size Defined
		/// </summary>
        public bool IsSizeDefined
		{
			get { return m_size > 0; }
		}

		/// <summary>
		/// Is Type Defined
		/// </summary>
        public bool IsTypeDefined
		{
			get { return m_sqlDbType != ParamTypeNotDefinedValue; }
		}

		/// <summary>
		/// Is Scale Defined
		/// </summary>
		internal bool IsScaleDefined
		{
			get { return m_scale > 0; }
		}

		/// <summary>
		/// Is Precision Defined
		/// </summary>
		internal bool IsPrecisionDefined
		{
			get { return m_precision > 0; }
		}
  		#endregion

	}

	#endregion

}

