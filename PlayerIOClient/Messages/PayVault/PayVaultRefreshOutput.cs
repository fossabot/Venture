﻿using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultRefreshOutput
    {
        [ProtoMember(1)]
        public PayVaultContents PayVaultContents { get; set; }
    }
}