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
using System.Threading.Tasks;
using Dapper;
using Jiifureit.Dapper.OutsideSql.Utility;

#endregion

namespace Jiifureit.Dapper.OutsideSql
{
    /// <summary>
    ///     Dapper Extension for writing sql to logfile.
    /// </summary>
    public static partial class DapperLogExtension
    {
        /// <summary>
        ///     Execute parameterized SQL asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        public static Task<int> ExecuteLogAsync(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteAsync(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell selected as <see cref="object" />.</returns>
        public static Task<object> ExecuteScalarLogAsync(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteScalarAsync(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell returned, as <typeparamref name="T" />.</returns>
        public static Task<T> ExecuteScalarLogAsync<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteScalarAsync<T>(newSql, newParam, transaction, commandTimeout, commandType);
        }


        /// <summary>
        ///     Execute parameterized SQL and return an <see cref="IDataReader" /> asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static Task<IDataReader> ExecuteReaderLogAsync(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteReaderAsync(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static Task<dynamic> QueryFirstLogAsync(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryFirstLogAsync<dynamic>(cnn, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static Task<dynamic> QueryFirstOrDefaultLogAsync(this IDbConnection cnn, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryFirstOrDefaultLogAsync<dynamic>(cnn, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static Task<dynamic> QuerySingleLogAsync(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QuerySingleLogAsync<dynamic>(cnn, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static Task<dynamic> QuerySingleOrDefaultLogAsync(this IDbConnection cnn, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QuerySingleOrDefaultLogAsync<dynamic>(cnn, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a query, returning the data typed as <typeparamref name="T" /> asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="types">Array of types in the recordset.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static Task<IEnumerable<T>> QueryLogAsync<T>(this IDbConnection cnn, string sql, 
            Type[] types, Func<object[], T> map,
            object param = null, IDbTransaction transaction = null,
            bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryAsync(newSql, types, map, newParam, transaction, buffered, splitOn, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a query, returning the data typed as <typeparamref name="T" /> asynchronously using Task.
        /// </summary>
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
        public static Task<IEnumerable<T>> QueryLogAsync<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryAsync<T>(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" /> asynchronously using Task.
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
        public static Task<T> QueryFirstLogAsync<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstAsync<T>(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" /> asynchronously using Task.
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
        public static Task<T> QueryFirstOrDefaultLogAsync<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefaultAsync<T>(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" /> asynchronously using Task.
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
        public static Task<T> QuerySingleLogAsync<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleAsync<T>(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" /> asynchronously using Task.
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
        public static Task<T> QuerySingleOrDefaultLogAsync<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefaultAsync<T>(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="types" /> asynchronously using Task.
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
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static Task<IEnumerable<object>> QueryLogAsync(this IDbConnection cnn, string sql, Type[] types,
            Func<object[], object> map, object param = null, IDbTransaction transaction = null,
            bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryAsync(newSql, types, map, newParam, transaction, buffered, splitOn, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query asynchronously using Task.
        /// </summary>
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
        public static Task<IEnumerable<object>> QueryLogAsync(this IDbConnection cnn, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryAsync<object>(newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" /> asynchronously using Task.
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
        public static Task<object> QueryFirstLogAsync(this IDbConnection cnn, Type type, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstAsync(type, newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" /> asynchronously using Task.
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
        public static Task<object> QueryFirstOrDefaultLogAsync(this IDbConnection cnn, Type type, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefaultAsync(type, newSql, newParam, transaction, commandTimeout, commandType);
        }
        
        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" /> asynchronously using Task.
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
        public static Task<object> QuerySingleLogAsync(this IDbConnection cnn, Type type, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleAsync(type, newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" /> asynchronously using Task.
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
        public static Task<object> QuerySingleOrDefaultLogAsync(this IDbConnection cnn, Type type, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefaultAsync(type, newSql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a command that returns multiple result sets, and access each in turn asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static Task<SqlMapper.GridReader> QueryMultipleLogAsync(this IDbConnection cnn, string sql,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var newSql = _LogSql(sql, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryMultipleAsync(newSql, newParam, transaction, commandTimeout, commandType);
        }
    }
}