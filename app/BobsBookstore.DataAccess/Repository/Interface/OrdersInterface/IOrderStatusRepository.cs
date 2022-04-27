﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobsBookstore.Models.Orders;

namespace BobsBookstore.DataAccess.Repository.Interface.OrdersInterface
{
    public interface IOrderStatusRepository
    {
        public List<OrderStatus> GetOrderStatuses();

        public OrderStatus FindOrderStatusByName(string status);

        public OrderStatus FindOrderStatusById(long id);

        Task<Order> UpdateOrderStatus(Order order, long Status_Id);

        IQueryable<OrderStatus> FilterOrderStatus(string filterValue, string searchString, string inBetween,
            string operand, string negate);
    }
}