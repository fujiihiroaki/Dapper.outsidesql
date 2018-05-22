// /* 
// *  Copyright (c) 2018-2018  Hiroaki Fujii All rights reserved. Licensed under the MIT license. 
// *  See LICENSE in the source repository root for complete license information. 
// */

using System;
using System.Collections;
using System.Text;
using NLog;

namespace Seasar.Dao.Context
{
    public class CommandContextImpl : ICommandContext
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Hashtable _args = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private readonly Hashtable _argTypes = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private readonly Hashtable _argNames = new Hashtable(StringComparer.OrdinalIgnoreCase);

        private readonly StringBuilder _sqlBuf = new StringBuilder(100);
        private readonly IList _bindVariables = new ArrayList();
        private readonly IList _bindVariableTypes = new ArrayList();
        private readonly IList _bindVariableNames = new ArrayList();
        private readonly ICommandContext _parent;

        public CommandContextImpl()
        {
        }

        public CommandContextImpl(ICommandContext parent)
        {
            _parent = parent;
            IsEnabled = false;
        }

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
                var value = _args[names[0]]; ;
                var type = GetArgType(names[0]);

                for (var pos = 1; pos < names.Length; pos++)
                {
                    if (value == null || type == null) break;
                    var pi = type.GetProperty(names[pos]);
                    if (pi == null)
                    {
                        return null;
                    }
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
                return (Type)_argTypes[name];
            }
            else if (_parent != null)
            {
                return _parent.GetArgType(name);
            }
            else
            {
                logger.Error("WDAO0001", new object[] { name });
                return null;
            }
        }

        public void AddArg(string name, object arg, Type argType)
        {
            if (_args.ContainsKey(name))
            {
                _args.Remove(name);
            }
            _args.Add(name, arg);

            if (_argTypes.ContainsKey(name))
            {
                _argTypes.Remove(name);
            }
            _argTypes.Add(name, argType);

            if (_argNames.ContainsKey(name))
            {
                _argNames.Remove(name);
            }
            _argNames.Add(name, name);
        }

        public string Sql => _sqlBuf.ToString();

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
            return this;
        }

        public ICommandContext AddSql(string sql, object bindVariable,
            Type bindVariableType, string bindVariableName)
        {

            _sqlBuf.Append(sql);
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