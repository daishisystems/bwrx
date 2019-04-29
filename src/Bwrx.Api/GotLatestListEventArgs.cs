using System;

namespace Bwrx.Api
{
    public class GotLatestListEventArgs : EventArgs
    {
        public GotLatestListEventArgs(int numIpAddresses)
        {
            NumIpAddresses = numIpAddresses;
        }

        public int NumIpAddresses { get; }
    }
}