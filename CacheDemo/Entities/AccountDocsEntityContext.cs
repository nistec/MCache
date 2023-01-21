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
    [Entity(EntityName = "[Accounts]", MappingName = "Accounts", ConnectionKey = "Netcell_Docs", EntityKey = new string[] { "AccountId" })]
    public class AccountDocsEntityContext : EntityContext<AccountEntity>
    {

        #region ctor

        public AccountDocsEntityContext()
        {
        }

        public AccountDocsEntityContext(int id)
            : base(id)
        {
        }

        public AccountDocsEntityContext(Guid row)
        {
            base.Init(new DataFilter("rowguid=@rowguid",row));
        }



        public static DataTable GetList()
        {
            DataTable dt = null;
            using (var db = new Netcell_Docs())
            {
                using (IDbCmd cmd = db.NewCmd())
                {
                    dt = cmd.ExecuteCommand<DataTable>("select top 10 * from Accounts", true);
                }
            }

            return dt;
        }

        #endregion

        #region binding

        protected override void EntityBind()
        {
            base.EntityDb.EntityCulture = NetcellResources.GetCulture();
            //If EntityAttribute not define you can initilaize the entity here
            //base.InitEntity<Netcell_Docs>("Contact", "Accounts", EntityKeys.Get("AccountId"));
        }

        #endregion

        #region methods

        public void Test()
        {
            string str = base.EntityDb.DoCommand<string>("select Email from Accounts where AccountId=@AccountId", new DataParameter[] { new DataParameter("AccountId", 2) }, CommandType.Text);
            Console.WriteLine(str);

        }
        #endregion
    }

    [Serializable]
    public class AccountEntity : IEntityItem
    {
        #region properties

        [EntityProperty(EntityPropertyType.Identity, Caption = "AccountId")]
        public int AccountId
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Column = "AccountName", Caption = "AccountName")]
        public string AccountName
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Column = "Email", Caption = "Email")]
        public string Email
        {
            get;
            set;
        }


        [EntityProperty(EntityPropertyType.Default, false, Column = "Mobile", Caption = "Mobile")]
        public string Mobile
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Caption = "OwnerId")]
        public int OwnerId
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Column = "City", Caption = "City")]
        public string City
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Caption = "LastUpdate")]
        public DateTime LastUpdate
        {
            get;
            set;
        }



        #endregion

        #region override

        public override string ToString()
        {
            return string.Format("AccountName:{0},Email:{1},Mobile:{2}", AccountName, Email, Mobile);
        }
        #endregion
    }

}
