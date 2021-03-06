﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class CaseWhenFlatteningExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public CaseWhenFlatteningExpressionVisitor([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override Expression VisitExtension(Expression node)
        {
            Check.NotNull(node, nameof(node));

            // Only applies to 'CASE WHEN condition...' not 'CASE operand WHEN...'
            if (node is CaseExpression caseExpression && caseExpression.Operand == null)
            {
                if (caseExpression.ElseResult is CaseExpression nestedCaseExpression && nestedCaseExpression.Operand == null)
                {
                    return VisitExtension(_sqlExpressionFactory.Case(
                        caseExpression.WhenClauses.Union(nestedCaseExpression.WhenClauses).ToList(),
                        nestedCaseExpression.ElseResult));
                }
            }
            return base.VisitExtension(node);
        }

    }
}
