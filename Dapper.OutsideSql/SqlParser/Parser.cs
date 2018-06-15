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

using System.Collections;
using Jiifureit.Dapper.OutsideSql.Exception;
using Jiifureit.Dapper.OutsideSql.Nodes;
using static System.String;

#endregion

namespace Jiifureit.Dapper.OutsideSql.SqlParser
{
    public class Parser
    {
        private readonly Stack _nodeStack = new Stack();
        private readonly ISqlTokenizer _tokenizer;

        public Parser(string sql)
        {
            sql = sql.Trim();
            if (sql.EndsWith(";")) sql = sql.Substring(0, sql.Length - 1);
            _tokenizer = new SqlTokenizerImpl(sql);
        }

        public INode Parse()
        {
            Push(new ContainerNode());
            while (TokenType.Eof != _tokenizer.Next()) ParseToken();
            return Pop();
        }

        protected void ParseToken()
        {
            switch (_tokenizer.TokenType)
            {
                case TokenType.Sql:
                    ParseSql();
                    break;
                case TokenType.Comment:
                    ParseComment();
                    break;
                case TokenType.Else:
                    ParseElse();
                    break;
                case TokenType.BindVariable:
                    ParseBindVariable();
                    break;
            }
        }

        protected void ParseSql()
        {
            var sql = _tokenizer.Token;
            if (IsElseMode()) sql = sql.Replace("--", Empty);
            INode node = Peek();

            if ((node is IfNode || node is ElseNode) && node.ChildSize == 0)
            {
                ISqlTokenizer st = new SqlTokenizerImpl(sql);
                st._SkipWhitespace();
                var token = st.SkipToken();
                st._SkipWhitespace();
                if ("AND".Equals(token.ToUpper()) || "OR".Equals(token.ToUpper()))
                    node.AddChild(new PrefixSqlNode(st.Before, st.After));
                else
                    node.AddChild(new SqlNode(sql));
            }
            else
            {
                node.AddChild(new SqlNode(sql));
            }
        }

        protected void ParseComment()
        {
            var comment = _tokenizer.Token;
            if (_IsTargetComment(comment))
            {
                if (_IsIfComment(comment))
                {
                    ParseIf();
                }
                else if (_IsBeginComment(comment))
                {
                    ParseBegin();
                }
                else if (_IsEndComment(comment))
                {
                }
                else
                {
                    ParseCommentBindVariable();
                }
            }
        }

        protected void ParseIf()
        {
            var condition = _tokenizer.Token.Substring(2).Trim();
            if (IsNullOrEmpty(condition)) throw new IfConditionNotFoundRuntimeException();
            var ifNode = new IfNode(condition);
            Peek().AddChild(ifNode);
            Push(ifNode);
            ParseEnd();
        }

        protected void ParseBegin()
        {
            var beginNode = new BeginNode();
            Peek().AddChild(beginNode);
            Push(beginNode);
            ParseEnd();
        }

        protected void ParseEnd()
        {
            while (TokenType.Eof != _tokenizer.Next())
            {
                if (_tokenizer.TokenType == TokenType.Comment
                    && _IsEndComment(_tokenizer.Token))
                {
                    Pop();
                    return;
                }

                ParseToken();
            }

            throw new EndCommentNotFoundRuntimeException();
        }

        protected void ParseElse()
        {
            var parent = Peek();
            if (!(parent is IfNode)) return;
            var ifNode = (IfNode) Pop();
            var elseNode = new ElseNode();
            ifNode.ElseNode = elseNode;
            Push(elseNode);
            _tokenizer._SkipWhitespace();
        }

        protected void ParseCommentBindVariable()
        {
            var expr = _tokenizer.Token;
            var s = _tokenizer.SkipToken();
            if (s.StartsWith("(") && s.EndsWith(")"))
                Peek().AddChild(CreateParenBindVariableNode(expr));
            else if (expr.StartsWith("$"))
                Peek().AddChild(CreateEmbeddedValueNode(expr.Substring(1)));
            else
                Peek().AddChild(CreateBindVariableNode(expr));
        }

        protected void ParseBindVariable()
        {
            var expr = _tokenizer.Token;
            Peek().AddChild(CreateBindVariableNode(expr));
        }

        protected ParenBindVariableNode CreateParenBindVariableNode(string expr)
        {
            return new ParenBindVariableNode(expr);
        }

        protected EmbeddedValueNode CreateEmbeddedValueNode(string expr)
        {
            return new EmbeddedValueNode(expr);
        }

        protected BindVariableNode CreateBindVariableNode(string expr)
        {
            return new BindVariableNode(expr);
        }

        protected INode Pop()
        {
            return (INode) _nodeStack.Pop();
        }

        protected INode Peek()
        {
            return (INode) _nodeStack.Peek();
        }

        protected void Push(INode node)
        {
            _nodeStack.Push(node);
        }

        protected bool IsElseMode()
        {
            for (var i = 0; i < _nodeStack.Count; ++i)
                if (_nodeStack.ToArray()[i] is ElseNode)
                    return true;
            return false;
        }

        private static bool _IsTargetComment(string comment)
        {
            return !IsNullOrEmpty(comment)
                   && _IsCSharpIdentifierStart(comment.ToCharArray()[0]);
        }

        private static bool _IsCSharpIdentifierStart(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '\\' || c == '$' || c == '@';
        }

        private static bool _IsIfComment(string comment)
        {
            return comment.StartsWith("IF");
        }

        private static bool _IsBeginComment(string content)
        {
            return content != null && "BEGIN".Equals(content);
        }

        private static bool _IsEndComment(string content)
        {
            return content != null && "END".Equals(content);
        }
    }
}