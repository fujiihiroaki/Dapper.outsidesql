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

using System.IO;
using Hnx8.ReadJEnc;
using Jiifureit.Dapper.OutsideSql;
using Jiifureit.Dapper.OutsideSql.Impl;
using Jiifureit.Dapper.OutsideSql.SqlParser;
using Jiifureit.Dapper.OutsideSql.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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