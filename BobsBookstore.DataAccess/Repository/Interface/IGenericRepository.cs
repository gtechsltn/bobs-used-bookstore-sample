﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BobsBookstore.DataAccess.Repository.Interface
{
    public interface IGenericRepository<TModel> where TModel : class
    {
        TModel Get(long? id);
        TModel Get(string id);
        IEnumerable<TModel> GetAll();
        void Add(TModel entity);
        void Remove(TModel entity);
        void Update(TModel entity);
        public IEnumerable<TModel> Get(
           Expression<Func<TModel, bool>> filter = null,
           Func<IQueryable<TModel>, IOrderedQueryable<TModel>> orderBy = null,
           string includeProperties = "");
        void Save();

    }
}
