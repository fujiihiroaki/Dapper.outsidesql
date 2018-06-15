#region copyright

// /* 
// *  Copyright (c) 2018-2018  Hiroaki Fujii All rights reserved. Licensed under the MIT license. 
// *  See LICENSE in the source repository root for complete license information. 
// */

#endregion

#region using

using System.IO;
using Hnx8.ReadJEnc;
using Jiifureit.Dapper.OutsideSql.Impl;
using Jiifureit.Dapper.OutsideSql.SqlParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seasar.Dao;
using Seasar.Framework.Util;

#endregion

namespace Dapper.OutsideSql.Test
{
    [TestClass]
    public class DapperOutsideSqlTest
    {
        [TestMethod]
        public void TestSqlParser()
        {
            var filePath = @"C:\projects\Dapper.outsidesql\Dapper.OutsideSql.Test\ParserTest.sql";

            string sql;
            var fileInfo = new FileInfo(filePath);
            using (var fileReader = new FileReader(fileInfo))
            {
                var charcode = fileReader.Read(fileInfo);

                using (TextReader reader = new StreamReader(filePath, charcode.GetEncoding()))
                {
                    sql = reader.ReadToEnd();
                }
            }

            var parser = new Parser(sql);
            var rootNode = parser.Parse();

            ICommandContext ctx = new CommandContextImpl(BindVariableType.QuestionWithParam);

            rootNode.Accept(ctx);

            Assert.IsNotNull(rootNode, "node is not null");
            Assert.IsTrue(sql.Length > 0, "sql string");
        }
    }
}