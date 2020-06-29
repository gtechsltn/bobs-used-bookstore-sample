﻿using BOBS_Backend.Models.Order;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BOBS_Backend.Repository
{
    public interface IOrderStatusRepository
    {

        Task<List<OrderStatus>> GetOrderStatuses();

        Task<OrderStatus> FindOrderStatusById(long id);

        Task<Order> UpdateOrderStatus(Order order, long Status_Id);
    }
}