﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler_serveme.Core.Interfaces.Manager;
using Crawler_serveme.Core.Interfaces.Repository;

namespace Crawler_serveme.BL.Manager
{
    public class BookingComManager : IBookingComManager
    {
        private readonly IRepository _repository;

        public BookingComManager(IRepository repository)
        {
            _repository = repository;
        }

        public void GetInfoBookingCom(string folder)
        {
            
        }
    }
}
