#region Copyright
/*
 * Copyright 2005-2015 the Seasar Foundation and the Others.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
 * either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */
#endregion

using System;
using Seasar.Framework.Util;

namespace Seasar.Dao
{
    public interface ICommandContext
    {
        object GetArg(string name);
        Type GetArgType(string name);
        void AddArg(string name, object arg, Type argType);
        string Sql { get; }
        string SqlWithValue { get; }
        object[] BindVariables { get; }
        Type[] BindVariableTypes { get; }
        string[] BindVariableNames { get; }
        BindVariableType BindVariableType { get; set; }
        ICommandContext AddSql(string sql);
        ICommandContext AddSql(string sql, object bindVariable, Type bindVariableType, string bindVariableName);
        ICommandContext AddSql(object bindVariable, Type bindVariableType, string bindVariableName);
        ICommandContext AddSql(string sql, object[] bindVariables, Type[] bindVariableTypes, string[] bindVariableNames);
        ICommandContext AppendSql(object bindVariable, Type bindVariableType, string bindVariableName);
        bool IsEnabled { get; set; }
    }
}
