#region copyright

// /* 
// *  Copyright (c) 2018-2018  Hiroaki Fujii All rights reserved. Licensed under the MIT license. 
// *  See LICENSE in the source repository root for complete license information. 
// */

#endregion

#region using

using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Jiifureit.Dapper.OutsideSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Dapper.OutsideSql.Test
{    
    [TestClass]
    public class SqlServerTest
    {
        private readonly NLog.Logger _logger 
            = NLog.LogManager.LoadConfiguration(FILE_LOCATION + @"\App1.config").GetCurrentClassLogger();

        private const string CONNECTION_STRING =
            "Data Source=localhost;Initial Catalog=s2dotnetdemo;Persist Security Info=True;User ID=sa;Password=P@ssw0rd123";

        private const string FILE_LOCATION = @"C:\projects\Dapper.outsidesql\Dapper.OutsideSql.Test";

        [TestMethod]
        public void TestSelect1()
        {
            var filePath = FILE_LOCATION + @"\Select1Test.sql";
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.Debug("--- Start ---");
                var list = conn.QueryOutsideSql<Test1>(filePath, new { sarary = 1500});
                Assert.AreEqual(7, list.AsList().Count, "Test Count1");

                list = conn.QueryOutsideSql<Test1>(filePath, new { jobnm = "CLERK" });
                var enumerable = list.ToList();
                Assert.AreEqual(4, enumerable.AsList().Count, "Test Count2");

                var data = enumerable[1];
                Assert.AreEqual(7876, data.EmpNo, "Entity Test1");
                Assert.AreEqual("ADAMS", data.Ename, "Entity Test2");
                Assert.AreEqual("CLERK", data.Job, "Entity Test3");
                Assert.AreEqual("RESEARCH", data.DName, "Entity Test4");
            }
            _logger.Debug("--- END ---");
        }

        [TestMethod]
        public void TestSelect2()
        {
            var filePath = FILE_LOCATION + @"\Select2Test.sql";
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.Debug("--- Start ---");
            }
            _logger.Debug("--- END ---");
        }

        [TestMethod]
        public void TestCrud()
        {
            var filePath = FILE_LOCATION + @"\Crud1Test.sql";
            IDbTransaction tran = null;
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                tran = conn.BeginTransaction();
            }
        }

        [TestMethod]
        public void TestCrud2()
        {
            var filePath = FILE_LOCATION + @"\Crud2Test.sql";
            IDbTransaction tran = null;
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                tran = conn.BeginTransaction();
            }
        }

        [TestMethod]
        public void TestCrud3()
        {
            var filePath = FILE_LOCATION + @"\Crud3Test.sql";
            IDbTransaction tran = null;
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                tran = conn.BeginTransaction();
            }
        }
    }

    public class Test1
    {
        public int EmpNo { get; set; }
        public string Ename { get; set; }
        public string Job { get; set; }
        public string DName { get; set; }
    }
}