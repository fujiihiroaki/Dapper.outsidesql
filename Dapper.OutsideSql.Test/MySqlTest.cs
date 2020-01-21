#region copyright

// /*
//  * Copyright 2018-2020 Hiroaki Fujii  All rights reserved. 
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
using Jiifureit.Dapper.OutsideSql;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using NLog;
using NLog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Logger = Jiifureit.Dapper.OutsideSql.Log.Logger;

#endregion

namespace Dapper.OutsideSql.Test
{
    /// <summary>
    ///     MySQL Test
    /// </summary>
    [TestClass]
    public class MySqlTest
    {
        private const string CONNECTION_STRING = "Server=localhost;Database=s2dotnetdemo;Uid=mysql;Pwd=mysql";
        private const string FILE_LOCATION = @"C:\projects\Dapper.outsidesql\Dapper.OutsideSql.Test";
        private readonly char DS = Path.DirectorySeparatorChar;
        
        private ILogger  _logger;
        
        [TestInitialize]
        public void TestSetup()
        {
            var path = $"{FILE_LOCATION}{DS}App1.config";
            
            LogManager.LoadConfiguration(path);
            Logger.Category = "Dapper.OutsideSql.Test.MySqlTest";
            Logger.Factory.AddProvider(new NLogLoggerProvider());
            
            var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>(Logger.Category, null);
            var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new []{ configureNamedOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
            var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());
            Logger.Factory.AddProvider(new ConsoleLoggerProvider(optionsMonitor));

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
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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

            using (var conn = new MySqlConnection(CONNECTION_STRING))
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
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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
                param.Add("mgrnm", mgList);
                list = conn.QueryOutsideSql<Test1>(filePath, param);
                Assert.AreEqual(2, list.AsList().Count, "Test Count25");
            }

            _logger.LogDebug("--- END ---");
        }

        [TestMethod]
        public void TestSelect3()
        {
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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
        public async Task TestSelectAsync1()
        {
            var filePath = FILE_LOCATION + DS + @"Select1Test.sql";
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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

            using (var conn = new MySqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                
                _logger.LogDebug("--- Start Stream Test ---");
                    
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var list = await conn.QueryOutsideSqlAsync<Test1>(stream, Encoding.UTF8, new {sarary = 1500});
                    Assert.AreEqual(7, list.AsList().Count, "Test Count11");
                }

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
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

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
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
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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
            using (var conn = new MySqlConnection(CONNECTION_STRING))
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
    }
}