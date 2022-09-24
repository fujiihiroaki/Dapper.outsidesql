#region copyright

// /*
//  * Copyright 2018-2022 Hiroaki Fujii  All rights reserved. 
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
using Dapper;
using Jiifureit.Dapper.OutsideSql.Impl;
using Jiifureit.Dapper.OutsideSql.SqlParser;
using Jiifureit.Dapper.OutsideSql.Utility;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Linq;
using Logger = Jiifureit.Dapper.OutsideSql.Log.Logger;

#endregion

namespace Jiifureit.Dapper.OutsideSql
{
    /// <summary>
    ///     Dapper Extension for writing sql to logfile.
    /// </summary>
    public static partial class DapperLogExtension
    {
        private static readonly ILogger logger = Logger.Create();

        /// <summary>
        ///     Execute parameterized SQL.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteLog(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.Execute(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell selected as <see cref="object" />.</returns>
        public static object ExecuteScalarLog(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteScalar(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell returned, as <typeparamref name="T" />.</returns>
        public static T ExecuteScalarLog<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteScalar<T>(newSql, newParam, transaction, commandTimeout, commandType);
        }


        /// <summary>
        ///     Execute parameterized SQL and return an <see cref="IDataReader" />.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static IDataReader ExecuteReaderLog(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteReader(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a sequence of dynamic objects with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static IEnumerable<dynamic> QueryLog(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryLog<dynamic>(cnn, sql, param, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QueryFirstLog(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryFirstLog<dynamic>(cnn, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QueryFirstOrDefaultLog(this IDbConnection cnn, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryFirstOrDefaultLog<dynamic>(cnn, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QuerySingleLog(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QuerySingleLog<dynamic>(cnn, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QuerySingleOrDefaultLog(this IDbConnection cnn, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QuerySingleOrDefaultLog<dynamic>(cnn, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
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
        public static IEnumerable<T> QueryLog<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.Query<T>(newSql, newParam, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QueryFirstLog<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirst<T>(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QueryFirstOrDefaultLog<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefault<T>(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QuerySingleLog<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingle<T>(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QuerySingleOrDefaultLog<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefault<T>(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
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
        public static IEnumerable<object> QueryLog(this IDbConnection cnn, Type type, string sql,
            object param = null, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.Query(type, newSql, newParam, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="types" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="types">The type to return.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="types" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static IEnumerable<object> QueryLog(this IDbConnection cnn, string sql, Type[] types,
            Func<object[], object> map, object param = null, IDbTransaction transaction = null,
            bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.Query(newSql, types, map, newParam, transaction, buffered, splitOn, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
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
        public static object QueryFirstLog(this IDbConnection cnn, Type type, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirst(type, newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
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
        public static object QueryFirstOrDefaultLog(this IDbConnection cnn, Type type, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefault(type, newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
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
        public static object QuerySingleLog(this IDbConnection cnn, Type type, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingle(type, newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
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
        public static object QuerySingleOrDefaultLog(this IDbConnection cnn, Type type, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefault(type, newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a command that returns multiple result sets, and access each in turn.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static SqlMapper.GridReader QueryMultipleLog(this IDbConnection cnn, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryMultiple(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     write sql to log file.
        /// </summary>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="bindVariableType"></param>
        private static string _LogSql(string sql, object param, BindVariableType bindVariableType)
        {
            // IEnumerable<> Check
            bool IsGenericEnumerable(Type type)
            {
                return type.GetInterfaces()
                    .Any(t => t.IsGenericType &&
                              (t.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                               t.GetGenericTypeDefinition() == typeof(IList<>)));
            }
            
            // parse file content.
            var parser = new Parser(sql);
            var rootNode = parser.Parse();

            ICommandContext ctx = new CommandContextImpl(BindVariableType.Question)
            {
                BindVariableType = bindVariableType
            };

            if (param != null)
            {
                // hold parameters.
                if (!(param is IEnumerable<KeyValuePair<string, object>> dictionary))
                {
                    if (param is DynamicParameters dynamicParam)
                    {
                        var lookup = (SqlMapper.IParameterLookup) dynamicParam;
                        using (var names = dynamicParam.ParameterNames.GetEnumerator())
                        {
                            while (names.MoveNext())
                            {
                                var name = names.Current;
                                var p = lookup[name];
                                if (p != null)
                                    ctx.AddArg(name, p, p.GetType());
                                else
                                    ctx.AddArg(name, null, null);
                            }
                        }
                        rootNode.Accept(ctx);
                        // log sql.
                        logger?.LogDebug(ctx.SqlWithValue);
                    }
                    else
                    {
#if DEBUG
                        var paramType = param.GetType();
                        if (paramType.IsArray)
                        {
                            Array array = (Array)param;
                            for (int i = 0; i < array.Length; i++)
                            {
                                ctx = new CommandContextImpl(BindVariableType.Question)
                                {
                                    BindVariableType = bindVariableType
                                };
                                
                                var pValue = array.GetValue(i);
                                var properties = pValue.GetType().GetProperties();
                                foreach (var property in properties)
                                {
                                    ctx.AddArg(property.Name, property.GetValue(pValue), property.GetValue(pValue).GetType());
                                }
                                rootNode.Accept(ctx);
                                logger?.LogDebug(ctx.SqlWithValue);
                            }
                        }
                        else if (paramType.GetInterface("System.Collections.IList") != null)
                        {
                            var list = (IList) param;
                            foreach (object pValue in list)
                            {
                                ctx = new CommandContextImpl(BindVariableType.Question)
                                {
                                    BindVariableType = bindVariableType
                                };

                                var properties = pValue.GetType().GetProperties();
                                foreach (var property in properties)
                                {
                                    ctx.AddArg(property.Name, property.GetValue(pValue), property.GetValue(pValue).GetType());
                                }
                                rootNode.Accept(ctx);
                                logger?.LogDebug(ctx.SqlWithValue);
                            }
                        }
                        else if (paramType.GetInterface("System.Collections.ICollection") != null
                                 || IsGenericEnumerable(paramType))
                        {
                            var list = ((ICollection)param).GetEnumerator();
                            list.Reset();
                            while (list.MoveNext())
                            {
                                ctx = new CommandContextImpl(BindVariableType.Question)
                                {
                                    BindVariableType = bindVariableType
                                };

                                var pValue = list.Current;
                                var properties = pValue?.GetType().GetProperties();
                                if (properties != null)
                                {
                                    foreach (var property in properties)
                                    {
                                        ctx.AddArg(property.Name, property.GetValue(pValue), property.GetValue(pValue).GetType());
                                    }
                                }
                                else
                                {
                                    throw new NullReferenceException("parameter properties is null");
                                }
                                
                                rootNode.Accept(ctx);
                                logger?.LogDebug(ctx.SqlWithValue);
                            }
                        }
                        else
                        {
                            var properties = param.GetType().GetProperties();
                            properties.AsList().ForEach(p =>
                            {
                                var v = p.GetValue(param);
                                if (v != null)
                                    ctx.AddArg(p.Name, p.GetValue(param), p.GetValue(param).GetType());
                                else
                                    ctx.AddArg(p.Name, null, null);
                            });
                            rootNode.Accept(ctx);
                            logger?.LogDebug(ctx.SqlWithValue);
                        }
#else
                        var paramType = param.GetType();
                        if (paramType.IsArray)
                        {
                            Array array = (Array)param;
                            for (int i = 0; i < array.Length; i++)
                            {
                                ctx = new CommandContextImpl(BindVariableType.Question)
                                {
                                    BindVariableType = bindVariableType
                                };
                                
                                var pValue = array.GetValue(i);
                                var properties = pValue.GetType().GetProperties();
                                properties.AsList().ForEach(p =>
                                {
                                    ctx.AddArg(p.Name, p.GetValue(pValue), p.GetValue(pValue).GetType());
                                });
                                rootNode.Accept(ctx);
                                logger?.LogDebug(ctx.SqlWithValue);
                            }
                        }
                        else if (paramType.GetInterface("System.Collections.IList") != null)
                        {
                            var list = (IList) param;
                            foreach (object pValue in list)
                            {
                                ctx = new CommandContextImpl(BindVariableType.Question)
                                {
                                    BindVariableType = bindVariableType
                                };

                                var properties = pValue.GetType().GetProperties();
                                properties.AsList().ForEach(p =>
                                {
                                    ctx.AddArg(p.Name, p.GetValue(pValue), p.GetValue(pValue).GetType());
                                });
                                rootNode.Accept(ctx);
                                logger?.LogDebug(ctx.SqlWithValue);
                            }
                        }
                        else if (paramType.GetInterface("System.Collections.ICollection") != null
                                 || IsGenericEnumerable(paramType))
                        {
                            var list = ((ICollection)param).GetEnumerator();
                            list.Reset();
                            while (list.MoveNext())
                            {
                                ctx = new CommandContextImpl(BindVariableType.Question)
                                {
                                    BindVariableType = bindVariableType
                                };

                                var pValue = list.Current;
                                var properties = pValue.GetType().GetProperties();
                                properties.AsList().ForEach(p =>
                                {
                                    ctx.AddArg(p.Name, p.GetValue(pValue), p.GetValue(pValue).GetType());
                                });

                                rootNode.Accept(ctx);
                                logger?.LogDebug(ctx.SqlWithValue);
                            }
                        }
                        else
                        {
                            var properties = param.GetType().GetProperties();
                            properties.AsList().ForEach(p =>
                            {
                                var v = p.GetValue(param);
                                if (v != null)
                                    ctx.AddArg(p.Name, p.GetValue(param), p.GetValue(param).GetType());
                                else
                                    ctx.AddArg(p.Name, null, null);
                            });
                            rootNode.Accept(ctx);
                            // log sql.
                            logger?.LogDebug(ctx.SqlWithValue);
                        }
#endif
                    }
                }
                else
                {
                    foreach (var keyValue in dictionary)
                    {
                        var v = keyValue.Value;
                        if (v != null) 
                            ctx.AddArg(keyValue.Key, keyValue.Value, keyValue.Value.GetType());
                        else
                            ctx.AddArg(keyValue.Key, null, null);
                    }
                    rootNode.Accept(ctx);
                    // log sql.
                    logger?.LogDebug(ctx.SqlWithValue);
                }
            }
            else
            {
                rootNode.Accept(ctx);
               // log sql.
                logger?.LogDebug(ctx.SqlWithValue);
            }
            
            return ctx.Sql;
        }
    }
}