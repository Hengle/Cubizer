﻿using Cubizer.Net.Protocol.Serialization;

namespace Cubizer.Net.Protocol.Status.Serverbound
{
	[Packet(Packet)]
	public sealed class Ping : IPacketSerializable
	{
		public const int Packet = 0x01;

		public long payload;

		public uint packetId
		{
			get
			{
				return Packet;
			}
		}

		public void Deserialize(NetworkReader br)
		{
			payload = br.ReadInt64();
		}

		public void Serialize(NetworkWrite bw)
		{
			bw.Write(payload);
		}

		public object Clone()
		{
			return new Ping();
		}
	}
}