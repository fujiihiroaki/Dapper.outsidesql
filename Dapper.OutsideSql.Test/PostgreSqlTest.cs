﻿#region copyright

// /* 
// *  Copyright (c) 2018-2018  Hiroaki Fujii All rights reserved. Licensed under the MIT license. 
// *  See LICENSE in the source repository root for complete license information. 
// */

#endregion

#region using

using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;

#endregion

namespace Dapper.OutsideSql.Test
{
    /// <summary>
    ///     PostgreSQL Test
    /// </summary>
    [TestClass]
    public class PostgreSqlTest
    {
        private const string CONNECTION_STRING = "Server=localhost;Port=5432;Database=s2demo;User id=s2demouser;Password=P@ssw0rd123";
        private const string FILE_LOCATION = @"C:\projects\Dapper.outsidesql\Dapper.OutsideSql.Test";

        [TestMethod]
        public void TestSelect1()
        {
            var filePath = FILE_LOCATION + @"\Select1Test.sql";
            using (var conn = new NpgsqlConnection(CONNECTION_STRING))
            {
            }
        }

        [TestMethod]
        public void TestSelect2()
        {
            var filePath = FILE_LOCATION + @"\Select2Test.sql";
            using (var conn = new NpgsqlConnection(CONNECTION_STRING))
            {
            }
        }

        [TestMethod]
        public void TestCrud()
        {
            var filePath = FILE_LOCATION + @"\Crud1Test.sql";
            IDbTransaction tran = null;
            using (var conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                tran = conn.BeginTransaction();
            }
        }

        [TestMethod]
        public void TestCrud2()
        {
            var filePath = FILE_LOCATION + @"\Crud2Test.sql";
            IDbTransaction tran = null;
            using (var conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                tran = conn.BeginTransaction();
            }
        }

        [TestMethod]
        public void TestCrud3()
        {
            var filePath = FILE_LOCATION + @"\Crud3Test.sql";
            IDbTransaction tran = null;
            using (var conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                tran = conn.BeginTransaction();
            }
        }
    }
}