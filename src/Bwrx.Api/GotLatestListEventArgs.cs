using System;

namespace Bwrx.Api
{
    public class GotLatestListEventArgs : EventArgs
    {
        public GotLatestListEventArgs(int numBlacklistedIpAddresses)
        {
            NumBlacklistedIpAddresses = numBlacklistedIpAddresses;
        }

        public int NumBlacklistedIpAddresses { get; }
    }
}