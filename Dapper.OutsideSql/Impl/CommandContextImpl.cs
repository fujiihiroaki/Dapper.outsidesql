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
using System.Collections;
using System.Text;
using Jiifureit.Dapper.OutsideSql.Utility;

#endregion

namespace Jiifureit.Dapper.OutsideSql.Impl
{
    public class CommandContextImpl : ICommandContext
    {
        private readonly Hashtable _argNames = new Hashtable(StringComparer.OrdinalIgnoreCase);

        private readonly Hashtable _args = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private readonly Hashtable _argTypes = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private readonly IList _bindVariableNames = new ArrayList();
        private readonly IList _bindVariables = new ArrayList();
        private readonly IList _bindVariableTypes = new ArrayList();
        private readonly ICommandContext _parent;

        private readonly StringBuilder _sqlBuf = new StringBuilder(100);
        private readonly StringBuilder _sqlBufWithValue = new StringBuilder(100);

        public CommandContextImpl(BindVariableType type)
        {
            BindVariableType = type;
        }

        public CommandContextImpl(ICommandContext parent, BindVariableType type)
        {
            _parent = parent;
            BindVariableType = type;
            IsEnabled = false;
        }

        public BindVariableType BindVariableType { get; set; }

        public object GetArg(string name)
        {
            if (_args.ContainsKey(name))
            {
                return _args[name];
            }
            else if (_parent != null)
            {
                return _parent.GetArg(name);
            }
            else
            {
                var names = name.Split('.');
                var value = _args[names[0]];
                var type = GetArgType(names[0]);

                for (var pos = 1; pos < names.Length; pos++)
                {
                    if (value == null || type == null) break;
                    var pi = type.GetProperty(names[pos]);
                    if (pi == null) return null;
                    value = pi.GetValue(value, null);
                    type = pi.PropertyType;
                }

                return value;
            }
        }

        public Type GetArgType(string name)
        {
            if (_argTypes.ContainsKey(name))
            {
                return (Type) _argTypes[name];
            }
            else
            {
                return _parent?.GetArgType(name);
            }
        }

        public void AddArg(string name, object arg, Type argType)
        {
            if (_args.ContainsKey(name)) _args.Remove(name);
            _args.Add(name, arg);

            if (_argTypes.ContainsKey(name)) _argTypes.Remove(name);
            _argTypes.Add(name, argType);

            if (_argNames.ContainsKey(name)) _argNames.Remove(name);
            _argNames.Add(name, name);
        }

        public string Sql => _sqlBuf.ToString();

        public string SqlWithValue => _sqlBufWithValue.ToString();

        public object[] BindVariables
        {
            get
            {
                var variables = new object[_bindVariables.Count];
                _bindVariables.CopyTo(variables, 0);
                return variables;
            }
        }

        public Type[] BindVariableTypes
        {
            get
            {
                var variables = new Type[_bindVariableTypes.Count];
                _bindVariableTypes.CopyTo(variables, 0);
                return variables;
            }
        }

        public string[] BindVariableNames
        {
            get
            {
                var variableNames = new string[_bindVariableNames.Count];
                _bindVariableNames.CopyTo(variableNames, 0);
                return variableNames;
            }
        }

        public ICommandContext AddSql(string sql)
        {
            _sqlBuf.Append(sql);
            _sqlBufWithValue.Append(sql);

            return this;
        }

        public ICommandContext AddSql(string sql, object bindVariable,
            Type bindVariableType, string bindVariableName)
        {
            string Func(object o)
            {
                if (o is decimal || o is byte || o is double || o is float || o is int || o is long || o is short ||
                    o is sbyte || o is uint || o is ulong || o is ushort)
                    return Convert.ToString(o);
                else if (o == null)
                    return "null";
                else
                    return "'" + Convert.ToString(o) + "'";
            }

            var after = sql;
            if (BindVariableType == BindVariableType.AtmarkWithParam)
                after = sql.Replace("?", "@" + bindVariableName);
            if (BindVariableType == BindVariableType.ColonWithParam)
                after = sql.Replace("?", ":" + bindVariableName);
            if (BindVariableType == BindVariableType.QuestionWithParam)
                after = sql + bindVariableName;

            _sqlBuf.Append(after);
            _sqlBufWithValue.Append(sql.Replace("?", Func(bindVariable)));
            _bindVariables.Add(bindVariable);
            _bindVariableTypes.Add(bindVariableType);
            _bindVariableNames.Add(bindVariableName);
            return this;
        }

        public ICommandContext AddSql(object bindVariable, Type bindVariableType, string bindVariableName)
        {
            AddSql("?", bindVariable, bindVariableType, bindVariableName);
            return this;
        }

        public ICommandContext AddSql(string sql, object[] bindVariables,
            Type[] bindVariableTypes, string[] bindVariableNames)
        {
            _sqlBuf.Append(sql);

            var after = sql;
            for (var i = 0; i < bindVariableTypes.Length; i++)
            {
                var o = bindVariables[i];
                var pos = after.IndexOf(bindVariableNames[i]);
                if (pos > 0)
                {
                    string quote;
                    if (o is decimal || o is byte || o is double || o is float || o is int || o is long || o is short ||
                        o is sbyte || o is uint || o is ulong || o is ushort)
                        quote = "";
                    else
                        quote = "'";

                    after = after.Substring(0, pos - 1) + quote + Convert.ToString(o) + quote +
                            after.Substring(pos + bindVariableNames[i].Length);
                }
            }

            _sqlBufWithValue.Append(after);

            for (var i = 0; i < bindVariables.Length; ++i)
            {
                _bindVariables.Add(bindVariables[i]);
                _bindVariableTypes.Add(bindVariableTypes[i]);
                _bindVariableNames.Add(bindVariableNames[i]);
            }

            return this;
        }

        public ICommandContext AppendSql(object bindVariable, Type bindVariableType, string bindVariableName)
        {
            AddSql(", ?", bindVariable, bindVariableType, bindVariableName);
            return this;
        }

        public bool IsEnabled { get; set; } = true;
    }
}