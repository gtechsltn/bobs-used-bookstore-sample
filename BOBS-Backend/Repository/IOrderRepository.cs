﻿using BOBS_Backend.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BOBS_Backend.Repository
{
    public interface IOrderRepository
    {
        Task<Order> FindOrderById(long id);

        Task<List<Order>> GetAllOrders();

        Task<List<Order>> FilterList(string filterValue, string searchString);

        Task<List<Order>> FilterOrderByOrderId(string searchString);

        Task<List<Order>> FilterOrderByCustomerId(string searchString);

        Task<List<Order>> FilterOrderByEmail(string searchString);

        Task<List<Order>> FilterOrderByState(string searchString);
    }
}