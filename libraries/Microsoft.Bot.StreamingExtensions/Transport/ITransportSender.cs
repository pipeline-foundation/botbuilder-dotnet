﻿using System.Threading.Tasks;

namespace Microsoft.Bot.StreamingExtensions.Transport
{
    internal interface ITransportSender : ITransport
    {
        Task<int> SendAsync(byte[] buffer, int offset, int count);
    }
}
