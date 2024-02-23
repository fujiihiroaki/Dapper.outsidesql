#region copyright

// /*
//  * Copyright 2018-2024 Hiroaki Fujii  All rights reserved. 
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.Data.DB2.Core;
using Jiifureit.Dapper.OutsideSql;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Logger = Jiifureit.Dapper.OutsideSql.Log.Logger;

#endregion

namespace Dapper.OutsideSql.Test
{
    [TestClass]
    public class Db2Test
    {
        private const string CONNECTION_STRING = "Server=localhost;Database=s2demo;Uid=db2inst1;Pwd=Passw0rd123";
        private const string FILE_LOCATION = @"C:\projects\Dapper.outsidesql\Dapper.OutsideSql.Test";
        private readonly char DS = Path.DirectorySeparatorChar;
        
        private ILogger  _logger;
        
        [TestInitialize]
        public void TestSetup()
        {
            var path = $"{FILE_LOCATION}{DS}App1.config";
            
            LogManager.Setup().LoadConfigurationFromFile(path);
            Logger.Category = "Dapper.OutsideSql.Test.Db2Test";
            Logger.Factory.AddProvider(new NLogLoggerProvider());
            Logger.Factory.AddProvider(new DebugLoggerProvider());
            
            _logger = Logger.CreateLogger<MySqlTest>();
        }

        [TestCleanup]
        public void TestEnd()
        {
            LogManager.Shutdown();
        }

        [TestMethod]
        public void TestSelect1()
        {
            var filePath = FILE_LOCATION + DS + @"Select1Test.sql";
            using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start File Test ---");
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
            
            _logger.LogDebug("--- END File Test ---");

            using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                
                _logger.LogDebug("--- Start Stream Test ---");
                    
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var list = conn.QueryOutsideSql<Test1>(stream, Encoding.UTF8, new {sarary = 1500});
                    Assert.AreEqual(7, list.AsList().Count, "Test Count11");
                }

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var list = conn.QueryOutsideSql<Test1>(stream, Encoding.UTF8, new {jobnm = "CLERK"});
                    var enumerable = list.ToList();
                    Assert.AreEqual(4, enumerable.AsList().Count, "Test Count12");

                    var data = enumerable[1];
                    Assert.AreEqual(7876, data.EmpNo, "Entity Test11");
                    Assert.AreEqual("ADAMS", data.Ename, "Entity Test12");
                    Assert.AreEqual("CLERK", data.Job, "Entity Test13");
                    Assert.AreEqual("RESEARCH", data.DName, "Entity Test14");
                }

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var list = conn.QueryOutsideSql<Test1>(stream, Encoding.UTF8);
                    var enumerable = list.ToList();
                    Assert.AreEqual(14, enumerable.AsList().Count, "Test Count13");
                }
                    
                _logger.LogDebug("--- END Stream Test ---");
            }
            
            _logger.LogDebug("--- END ---");

        }

        [TestMethod]
        public void TestSelect2()
        {
            var filePath = FILE_LOCATION + DS + @"Select2Test.sql";
            using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");

                var list = conn.QueryOutsideSql<Test1>(filePath, new {sarary = 1500});
                Assert.AreEqual(7, list.AsList().Count, "Test Count21");

                string[] mgrnm = {"CLARK", "FORD"};

                list = conn.QueryOutsideSql<Test1>(filePath, new { sarary = 500, mgrnm });
                Assert.AreEqual(2, list.AsList().Count, "Test Count22");

                var mgnramList = new List<string> { "CLARK", "FORD" };

                list = conn.QueryOutsideSql<Test1>(filePath, new { sarary = 500, mgrnm = mgnramList });
                Assert.AreEqual(2, list.AsList().Count, "Test Count23");

                ICollection<string> mgList = new List<string> { "CLARK", "FORD" };
                list = conn.QueryOutsideSql<Test1>(filePath, new { sarary = 500, mgrnm = mgList });
                Assert.AreEqual(2, list.AsList().Count, "Test Count24");

                var param = new DynamicParameters();
                param.Add("sarary", 500);
                param.Add("mgrnm", mgnramList);
                list = conn.QueryOutsideSql<Test1>(filePath, param);
                Assert.AreEqual(2, list.AsList().Count, "Test Count25");
            }

            _logger.LogDebug("--- END ---");
        }

        [TestMethod]
        public void TestSelect3()
        {
            using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");

                var sql = "SELECT EMPNO EmpNo, JOB Job From EMP where EMP.SAL > /*sarary*/1000 order by EMPNO";

                var list = conn.QueryLog<Test1>(sql, new {sarary = 1500});
                Assert.AreEqual(7, list.AsList().Count, "Test Count31");

                var param = new DynamicParameters();
                param.Add("sarary", 1500);
                var test1 = conn.QueryFirstOrDefaultLog<Test1>(sql, param);
                Assert.AreEqual(7499, test1.EmpNo , "Test Count32");
            }

            _logger.LogDebug("--- END ---");
        }

        [TestMethod]
        public void TestCrud()
        {
            var filePath = FILE_LOCATION + DS + @"Crud1Test.sql";
            using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");

                IDbTransaction tran = conn.BeginTransaction();

                var ret = conn.ExecuteOutsideSql(filePath, new {newsarary = 2000, salary = 960, ts = DateTime.Now},
                    tran);
                Assert.AreEqual(2, ret, "Test Update3");

                tran.Rollback();
            }

            _logger.LogDebug("--- END ---");
        }

        [TestMethod]
        public void TestCrud2()
        {
            var filePath = FILE_LOCATION + DS + @"Crud2Test.sql";
            using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");

                IDbTransaction tran = conn.BeginTransaction();

                var ret = conn.ExecuteOutsideSql(filePath,
                    new {deptno = 50, nm = "SHOP", location = "TOKYO", active = 1}, tran);
                Assert.AreEqual(1, ret, "Test Insert4");

                tran.Rollback();
            }

            _logger.LogDebug("--- END ---");
        }

        [TestMethod]
        public void TestCrud3()
        {
            var filePath = FILE_LOCATION + DS + @"Crud3Test.sql";
            using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");

                IDbTransaction tran = conn.BeginTransaction();
                var sql = "insert into DEPT (DEPTNO, DNAME) values (/*DeptNo*/50, /*Dname*/'DEPT50')";
                var ret = conn.ExecuteLog(sql, new {DeptNo = 50, Dname= "DEPT50"}, tran);
                Assert.AreEqual(1, ret, "Test Delete1");
                
                ret = conn.ExecuteOutsideSql(filePath, new {no = 50}, tran);
                Assert.AreEqual(1, ret, "Test Delete2");

                tran.Commit();
            }

            _logger.LogDebug("--- END ---");
        }
        
        [TestMethod]
        public void TestExecuteMultiNoResults()
        {
            using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");
                
                IDbTransaction tran = conn.BeginTransaction();
                var sql = "insert into DEPT (DEPTNO, DNAME) values (/*DeptNo*/50, /*Dname*/'DEPT50')";
                var param = new[] { new { DeptNo = 90, Dname = "DEPT90" }, new { DeptNo = 91, Dname = "DEPT91" } };
                var ret = conn.ExecuteLog(sql, param, tran);
                Assert.AreEqual(2, ret, "Test ExecuteMultiNoResults against array");

                var param2 = new List<Foo>
                {
                    new(deptNo: 93, dname: "DEPT93"),
                    new(deptNo: 92, dname: "DEPT92"),
                    new(deptNo: 94, dname: "DEPT94")
                };
                var ret2 = conn.ExecuteLog(sql, param2, tran);
                Assert.AreEqual(3, ret2, "Test ExecuteMultiNoResults against List");
            }
            _logger.LogDebug("--- END ---");
        }

        [TestMethod]
        public async Task TestSelectAsync1()
        {
            var filePath = FILE_LOCATION + DS + @"Select1Test.sql";
            await using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start File Test ---");
                var list = await conn.QueryOutsideSqlAsync<Test1>(filePath, new {sarary = 1500});
                Assert.AreEqual(7, list.AsList().Count, "Test Count1");

                list = await conn.QueryOutsideSqlAsync<Test1>(filePath, new {jobnm = "CLERK"});
                var enumerable = list.ToList();
                Assert.AreEqual(4, enumerable.AsList().Count, "Test Count2");

                var data = enumerable[1];
                Assert.AreEqual(7876, data.EmpNo, "Entity Test1");
                Assert.AreEqual("ADAMS", data.Ename, "Entity Test2");
                Assert.AreEqual("CLERK", data.Job, "Entity Test3");
                Assert.AreEqual("RESEARCH", data.DName, "Entity Test4");

                list = await conn.QueryOutsideSqlAsync<Test1>(filePath);
                enumerable = list.ToList();
                Assert.AreEqual(14, enumerable.AsList().Count, "Test Count3");
            }
            
            _logger.LogDebug("--- END File Test ---");

            await using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                
                _logger.LogDebug("--- Start Stream Test ---");

                await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var list = await conn.QueryOutsideSqlAsync<Test1>(stream, Encoding.UTF8, new {sarary = 1500});
                    Assert.AreEqual(7, list.AsList().Count, "Test Count11");
                }

                await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var list = await conn.QueryOutsideSqlAsync<Test1>(stream, Encoding.UTF8, new {jobnm = "CLERK"});
                    var enumerable = list.ToList();
                    Assert.AreEqual(4, enumerable.AsList().Count, "Test Count12");

                    var data = enumerable[1];
                    Assert.AreEqual(7876, data.EmpNo, "Entity Test11");
                    Assert.AreEqual("ADAMS", data.Ename, "Entity Test12");
                    Assert.AreEqual("CLERK", data.Job, "Entity Test13");
                    Assert.AreEqual("RESEARCH", data.DName, "Entity Test14");
                }

                await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var list = await conn.QueryOutsideSqlAsync<Test1>(stream, Encoding.UTF8);
                    var enumerable = list.ToList();
                    Assert.AreEqual(14, enumerable.AsList().Count, "Test Count13");
                }
                    
                _logger.LogDebug("--- END Stream Test ---");
            }

            _logger.LogDebug("--- END ---");

        }

        [TestMethod]
        public async Task TestSelectAsync2()
        {
            var filePath = FILE_LOCATION + DS + @"Select2Test.sql";
            await using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");

                var list = await conn.QueryOutsideSqlAsync<Test1>(filePath, new {sarary = 1500});
                Assert.AreEqual(7, list.AsList().Count, "Test Count21");

                string[] mgrnm = {"CLARK", "FORD"};

                list =  await conn.QueryOutsideSqlAsync<Test1>(filePath, new { sarary = 500, mgrnm });
                Assert.AreEqual(2, list.AsList().Count, "Test Count22");

                var mgnramList = new List<string> { "CLARK", "FORD" };

                list = await conn.QueryOutsideSqlAsync<Test1>(filePath, new { sarary = 500, mgrnm = mgnramList });
                Assert.AreEqual(2, list.AsList().Count, "Test Count23");

                ICollection<string> mgList = new List<string> { "CLARK", "FORD" };
                list = await conn.QueryOutsideSqlAsync<Test1>(filePath, new { sarary = 500, mgrnm = mgList });
                Assert.AreEqual(2, list.AsList().Count, "Test Count24");

                var param = new DynamicParameters();
                param.Add("sarary", 500);
                param.Add("mgrnm", mgList);
                list = await conn.QueryOutsideSqlAsync<Test1>(filePath, param);
                Assert.AreEqual(2, list.AsList().Count, "Test Count25");
            }

            _logger.LogDebug("--- END ---");
        }

        [TestMethod]
        public async Task TestSelectAsync3()
        {
            await using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");

                var sql = "SELECT EMPNO EmpNo, JOB Job From EMP where EMP.SAL > /*sarary*/1000 order by EMPNO";

                var list = await conn.QueryLogAsync<Test1>(sql, new {sarary = 1500});
                Assert.AreEqual(7, list.AsList().Count, "Test Count31");

                var param = new DynamicParameters();
                param.Add("sarary", 1500);
                var test1 = await conn.QueryFirstOrDefaultLogAsync<Test1>(sql, param);
                Assert.AreEqual(7499, test1.EmpNo , "Test Count32");
            }

            _logger.LogDebug("--- END ---");
        }

        [TestMethod]
        public async Task TestCrudAsync1()
        {
            var filePath = FILE_LOCATION + DS + @"Crud1Test.sql";
            await using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");

                IDbTransaction tran = conn.BeginTransaction();

                var ret = await conn.ExecuteOutsideSqlAsync(filePath, new {newsarary = 2000, salary = 960, ts = DateTime.Now},
                    tran);
                Assert.AreEqual(2, ret, "Test Update3");

                tran.Rollback();
            }

            _logger.LogDebug("--- END ---");
        }

        [TestMethod]
        public async Task TestCrudAsync2()
        {
            var filePath = FILE_LOCATION + DS + @"Crud2Test.sql";
            await using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");

                IDbTransaction tran = conn.BeginTransaction();

                var ret = await conn.ExecuteOutsideSqlAsync(filePath,
                    new {deptno = 50, nm = "SHOP", location = "TOKYO", active = 1}, tran);
                Assert.AreEqual(1, ret, "Test Insert4");

                tran.Rollback();
            }

            _logger.LogDebug("--- END ---");
        }

        [TestMethod]
        public async Task TestCrudAsync3()
        {
            var filePath = FILE_LOCATION + DS + @"Crud3Test.sql";
            await using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");

                IDbTransaction tran = conn.BeginTransaction();
                var sql = "insert into DEPT (DEPTNO, DNAME) values (/*DeptNo*/50, /*Dname*/'DEPT50')";
                var ret = await conn.ExecuteLogAsync(sql, new {DeptNo = 50, Dname= "DEPT50"}, tran);
                Assert.AreEqual(1, ret, "Test Delete1");
                
                ret = await conn.ExecuteOutsideSqlAsync(filePath, new {no = 50}, tran);
                Assert.AreEqual(1, ret, "Test Delete2");

                tran.Commit();
            }

            _logger.LogDebug("--- END ---");
        }
        
        [TestMethod]
        public async Task TestExecuteMultiNoResultsAsync()
        {
            var filePath = FILE_LOCATION + DS + @"Crud2Test.sql";
            await using (var conn = new DB2Connection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.LogDebug("--- Start ---");
                
                IDbTransaction tran = await conn.BeginTransactionAsync();
                var param = new[] { 
                    new { deptno = 90, nm = "DEPT90", location = "NewYork", active = 1  }
                    , new { deptno = 91, nm = "DEPT91", location = "San Francisco", active = 0 } };
                var ret = await conn.ExecuteOutsideSqlAsync(filePath, param, tran);
                Assert.AreEqual(2, ret, "Test ExecuteMultiNoResults against array");

                var param2 = new List<Foo2>
                {
                    new(deptNo: 93, dname: "DEPT93", location: "NewYork", active: 1 ),
                    new(deptNo: 92, dname: "DEPT92", location: "NewYork", active: 0),
                    new(deptNo: 94, dname: "DEPT94", location: "San Francisco", active: 1)
                };
                var ret2 = await conn.ExecuteOutsideSqlAsync(filePath, param2, tran);
                Assert.AreEqual(3, ret2, "Test ExecuteMultiNoResults against List");
            }
            _logger.LogDebug("--- END ---");
        }
        
        private class Foo
        {
            public Foo(int deptNo, string dname)
            {
                DeptNo = deptNo;
                Dname = dname;
            }

            public int DeptNo { get; set; }
            public string Dname { get; set; }
        }
        
        private class Foo2
        {
            public Foo2(int deptNo, string dname, string location, int active)
            {
                deptno = deptNo;
                nm = dname;
                this.active = active;
                this.location = location;
            }

            public int deptno { get; set; }
            public string nm { get; set; }
            
            public int active { get; set; }
            
            public string location { get; set; }
        }
    }
}