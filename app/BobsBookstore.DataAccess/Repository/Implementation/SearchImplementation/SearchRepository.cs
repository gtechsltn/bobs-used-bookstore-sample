﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using BobsBookstore.DataAccess.Repository.Interface.SearchImplementations;

namespace BobsBookstore.DataAccess.Repository.Implementation.SearchImplementation
{
    public class SearchRepository : ISearchRepository
    {
        private readonly ISearchDatabaseCalls _searchDbCalls;
        private readonly int pagesPerPage = 10;

        public SearchRepository(ISearchDatabaseCalls searchDatabaseCalls)
        {
            _searchDbCalls = searchDatabaseCalls;
        }

        public int[] GetModifiedPagesArr(int pageNum, int totalPages)
        {
            var start = pageNum / pagesPerPage;

            var noRemainder = pageNum % pagesPerPage == 0;

            int[] pages;
            if (start < totalPages / pagesPerPage || noRemainder)
                pages = Enumerable.Range(noRemainder ? (start - 1) * pagesPerPage + 1 : start * pagesPerPage + 1,
                    pagesPerPage).ToArray();
            else
                pages = Enumerable.Range(noRemainder ? (start - 1) * pagesPerPage : start * pagesPerPage + 1,
                    totalPages - start * pagesPerPage).ToArray();

            return pages;
        }

        public int GetTotalPages(int totalCount, int valsPerPage)
        {
            if (totalCount % valsPerPage == 0)
                return totalCount / valsPerPage;
            return totalCount / valsPerPage + 1;
        }

        public BinaryExpression ReturnExpression(ParameterExpression parameterExpression, string filterValue,
            string searchString)
        {
            var listOfFilters = filterValue.Split(' ');
            var isFirst = true;
            BinaryExpression expression = null;

            for (var i = 1; i < listOfFilters.Length; i++)
            {
                BinaryExpression exp2 = null;

                if (!listOfFilters[i].Contains("."))
                    exp2 = GenerateDynamicLambdaFunctionObjectProperty(listOfFilters[i], parameterExpression,
                        searchString);
                else
                    exp2 = GenerateDynamicLambdaFunctionSubObjectProperty(listOfFilters[i].Split("."),
                        parameterExpression, searchString);

                if (exp2 == null) continue;

                if (isFirst)
                {
                    expression = exp2;
                    isFirst = false;
                }
                else
                {
                    expression = Expression.And(expression, exp2);
                    isFirst = false;
                }
            }

            return expression;
        }

        private BinaryExpression PerformArtithmeticExpresion(string operand, Expression property,
            ConstantExpression constant)
        {
            if (operand.Equals(">")) return Expression.GreaterThan(property, constant);
            if (operand.Equals("==")) return Expression.Equal(property, constant);
            if (operand.Equals("<")) return Expression.LessThan(property, constant);

            return Expression.Equal(property, constant);
        }

        private BinaryExpression GenerateExpressionObject(string type, string subSearch, MemberExpression property,
            bool isEntire)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(Type.GetType(type));

                var test = converter.ConvertFrom(subSearch);
                ConstantExpression constant = null;
                if (type == "System.Int64")
                {
                    long value = 0;

                    var res = long.TryParse(subSearch, out value);

                    constant = Expression.Constant(test);

                    return PerformArtithmeticExpresion("==", property, constant);
                }

                constant = Expression.Constant(subSearch);
                var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                var expression = Expression.Call(property, method, constant);

                return Expression.Or(expression, expression);
            }
            catch
            {
                return null;
            }
        }

        private BinaryExpression GenerateDynamicLambdaFunctionObjectProperty(string splitFilter,
            ParameterExpression parameterExpression, string searchString)
        {
            var property = Expression.Property(parameterExpression, splitFilter);

            BinaryExpression lambda = null;
            var isFirst = true;
            searchString = searchString.Trim();

            var table = _searchDbCalls.GetTable("Order");

            var row = Expression.Parameter(table.ElementType, "row");

            var col = Expression.Property(row, splitFilter);

            var type = col.Type.FullName;

            foreach (var subSearch in searchString.Split(' '))
                try
                {
                    var expression = GenerateExpressionObject(type, subSearch, property, false);
                    if (isFirst)
                    {
                        lambda = expression;
                        isFirst = false;
                    }
                    else
                    {
                        lambda = Expression.Or(lambda, expression);
                        isFirst = false;
                    }
                }
                catch
                {
                }

            return lambda;
        }

        private BinaryExpression GenerateExpressionSubObject(string type, string subSearch, string[] splitFilter,
            ParameterExpression parameterExpression, bool isEntire)
        {
            try
            {
                ConstantExpression constant = null;
                if (type == "System.Int64")
                {
                    long value = 0;

                    var res = long.TryParse(subSearch, out value);

                    constant = Expression.Constant(value);

                    Expression property2 = parameterExpression;

                    foreach (var member in splitFilter) property2 = Expression.PropertyOrField(property2, member);

                    var expression = PerformArtithmeticExpresion("==", property2, constant);
                    return expression;
                }
                else
                {
                    constant = Expression.Constant(subSearch);
                    var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                    Expression property2 = parameterExpression;

                    foreach (var member in splitFilter) property2 = Expression.PropertyOrField(property2, member);

                    var expression = isEntire
                        ? Expression.Call(constant, method, property2)
                        : Expression.Call(property2, method, constant);

                    return Expression.Or(expression, expression);
                }
            }
            catch
            {
                return null;
            }
        }

        private BinaryExpression GenerateDynamicLambdaFunctionSubObjectProperty(string[] splitFilter,
            ParameterExpression parameterExpression, string searchString)
        {
            BinaryExpression lambda = null;

            var isFirst = true;
            searchString = searchString.Trim();

            var table = _searchDbCalls.GetTable(splitFilter[0]);

            var row = Expression.Parameter(table.ElementType, "row");

            var col = Expression.Property(row, splitFilter[1]);

            var type = col.Type.FullName;
            foreach (var subSearch in searchString.Split(' '))
                try
                {
                    var expression =
                        GenerateExpressionSubObject(type, subSearch, splitFilter, parameterExpression, false);

                    if (isFirst)
                    {
                        lambda = expression;
                        isFirst = false;
                    }
                    else
                    {
                        lambda = Expression.Or(lambda, expression);
                        isFirst = false;
                    }
                }
                catch
                {
                }

            return lambda;
        }
    }
}