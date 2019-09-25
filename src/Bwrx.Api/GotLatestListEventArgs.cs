using System;
using System.Collections.Generic;

namespace Bwrx.Api
{
    public class GotLatestListEventArgs : EventArgs
    {
        public GotLatestListEventArgs(int numIpAddresses)
        {
            NumIpAddresses = numIpAddresses;
        }

        public GotLatestListEventArgs(int numIpAddresses, Dictionary<string, int> regionCounts)
        {
            NumIpAddresses = numIpAddresses;
            RegionCounts = regionCounts;
        }

        public int NumIpAddresses { get; }
        public Dictionary<string, int> RegionCounts { get; }
    }
}