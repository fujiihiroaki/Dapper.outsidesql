#region copyright

// /*
//  * Copyright 2018-2018 Hiroaki Fujii  All rights reserved. 
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

#endregion

namespace Jiifureit.Dapper.OutsideSql.Nodes
{
    public class ParenBindVariableNode : AbstractNode
    {
        private readonly string _bindName;

        public ParenBindVariableNode(string expression)
        {
            _bindName = expression;
            Expression = "self.GetArg('" + expression + "')";
        }

        public string Expression { get; }

        public override void Accept(ICommandContext ctx)
        {
            var o = InvokeExpression(Expression, ctx);
            if (o != null)
            {
                if (o is IList list)
                {
                    Array array = new object[list.Count];
                    list.CopyTo(array, 0);
                    _BindArray(ctx, array);
                }
            }
        }

        private void _BindArray(ICommandContext ctx, object arrayArg)
        {
            var array = (object[]) arrayArg;
            if (array != null)
            {
                var length = array.Length;
                if (length == 0) return;
                Type type = null;
                for (var i = 0; i < length; ++i)
                {
                    var o = array[i];
                    if (o != null) type = o.GetType();
                }

                ctx.AddSql("(");
                ctx.AddSql(array[0], type, _bindName + 1);
                for (var i = 1; i < length; ++i) ctx.AppendSql(array[i], type, _bindName + (i + 1));
            }

            ctx.AddSql(")");
        }
    }
}