#region copyright

// /*
//  * Copyright 2018-2018 Hiroaki Fujii  All rights reserved. 
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *     http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
//  * either express or implied. See the License for the specific language
//  * governing permissions and limitations under the License.
//  */

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Jiifureit.Dapper.OutsideSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;

#endregion

namespace Dapper.OutsideSql.Test
{
    [TestClass]
    public class SqlServerTest
    {
        private const string CONNECTION_STRING =
            "Data Source=localhost;Initial Catalog=s2dotnetdemo;Persist Security Info=True;User ID=sa;Password=P@ssw0rd123";

        private const string FILE_LOCATION = @"C:\projects\Dapper.outsidesql\Dapper.OutsideSql.Test";

        private readonly Logger _logger
            = LogManager.LoadConfiguration(FILE_LOCATION + @"\App1.config").GetCurrentClassLogger();

        [TestMethod]
        public void TestSelect1()
        {
            var filePath = FILE_LOCATION + @"\Select1Test.sql";
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.Debug("--- Start ---");
                var list = conn.QueryOutsideSql<Test1>(filePath, new {sarary = 1500});
                Assert.AreEqual(7, list.AsList().Count, "Test Count1");

                list = conn.QueryOutsideSql<Test1>(filePath, new {jobnm = "CLERK"});
                var enumerable = list.ToList();
                Assert.AreEqual(4, enumerable.AsList().Count, "Test Count2");

                var data = enumerable[1];
                Assert.AreEqual(7876, data.EmpNo, "Entity Test1");
                Assert.AreEqual("ADAMS", data.Ename, "Entity Test2");
                Assert.AreEqual("CLERK", data.Job, "Entity Test3");
                Assert.AreEqual("RESEARCH", data.DName, "Entity Test4");

                list = conn.QueryOutsideSql<Test1>(filePath);
                enumerable = list.ToList();
                Assert.AreEqual(14, enumerable.AsList().Count, "Test Count3");
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

                var list = conn.QueryOutsideSql<Test1>(filePath, new {sarary = 1500});
                Assert.AreEqual(7, list.AsList().Count, "Test Count21");

                string[] mgrnm = {"CLARK", "FORD"};

                list = conn.QueryOutsideSql<Test1>(filePath, new {sarary = 500, mgrnm});
                Assert.AreEqual(2, list.AsList().Count, "Test Count22");

                var mgnramList = new List<string> { "CLARK", "FORD" };

                list = conn.QueryOutsideSql<Test1>(filePath, new { sarary = 500, mgrnm = mgnramList });
                Assert.AreEqual(2, list.AsList().Count, "Test Count23");

                ICollection<string> mgList = new List<string> {"CLARK", "FORD"};
                list = conn.QueryOutsideSql<Test1>(filePath, new { sarary = 500, mgrnm = mgList });
                Assert.AreEqual(2, list.AsList().Count, "Test Count24");

                var param = new DynamicParameters();
                param.Add("sarary", 500);
                param.Add("mgrnm", mgnramList);
                list = conn.QueryOutsideSql<Test1>(filePath, param);
                Assert.AreEqual(2, list.AsList().Count, "Test Count25");
            }

            _logger.Debug("--- END ---");
        }

        [TestMethod]
        public void TestCrud()
        {
            var filePath = FILE_LOCATION + @"\Crud1Test.sql";
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.Debug("--- Start ---");

                IDbTransaction tran = conn.BeginTransaction();

                var ret = conn.ExecuteOutsideSql(filePath, new {newsarary = 2000, salary = 960, ts = DateTime.Now},
                    tran);
                Assert.AreEqual(2, ret, "Test Update3");

                tran.Commit();
            }
            _logger.Debug("--- END ---");
        }

        [TestMethod]
        public void TestCrud2()
        {
            var filePath = FILE_LOCATION + @"\Crud2Test.sql";
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.Debug("--- Start ---");

                IDbTransaction tran = conn.BeginTransaction();

                var ret = conn.ExecuteOutsideSql(filePath, new {deptno = 50, nm = "SHOP", location = "TOKYO", active  = 1 }, tran);
                Assert.AreEqual(1, ret, "Test Insert4");

                tran.Commit();
            }
            _logger.Debug("--- END ---");
        }

        [TestMethod]
        public void TestCrud3()
        {
            var filePath = FILE_LOCATION + @"\Crud3Test.sql";
            using (var conn = new SqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.Debug("--- Start ---");

                IDbTransaction tran = conn.BeginTransaction();
                var ret = conn.ExecuteOutsideSql(filePath, new { no = 50 }, tran);
                Assert.AreEqual(1, ret, "Test Insert4");

                tran.Commit();
            }
            _logger.Debug("--- END ---");
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