// /* 
// *  Copyright (c) 2018-2018  Hiroaki Fujii All rights reserved. Licensed under the MIT license. 
// *  See LICENSE in the source repository root for complete license information. 
// */

using System;
using System.Collections;
using Seasar.Dao;
using Seasar.Dao.Node;
using Seasar.Dao.Parser;
using static System.String;

namespace Jiifureit.Dapper.OutsideSql.SqlParser
{
    public  class Parser
    {
        private readonly ISqlTokenizer _tokenizer;
        private readonly Stack _nodeStack = new Stack();

        public Parser(string sql)
        {
            sql = sql.Trim();
            if (sql.EndsWith(";"))
            {
                sql = sql.Substring(0, sql.Length - 1);
            }
            _tokenizer = new SqlTokenizerImpl(sql);
        }

        public INode Parse()
        {
            Push(new ContainerNode());
            while (TokenType.EOF != _tokenizer.Next())
            {
                ParseToken();
            }
            return Pop();
        }

        protected void ParseToken()
        {
            switch (_tokenizer.TokenType)
            {
                case TokenType.SQL:
                    ParseSql();
                    break;
                case TokenType.COMMENT:
                    ParseComment();
                    break;
                case TokenType.ELSE:
                    ParseElse();
                    break;
                case TokenType.BIND_VARIABLE:
                    ParseBindVariable();
                    break;
            }
        }

        protected void ParseSql()
        {
            var sql = _tokenizer.Token;
            if (IsElseMode())
            {
                sql = sql.Replace("--", Empty);
            }
            INode node = Peek();

            if ((node is IfNode || node is ElseNode) && node.ChildSize == 0)
            {
                ISqlTokenizer st = new SqlTokenizerImpl(sql);
                st.SkipWhitespace();
                var token = st.SkipToken();
                st.SkipWhitespace();
                if ("AND".Equals(token.ToUpper()) || "OR".Equals(token.ToUpper()))
                {
                    node.AddChild(new PrefixSqlNode(st.Before, st.After));
                }
                else
                {
                    node.AddChild(new SqlNode(sql));
                }
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
            if (IsNullOrEmpty(condition))
            {
                throw new IfConditionNotFoundRuntimeException();
            }
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
            while (TokenType.EOF != _tokenizer.Next())
            {
                if (_tokenizer.TokenType == TokenType.COMMENT
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
            if (!(parent is IfNode))
            {
                return;
            }
            var ifNode = (IfNode)Pop();
            var elseNode = new ElseNode();
            ifNode.ElseNode = elseNode;
            Push(elseNode);
            _tokenizer.SkipWhitespace();
        }

        protected void ParseCommentBindVariable()
        {
            var expr = _tokenizer.Token;
            var s = _tokenizer.SkipToken();
            if (s.StartsWith("(") && s.EndsWith(")"))
            {
                Peek().AddChild(CreateParenBindVariableNode(expr));
            }
            else if (expr.StartsWith("$"))
            {
                Peek().AddChild(CreateEmbeddedValueNode(expr.Substring(1)));
            }
            else
            {
                Peek().AddChild(CreateBindVariableNode(expr));
            }
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
            return (INode)_nodeStack.Pop();
        }

        protected INode Peek()
        {
            return (INode)_nodeStack.Peek();
        }

        protected void Push(INode node)
        {
            _nodeStack.Push(node);
        }

        protected bool IsElseMode()
        {
            for (var i = 0; i < _nodeStack.Count; ++i)
            {
                if (_nodeStack.ToArray()[i] is ElseNode)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool _IsTargetComment(string comment)
        {
            return !IsNullOrEmpty(comment)
                && _IsCSharpIdentifierStart(comment.ToCharArray()[0]);
        }

        private static bool _IsCSharpIdentifierStart(Char c)
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