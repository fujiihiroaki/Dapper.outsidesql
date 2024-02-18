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

using System.Collections.Generic;
using Jiifureit.Dapper.OutsideSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using Oracle.ManagedDataAccess.Client;

namespace Dapper.OutsideSql.Test
{
    [TestClass]
    public class DapperLogTest
    {
        private const string CONNECTION_STRING = "Data Source=localhost:1521/freepdb1;User Id=s2dotnetdemo;Password=s2dotnetdemo";
        private const string FILE_LOCATION = @"C:\projects\Dapper.outsidesql\Dapper.OutsideSql.Test";

        private readonly Logger _logger
            = LogManager.LoadConfiguration(FILE_LOCATION + @"\App1.config").GetCurrentClassLogger();


        [TestMethod]
        public void TestSelect1()
        {
            using (var conn = new OracleConnection(CONNECTION_STRING))
            {
                conn.Open();
                _logger.Debug("--- Start ---");

                var sql = "select EMP.EMPNO EmpNo,EMP.ENAME Enam from EMP where EMPNO >= /*Empno1*/500 and EMPNO <= /*Empno2*/1000";
                var list = conn.QueryLog<Test1>(sql, new { Empno1 = 7900, Empno2 = 7940 });
                Assert.AreEqual(3, list.AsList().Count, "Test Count1");

                sql = "select EMP.EMPNO EmpNo,EMP.ENAME Enam from EMP where ENAME IN /*mgrnm*/('SMITH')";
                var mgnramList = new List<string> { "CLARK", "FORD" };

                list = conn.QueryLog<Test1>(sql, new {mgrnm = mgnramList });
                Assert.AreEqual(2, list.AsList().Count, "Test Count2");


                sql = "select EMP.EMPNO EmpNo,EMP.ENAME Enam from EMP";
                list = conn.QueryLog<Test1>(sql);
                Assert.AreEqual(14, list.AsList().Count, "Test Count3");

            }
        }
    }
}