using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nistec.Data.Entities;
using Nistec.Data.Factory;
using Nistec.Data;
using Nistec.Caching.Demo.DB;

namespace Nistec.Caching.Demo.Entities
{

    [Serializable]
    [Entity(EntityName = "Contact", MappingName = "Person.Contact", ConnectionKey = "AdventureWorks", EntityKey = new string[] { "ContactID" })]
    public class ContactEntityContext : EntityContext<ContactEntity>
    {

        #region ctor

        public ContactEntityContext()
        {
        }

        public ContactEntityContext(int id)
            : base(id)
        {
        }

        public ContactEntityContext(Guid row)
        {
            base.Init(new DataFilter("rowguid=@rowguid",row));
        }



        public static DataTable GetList()
        {
            DataTable dt = null;
            using (IDbCmd cmd = AdventureWorks.Instance.NewCmd())
            {
                dt = cmd.ExecuteCommand<DataTable>("select top 10 * from Person.Contact", true);
            }

            return dt;
        }

        #endregion

        #region binding

        protected override void EntityBind()
        {
            base.EntityDb.EntityCulture = AdventureWorksResources.GetCulture();
            //If EntityAttribute not define you can initilaize the entity here
            //base.InitEntity<AdventureWorks>("Contact", "Person.Contact", EntityKeys.Get("ContactID"));
        }

        #endregion

        #region methods

        public void Test()
        {
            string str = base.EntityDb.DoCommand<string>("select EmailAddress from Person.Contact where ContactID=@ContactID", new DataParameter[] { new DataParameter("ContactID", 2) }, CommandType.Text);
            Console.WriteLine(str);

        }
        #endregion
    }

}
