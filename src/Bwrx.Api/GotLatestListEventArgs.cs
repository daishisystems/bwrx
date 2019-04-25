using System;

namespace Bwrx.Api
{
    public class GotLatestListEventArgs : EventArgs
    {
        public GotLatestListEventArgs(int numBlacklistedIpAddresses)
        {
            NumIpAddresses = numBlacklistedIpAddresses;
        }

        public int NumIpAddresses { get; }
    }
}