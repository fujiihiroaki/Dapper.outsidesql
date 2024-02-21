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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dapper;

#endregion

namespace Jiifureit.Dapper.OutsideSql.Utility
{
    public static class DynamicParameterUtil
    {
        /// <summary>
        ///     Recreate arguments.
        /// </summary>
        /// <param name="param"></param>
        /// <returns>Recreated arguments parameter</returns>
        public static object CreateDynamicParameters(object param)
        {
            if (param == null)
                return null;

            if (param is IEnumerable<KeyValuePair<string, object>>)
                return param;

            // Convert parameters;
            var newParam = new DynamicParameters();
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
                            _CreateParameter(newParam, p, p.GetType(), name);
                        else
                            _CreateParameter(newParam, null, new object().GetType(), name);
                    }
                }

                return newParam;
            }
            else
            {
                if (!(param is IEnumerable<KeyValuePair<string, object>> dictionary))
                {
                    var newParamList = new List<DynamicParameters>();
                    var paramType = param.GetType();
                    if (paramType.IsArray)
                    {
                        Array array = (Array)param;
                        for (int i = 0; i < array.Length; i++)
                        {
                            newParam = new DynamicParameters();
                            var pValue = array.GetValue(i);
                            var properties = pValue.GetType().GetProperties();
                            foreach (var property in properties)
                            {
                                var val = property.GetValue(pValue);
                                var type = property.PropertyType;
                                var name = property.Name;
                                _CreateParameter(newParam, val, type, name);
                                
                            }
                            newParamList.Add(newParam);
                        }
                    }
                    else if (paramType.GetInterface("System.Collections.IList") != null)
                    {
                        var list = (IList) param;
                        foreach (object pValue in list)
                        {
                            newParam = new DynamicParameters();
                            var properties = pValue.GetType().GetProperties();
                            foreach (var property in properties)
                            {
                                var val = property.GetValue(pValue);
                                var type = property.PropertyType;
                                var name = property.Name;
                                _CreateParameter(newParam, val, type, name);
                            }
                            newParamList.Add(newParam);
                        }
                    }
                    else if (paramType.GetInterface("System.Collections.ICollection") != null)
                    {
                        var list = ((ICollection)param).GetEnumerator();
                        list.Reset();
                        while (list.MoveNext())
                        {
                            var pValue = list.Current;
                            var properties = pValue?.GetType().GetProperties();
                            if (properties != null)
                            {
                                foreach (var property in properties)
                                {
                                    var val = property.GetValue(pValue);
                                    var type = property.PropertyType;
                                    var name = property.Name;
                                    _CreateParameter(newParam, val, type, name);
                                }
                            }
                            else
                            {
                                throw new NullReferenceException("parameter properties is null");
                            }
                        }
                    }
                    else
                    {
                        var properties = param.GetType().GetProperties();
                        foreach (var info in properties)
                        {
                            var val = info.GetValue(param);
                            var type = info.PropertyType;
                            var name = info.Name;
                            _CreateParameter(newParam, val, type, name);
                        }
                        return newParam;
                    }
                    return newParamList;
                }
                else
                {
                    foreach (var keyValue in dictionary)
                    {
                        var v = keyValue.Value;
                        _CreateParameter(newParam, v, v.GetType(), keyValue.Key);
                    }
                    return newParam;
                }
            }
        }

        /// <summary>
        ///     RecreateParameter
        /// </summary>
        /// <param name="newParam">Recreate parameter</param>
        /// <param name="value">parameter value</param>
        /// <param name="info">parameter type</param>
        /// <param name="name">parameter name</param>
        private static void _CreateParameter(DynamicParameters newParam, object value, Type info, string name)
        {
            // IEnumerable<> Check
            bool IsGenericEnumerable(Type type)
            {
                return type.GetInterfaces()
                    .Any(t => t.IsGenericType &&
                              (t.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                               t.GetGenericTypeDefinition() == typeof(IList<>)));
            }

            if (info.GetInterface("System.Collections.ICollection") != null)
            {
                var list = (ICollection) value;
                var i = 1;
                foreach (var o in list)
                {
                    newParam.Add(name + i, o);
                    i++;
                }
            }
            else if (info.GetInterface("System.Collections.IList") != null)
            {
                var list = (IList) value;
                var i = 1;
                foreach (var o in list)
                {
                    newParam.Add(name + i, o);
                    i++;
                }
            }
            else if (IsGenericEnumerable(info))
            {
                var val = (IList) value;
                if (val != null)
                {
                    for (var i = 0; i < val.Count; i++) newParam.Add(name + (i + 1), val[i]);
                }
                else
                {
                    var val2 = (ICollection) value;
                    var i = 1;
                    foreach (var o in val2)
                    {
                        newParam.Add(name + i, o);
                        i++;
                    }
                }
            }
            else if (info.IsArray)
            {
                var rank = value.GetType().GetArrayRank();
                for (var i = 0; i < rank; i++)
                {
                    var val = (value as Array)?.GetValue(i);
                    newParam.Add(name + (i + 1), val);
                }
            }
            else
            {
                newParam.Add(name, value);
            }
        }
    }
}