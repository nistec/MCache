using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Nistec.Legacy
{

    #region IDalDB

    /// <summary>
    /// Interface that every dal class must inherit. When DBLayer creates 
    /// a dal class it uses <see cref="Common.DalDB">DalDB</see> class as its base class
    /// </summary>
    public interface IDalDB
    {
        /// <summary>
        /// Connection property
        /// </summary>
        IDbConnection Connection { get; set; }

        /// <summary>
        /// Transaction property
        /// </summary>
        IDbTransaction Transaction { get; set; }

        /// <summary>
        /// AutoCloseConnection property
        /// </summary>
        bool AutoCloseConnection { get; set; }

        ///// <summary>
        ///// Get DBProvider property
        ///// </summary>
        //DBProvider DBProvider { get; }
        ///// <summary>
        ///// DataSet representing the Schema of Database.
        ///// </summary>
        //DalSchema DBSchema { get;set;}
    }


    #endregion

    /// <summary>
    /// Parameter type enumeration for <see cref="DalPropertyAttribute.ParameterType"/> property
    /// of <see cref="DalPropertyAttribute"/> attribute.
    /// </summary>
    public enum DalPropertyType
    {
        /// <summary>
        /// The parameter is defaul and has not special role.
        /// </summary>
        Default,

        /// <summary>
        /// This parameter is a part of a table key 
        /// and is applicable only 
        /// </summary>
        Key,

        /// <summary>
        /// This parameter is a part of a table autoincremental field 
        /// and is applicable only 
        /// </summary>
        Identity,
        /// <summary>
        /// This parameter is not part of a table autoincremental field 
        /// and is applicable only 
        /// </summary>
        NA

    }

}
