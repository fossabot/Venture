﻿using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class CreateJoinRoomOutput
    {
        [ProtoMember(1)]
        public string RoomId { get; set; }

        [ProtoMember(2)]
        public string JoinKey { get; set; }

        [ProtoMember(3)]
        public ServerEndPoint[] Endpoints { get; set; }
    }
}
