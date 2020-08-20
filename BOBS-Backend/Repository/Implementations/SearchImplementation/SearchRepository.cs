﻿using BOBS_Backend.Database;
using BOBS_Backend.Repository.SearchImplementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BOBS_Backend.Repository.Implementations.SearchImplementation
{


    public class SearchRepository : ISearchRepository
    {

        private DatabaseContext _context;

        public SearchRepository(DatabaseContext context)
        {
            _context = context;
        }

        public int[] GetModifiedPagesArr(int pageNum, int totalPages)
        {
            int[] pages = null;

            var start = pageNum / 10;

            bool Noremainder = pageNum % 10 == 0;


            if (start < (totalPages / 10) || Noremainder == true)
            {
                pages = Enumerable.Range((Noremainder) ? ((start - 1) * 10 + 1) : (start * 10 + 1), 10).ToArray();
            }
            else
            {
                pages = Enumerable.Range((Noremainder) ? ((start - 1) * 10) : (start * 10 + 1), totalPages - (start * 10)).ToArray();
            }

            return pages;
        }

        public int GetTotalPages(int totalCount, int valsPerPage)
        {
            if ((totalCount % valsPerPage) == 0)
            {
                return (totalCount / valsPerPage);
            }
            else return (totalCount / valsPerPage) + 1;
        }

        public IQueryable GetBaseQuery(string objPath)
        {
            var query = _context.Query(objPath);

            return query;
        }

        private MethodCallExpression GenerateExpressionObject(string type, string subSearch, MethodInfo method, MemberExpression property, bool isEntire)
        {
            try
            {
                ConstantExpression constant = null;
                if (type == "System.Int64")
                {
                    long value = 0;

                    bool res = long.TryParse(subSearch, out value);

                    constant = Expression.Constant(value);
                    method = typeof(long).GetMethod("Equals", new Type[] { typeof(int) });
                }
                else
                {
                    constant = Expression.Constant(subSearch);
                    method = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
                }

                var expression = (isEntire == true) ? Expression.Call(constant, method, property) : Expression.Call(property, method, constant);
                return expression;
            }
            catch
            {
                return null;
            }

        }

        private BinaryExpression GenerateDynamicLambdaFunctionObjectProperty(string splitFilter, ParameterExpression parameterExpression, string searchString)
        {
            var property = Expression.Property(parameterExpression, splitFilter);

            BinaryExpression lambda = null;
            MethodInfo method = null;
            bool isFirst = true;
            searchString = searchString.Trim();

            var table = (IQueryable)_context.GetType().GetProperty("Order").GetValue(_context, null);

            var row = Expression.Parameter(table.ElementType, "row");

            var col = Expression.Property(row, splitFilter);

            var type = col.Type.FullName;

            foreach (var subSearch in searchString.Split(' '))
            {
                try
                {

                    var expression = GenerateExpressionObject(type, subSearch, method, property, false);


                    if (isFirst)
                    {
                        lambda = Expression.Or(expression, expression);
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
            }

            //var exp2 = GenerateExpressionOrder(type, searchString, method, property, true);

            //lambda = (isFirst == true) ? Expression.Or(exp2, exp2) : Expression.Or(lambda, exp2);

            return lambda;

        }

        private MethodCallExpression GenerateExpressionSubObject(string type, string subSearch, MethodInfo method, string[] splitFilter, ParameterExpression parameterExpression, bool isEntire)
        {
            try
            {
                ConstantExpression constant = null;
                if (type == "System.Int64")
                {
                    long value = 0;

                    bool res = long.TryParse(subSearch, out value);

                    constant = Expression.Constant(value);

                    method = typeof(long).GetMethod("Equals", new Type[] { typeof(int) });
                }
                else
                {
                    constant = Expression.Constant(subSearch);
                    method = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
                }

                Expression property2 = parameterExpression;

                foreach (var member in splitFilter)
                {
                    property2 = Expression.PropertyOrField(property2, member);
                }

                var expression = (isEntire == true) ? Expression.Call(constant, method, property2) : Expression.Call(property2, method, constant);

                return expression;
            }
            catch
            {
                return null;
            }

        }

        private BinaryExpression GenerateDynamicLambdaFunctionSubObjectProperty(string[] splitFilter, ParameterExpression parameterExpression, string searchString)
        {
            BinaryExpression lambda = null;
            MethodInfo method = null;
            bool isFirst = true;
            searchString = searchString.Trim();

            var table = (IQueryable)_context.GetType().GetProperty(splitFilter[0]).GetValue(_context, null);

            var row = Expression.Parameter(table.ElementType, "row");

            var col = Expression.Property(row, splitFilter[1]);

            var type = col.Type.FullName;
            foreach (var subSearch in searchString.Split(' '))
            {
                try
                {


                    var expression = GenerateExpressionSubObject(type, subSearch, method, splitFilter, parameterExpression, false);

                    if (isFirst)
                    {
                        lambda = Expression.Or(expression, expression);
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
            }

            //var exp2 = GenerateExpressionSubOrder(type, searchString, method, splitFilter, parameterExpression, true);

            //lambda = (isFirst == true) ? Expression.Or(exp2, exp2) : Expression.Or(lambda, exp2); 
            return lambda;

        }

        public BinaryExpression ReturnExpression(ParameterExpression parameterExpression, string filterValue, string searchString)
        {
            string[] listOfFilters = filterValue.Split(' ');
            bool isFirst = true;
            BinaryExpression expression = null;

            for (int i = 1; i < listOfFilters.Length; i++)
            {

                BinaryExpression exp2 = null;

                if (!listOfFilters[i].Contains("."))
                {
                    exp2 = GenerateDynamicLambdaFunctionObjectProperty(listOfFilters[i], parameterExpression, searchString);
                }
                else
                {

                    exp2 = GenerateDynamicLambdaFunctionSubObjectProperty(listOfFilters[i].Split("."), parameterExpression, searchString);



                }

                if (exp2 == null)
                {
                    continue;
                }
                if (isFirst)
                {
                    expression = Expression.And(exp2, exp2);
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
    }
}