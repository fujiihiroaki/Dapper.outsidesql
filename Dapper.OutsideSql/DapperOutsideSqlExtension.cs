// /* 
// *  Copyright (c) 2018-2018  Hiroaki Fujii All rights reserved. Licensed under the MIT license. 
// *  See LICENSE in the source repository root for complete license information. 
// */

#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Dapper;
using Hnx8.ReadJEnc;
using Jiifureit.Dapper.OutsideSql.SqlParser;
using Jiifureit.Dapper.OutsideSql.Utility;
using NLog;
using Seasar.Dao;
using Seasar.Dao.Context;
using Seasar.Framework.Util;

#endregion

namespace Jiifureit.Dapper.OutsideSql
{
    /// <summary>
    ///     Dapper Extension for using Outside SQL file.
    /// </summary>
    public static class DapperOutsideSqlExtension
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly ConcurrentDictionary<string, string> sqlDictionary =
            new ConcurrentDictionary<string, string>();

        /// <summary>
        ///     Execute parameterized SQL.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteOutsideSql(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.Execute(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="filepath">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell selected as <see cref="object" />.</returns>
        public static object ExecuteScalarOutsideSql(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.ExecuteScalar(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="filepath">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell returned, as <typeparamref name="T" />.</returns>
        public static T ExecuteScalarOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.ExecuteScalar<T>(sql, param, transaction, commandTimeout, commandType);
        }


        /// <summary>
        ///     Execute parameterized SQL and return an <see cref="IDataReader" />.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="filepath">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static IDataReader ExecuteReaderOutsideSql(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.ExecuteReader(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a sequence of dynamic objects with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static IEnumerable<dynamic> QueryOutsideSql(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryOutsideSql<dynamic>(cnn, filepath, param as object, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QueryFirstOutsideSql(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryFirstOutsideSql<dynamic>(cnn, filepath, param as object, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QueryFirstOrDefaultOutsideSql(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryFirstOrDefaultOutsideSql<dynamic>(cnn, filepath, param as object, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QuerySingleOutsideSql(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QuerySingleOutsideSql<dynamic>(cnn, filepath, param as object, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QuerySingleOrDefaultOutsideSql(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QuerySingleOrDefaultOutsideSql<dynamic>(cnn, filepath, param as object, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of results to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static IEnumerable<T> QueryOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null,  IDbTransaction transaction = null, 
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QueryFirstOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var type = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, type);
            return cnn.QueryFirst<T>(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QueryFirstOrDefaultOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.QueryFirstOrDefault<T>(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QuerySingleOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.QuerySingle<T>(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QuerySingleOrDefaultOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.QuerySingleOrDefault<T>(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static IEnumerable<object> QueryOutsideSql(this IDbConnection cnn, Type type, string filepath, object param = null, IDbTransaction transaction = null, 
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.Query(type, sql, param, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static object QueryFirstOutsideSql(this IDbConnection cnn, Type type, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.QueryFirst(type, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static object QueryFirstOrDefaultOutsideSql(this IDbConnection cnn, Type type, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.QueryFirstOrDefault(type, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static object QuerySingleOutsideSql(this IDbConnection cnn, Type type, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.QuerySingle(type, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static object QuerySingleOrDefaultOutsideSql(this IDbConnection cnn, Type type, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.QuerySingleOrDefault(type, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a command that returns multiple result sets, and access each in turn.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static SqlMapper.GridReader QueryMultipleOutsideSql(this IDbConnection cnn, string filepath, object param = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            return cnn.QueryMultiple(sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Read sql file.
        /// </summary>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="type">Bind Viarables type.</param>
        /// <returns>sql</returns>
        private static string _ParseFile(string filepath, object param,
            BindVariableType type = BindVariableType.Question)
        {
            string sql;
            if (sqlDictionary.ContainsKey(filepath))
            {
                sql = sqlDictionary[filepath];
            }
            else
            {
                var fileInfo = new FileInfo(filepath);
                using (var fileReader = new FileReader(fileInfo))
                {
                    var charcode = fileReader.Read(fileInfo);

                    using (TextReader reader = new StreamReader(filepath, charcode.GetEncoding()))
                    {
                        sql = reader.ReadToEnd();
                    }
                }

                sqlDictionary.TryAdd(filepath, sql);
            }

            var parser = new Parser(sql);
            var rootNode = parser.Parse();

            ICommandContext ctx = new CommandContextImpl(BindVariableType.Question)
            {
                BindVariableType = type
            };

            var p = new DynamicParameters(param);
            p.ParameterNames.AsList().ForEach(s =>
            {
                object v = p.Get<dynamic>(s);
                ctx.AddArg(s, v, v.GetType());
            });

            rootNode.Accept(ctx);

            logger.Debug(ctx.SqlWithValue);

            return ctx.Sql;
        }
    }
}