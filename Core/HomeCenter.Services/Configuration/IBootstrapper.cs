﻿using HomeCenter.Model.Core;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Services.Configuration
{
    public interface IBootstrapper : IDisposable
    {
        Task<Controller> BuildController();
    }
}