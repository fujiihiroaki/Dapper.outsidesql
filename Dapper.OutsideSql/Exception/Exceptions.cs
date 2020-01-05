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

#region using

using System;
using System.Reflection;
using System.Runtime.Serialization;

#endregion

namespace Jiifureit.Dapper.OutsideSql.Exception
{
    [Serializable]
    public class EndCommentNotFoundRuntimeException : SRuntimeException
    {
        public EndCommentNotFoundRuntimeException()
            : base("EDAO0007")
        {
        }
    }

    [Serializable]
    public class IfConditionNotFoundRuntimeException : SRuntimeException
    {
        public IfConditionNotFoundRuntimeException()
            : base("EDAO0004")
        {
        }
    }

    [Serializable]
    public class IllegalBoolExpressionRuntimeException : SRuntimeException
    {
        public IllegalBoolExpressionRuntimeException(string expression)
            : base("EDAO0003", new object[] {expression})
        {
            Expression = expression;
        }

        public IllegalBoolExpressionRuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Expression = info.GetString("_expression");
        }

        public string Expression { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_expression", Expression, typeof(string));
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class UpdateFailureRuntimeException : SRuntimeException
    {
        public UpdateFailureRuntimeException(object bean, int rows)
            : base("EDAO0005", new object[] {bean.ToString(), rows.ToString()})
        {
            Bean = bean;
            Rows = rows;
        }

        public UpdateFailureRuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Bean = info.GetValue("_bean", typeof(object));
            Rows = info.GetInt32("_rows");
        }

        public object Bean { get; }

        public int Rows { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_bean", Bean, typeof(object));
            info.AddValue("_rows", Rows, typeof(int));
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class TokenNotClosedRuntimeException : SRuntimeException
    {
        public TokenNotClosedRuntimeException(string token, string sql)
            : base("EDAO0002", new object[] {token, sql})
        {
            Token = token;
            Sql = sql;
        }

        public TokenNotClosedRuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Token = info.GetString("_token");
            Sql = info.GetString("_sql");
        }

        public string Token { get; }

        public string Sql { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_token", Token, typeof(string));
            info.AddValue("_sql", Sql, typeof(string));
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class WrongPropertyTypeOfTimestampException : SRuntimeException
    {
        public WrongPropertyTypeOfTimestampException(string propertyName, string propertyType)
            : base("EDAO0010", new object[] {propertyName, propertyType})
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
        }

        public WrongPropertyTypeOfTimestampException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PropertyName = info.GetString("_propertyName");
            PropertyType = info.GetString("_propertyType");
        }

        public string PropertyName { get; }

        public string PropertyType { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_propertyName", PropertyName, typeof(string));
            info.AddValue("_propertyType", PropertyType, typeof(string));
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class NotFoundModifiedPropertiesRuntimeException : SRuntimeException
    {
        public NotFoundModifiedPropertiesRuntimeException(
            string beanClassName, string propertyName)
            : base("EDAXXXXX", new object[] {beanClassName, propertyName})
        {
            BeanClassName = beanClassName;
        }

        public string BeanClassName { get; }
    }

    [Serializable]
    public class NoUpdatePropertyTypeRuntimeException : SRuntimeException
    {
        public NoUpdatePropertyTypeRuntimeException()
            : base("EDA00012")
        {
        }
    }

    [Serializable]
    public class SqlFileNotFoundRuntimeException : SRuntimeException
    {
        public SqlFileNotFoundRuntimeException(string fileName)
            : base("EDAO0025", new object[] {fileName})
        {
        }
    }

    [Serializable]
    public class IllegalReturnElementTypeException : SRuntimeException
    {
        public IllegalReturnElementTypeException(MemberInfo elementType, MemberInfo resultType)
            : base("EDAO0026", new object[] {elementType.Name, resultType.Name})
        {
        }
    }

    [Serializable]
    public class SqlStreamNullException : SRuntimeException
    {
        public SqlStreamNullException()
            : base("EDAO0027")
        {
        }
    }
}