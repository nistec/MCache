using Nistec.Data.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nistec.Legacy
{
    public static class CacheDataExtention
    {
       
            static T[] GetCustomAttributes<T>(this ICustomAttributeProvider provider) where T : Attribute
            {
                return GetCustomAttributes<T>(provider, true);
            }

            static T[] GetCustomAttributes<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
            {
                if (provider == null)
                    throw new ArgumentNullException("provider");
                T[] attributes = provider.GetCustomAttributes(typeof(T), inherit) as T[];
                if (attributes == null)
                {
                    // WORKAROUND: Due to a bug in the code for retrieving attributes            
                    // from a dynamic generated parameter, GetCustomAttributes can return            
                    // an instance of an object[] instead of T[], and hence the cast above            
                    // will return null.            
                    return new T[0];
                }
                return attributes;
            }
        

        public static EntityDb GetEntityDb<Dbe>(CultureInfo culture) where Dbe : IEntity
        {
            EntityDb db = null;

            EntityAttribute[] attributes = typeof(Dbe).GetCustomAttributes<EntityAttribute>().ToArray();
            if (attributes == null || attributes.Length == 0)
                return db;
            var attribute = attributes[0];
            db = new EntityDb(attribute.ConnectionKey, attribute.EntityName, attribute.MappingName, attribute.EntitySourceType, EntityKeys.Get(attribute.EntityKey));
            db.EntityCulture = culture;
            //db.EntityCommandType = attribute.CommandType;
            return db;
        }
    }
}
