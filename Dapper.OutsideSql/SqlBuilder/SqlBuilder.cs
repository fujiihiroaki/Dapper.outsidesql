// /* 
// *  Copyright (c) 2018-2018  Hiroaki Fujii All rights reserved. Licensed under the MIT license. 
// *  See LICENSE in the source repository root for complete license information. 
// */

using Dapper;
using NLog;
using Seasar.Dao;

namespace Jiifureit.Dapper.OutsideSql.SqlBuilder
{
    public class SqlBuilder
    {
        private readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private INode _node;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public SqlBuilder(INode node, string sql)
        {
            _node = node;
            Sql = sql;
        }

        public string Sql { get; }

        public string Build(object parameters)
        {
            if (parameters == null)
            {
                _logger.Debug(Sql);
                return Sql;
            }
            else
            {
                var sql = "";
                var p = new DynamicParameters(parameters);
                p.ParameterNames.AsList().ForEach(s =>
                {

                });

                return sql;
            }
        }
    }
}