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

namespace Seasar.Dao.Node
{
    public class IfNode : ContainerNode
    {
        private readonly string _expression;

        public IfNode(string expression)
        {
            var expressionUtil = new ExpressionUtil();
            _expression = expressionUtil.ParseExpression(expression);
            if (_expression == null)
                throw new ApplicationException("IllegalBoolExpression=[" + _expression + "]");
        }

        public string Expression => _expression;

        public ElseNode ElseNode { get; set; }

        public override void Accept(ICommandContext ctx)
        {
            object result = InvokeExpression(_expression, ctx);
            if (result != null)
            {
                if (Convert.ToBoolean(result))
                {
                    base.Accept(ctx);
                    ctx.IsEnabled = true;
                }
                else if (ElseNode != null)
                {
                    ElseNode.Accept(ctx);
                    ctx.IsEnabled = true;
                }
            }
            else
            {
                throw new ApplicationException("IllegalBoolExpression=[" + _expression + "]");
            }
        }
    }
}
