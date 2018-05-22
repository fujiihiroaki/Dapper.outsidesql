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
using NLog;

namespace Seasar.Dao.Node
{
    public class BindVariableNode : AbstractNode
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public BindVariableNode(string expression)
        {
            Expression = expression;
        }

        public string Expression { get; }

        public override void Accept(ICommandContext ctx)
        {
            object value = ctx.GetArg(Expression);
            Type type = null;
            if (value != null)
            {
                type = value.GetType();
            }
            else
            {
                logger.Log(LogLevel.Error, new object[] { Expression });
            }
            ctx.AddSql(value, type, Expression.Replace('.', '_'));
        }
    }
}
