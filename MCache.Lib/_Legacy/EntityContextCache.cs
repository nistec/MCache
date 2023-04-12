using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using Nistec.Generic;
using Nistec.Data.Entities;

namespace Nistec.Legacy
{
   
    /// <summary>
    /// Represent Entity Context Cache wrapper that contains fast search mechanizem.
    /// </summary>
    public class EntityContextCache<T> : EntityCache<string, T>
    {
        public EntityContextCache(EntityKeys info)
            : base(1)
        {
            base.DataKeys.AddRange(info);
            InitCache();
        }

       
        public EntityContextCache()
            : base(1)
        {
            T instance = Activator.CreateInstance<T>();

            EntityAttribute keyattr = AttributeProvider.GetCustomAttribute<EntityAttribute>(instance.GetType());
            string[] keys = keyattr.EntityKey;
            base.DataKeys.AddRange(keys);
            IDictionary dt = ((IEntity<T>)instance).EntityDictionary();
         }
         
        protected override void InitCache()
        {
            T instance = Activator.CreateInstance<T>();
            IDictionary dt = ((IEntity<T>)instance).EntityDictionary();
            base.CreateCache(dt);
        }

    }
}
