﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace PlayerIOClient
{
    public class Multiplayer
    {
        /// <summary>
        /// If set, rooms will be created on the development server at the address defined by the endpoint specified, instead of using the live Player.IO servers.
        /// </summary>
        public DevelopmentServer DevelopmentServer = null;

        /// <summary>
        /// If true, the multiplayer connections will be encrypted using TLS/SSL. Beaware that this will cause a performance degredation by introducting secure connection negotiation latency.
        /// </summary>
        public bool UseSecureConnections { get; set; }

        internal Multiplayer(PlayerIOChannel channel)
        {
            this.Channel = channel;
        }

        /// <summary> Create a multiplayer room on the Player.IO infrastructure. </summary>
        /// <param name="roomId"> The ID you wish to assign to your new room - You can use this to connect to the specific room later as long as it still exists. </param>
        /// <param name="roomType"> The name of the room type you wish to run the room as. This value should match one of the [RoomType(...)] attributes of your uploaded code. A room type of 'bounce' is always available. </param>
        /// <param name="visible"> Whether the room should be visible when listing rooms with ListRooms. </param>
        /// <param name="roomData"> The data to initialize the room with, this can be read with ListRooms and changed from the serverside. </param>
        /// <returns> The ID of the room that was created. </returns>
        public string CreateRoom(string roomId, string roomType, bool visible, Dictionary<string, string> roomData)
        {
            var (success, response, error) = this.Channel.Request<CreateRoomArgs, CreateRoomOutput>(21, new CreateRoomArgs
            {
                RoomId = roomId,
                RoomType = roomType,
                Visible = visible,
                RoomData = DictionaryEx.Convert(roomData),
                IsDevRoom = this.DevelopmentServer != null
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return response.RoomId;
        }

        /// <summary> Join a running multiplayer room. </summary>
        /// <param name="roomId"> The ID of the room you wish to join. </param>
        /// <param name="joinData"> Data to send to the room with additional information about the join. </param>
        /// <param name="successCallback"> A callback called when you successfully created and joined the room. </param>
        /// <param name="errorCallback"> A callback called instead of <paramref name="successCallback"/> if an error occurs when joining the room. </param>
        public Connection JoinRoom(string roomId, Dictionary<string, string> joinData = null)
        {
            var (success, response, error) = this.Channel.Request<JoinRoomArgs, JoinRoomOutput>(24, new JoinRoomArgs
            {
                RoomId = roomId,
                JoinData = DictionaryEx.Convert(joinData),
                IsDevRoom = this.DevelopmentServer != null
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            var endpoints = response.Endpoints.ToList();

            foreach (var endpoint in endpoints)
            {
                if (PortCheck.IsPortOpen(endpoint.Address, endpoint.Port, 1000, 3))
                {
                    var resolution = Dns.GetHostEntry(endpoint.Address).AddressList[0];

                    return new Connection(new IPEndPoint(resolution, endpoint.Port), response.JoinKey);
                }
            }

            throw new PlayerIOError(ErrorCode.GeneralError, "[Venture] Unable to join room - unable to establish connection from any endpoint(s) returned by API.");
        }

        /// <summary> Creates a multiplayer room (if it doesn't exists already), and joins it. </summary>
        /// <param name="roomId"> The ID of the room you wish to (create and then) join. </param>
        /// <param name="serverType">
        /// If the room doesn't exists: The name of the room type you wish to run the room as. This
        /// value should match one of the 'RoomType(...)' attributes of your uploaded code. A room
        /// type of 'bounce' is always available.
        /// </param>
        /// <param name="visible">
        /// If the room doesn't exists: Determines (upon creation) if the room should be visible when
        /// listing rooms with ListRooms.
        /// </param>
        /// <param name="roomData">
        /// If the room doesn't exists: The data to initialize the room with (upon creation).
        /// </param>
        /// <param name="joinData">
        /// Data to send to the room with additional information about the join.
        /// </param>
        public Connection CreateJoinRoom(string roomId, string serverType, bool visible = true, Dictionary<string, string> roomData = null, Dictionary<string, string> joinData = null)
        {
            var (success, response, error) = this.Channel.Request<CreateJoinRoomArgs, CreateJoinRoomOutput>(27, new CreateJoinRoomArgs
            {
                RoomId = roomId,
                ServerType = serverType,
                Visible = visible,
                RoomData = roomData,
                JoinData = joinData,
                IsDevRoom = this.DevelopmentServer != null
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            if (this.DevelopmentServer != null)
            {
                return new Connection(new IPEndPoint(Dns.GetHostEntry(this.DevelopmentServer.Address).AddressList[0], this.DevelopmentServer.Port), response.JoinKey);
            }
            else
            {
                var endpoints = response.Endpoints.ToList();

                foreach (var endpoint in endpoints)
                {
                    if (PortCheck.IsPortOpen(endpoint.Address, endpoint.Port, 1000, 3))
                    {
                        var resolution = Dns.GetHostEntry(endpoint.Address).AddressList[0];

                        return new Connection(new IPEndPoint(resolution, endpoint.Port), response.JoinKey);
                    }
                }
            }

            throw new PlayerIOError(ErrorCode.GeneralError, "[Venture] Unable to join room - unable to establish connection from any endpoint(s) returned by API.");
        }

        private PlayerIOChannel Channel { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }
}