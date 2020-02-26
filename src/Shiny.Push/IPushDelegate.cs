﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Shiny.Push
{
    public interface IPushDelegate
    {
        Task OnReceived(IDictionary<string, string> data);
        Task OnTokenChanged(string token);
    }
}
