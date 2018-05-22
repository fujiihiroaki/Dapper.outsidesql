using System;
using System.IO;
using Hnx8.ReadJEnc;
using Jiifureit.Dapper.OutsideSql.SqlParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Seasar.Dao;
using Seasar.Dao.Context;

namespace Dapper.OutsideSql.Test
{
    [TestClass]
    public class DapperOutsideSqlTest
    {
        [TestMethod]
        public void TestSqlParser()
        {
            var filePath = @"C:\projects\Dapper.outsidesql\Dapper.OutsideSql.Test\ParserTest.sql";

            var sql = "";
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

            ICommandContext ctx = new CommandContextImpl();
            rootNode.Accept(ctx);

            var builder = new Jiifureit.Dapper.OutsideSql.SqlBuilder.SqlBuilder(rootNode, ctx.Sql);

            Assert.IsNotNull(rootNode);
        }
    }
}
