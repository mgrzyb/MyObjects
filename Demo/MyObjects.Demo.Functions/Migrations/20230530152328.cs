using System;
using System.Data.Common;
using FluentMigrator;
using NHibernate;

namespace MyObjects.Demo.Migrations
{
    [Migration(20230530152328)]
    public class Migration_20230530152328: Migration
    {
        public override void Up()
        {
            this.Execute.Script("Migrations/20230530152328.sql");
        }
		
        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
