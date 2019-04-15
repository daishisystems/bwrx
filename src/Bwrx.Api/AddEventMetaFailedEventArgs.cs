﻿using System;

namespace Bwrx.Api
{
    public class AddEventMetaFailedEventArgs : EventArgs
    {
        public AddEventMetaFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}