﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Cubizer.Chunk;
using Cubizer.Net.Client;
using Cubizer.Net.Protocol;
using Cubizer.Net.Protocol.Login.Serverbound;
using Cubizer.Net.Protocol.Play.Serverbound;
using Cubizer.Net.Protocol.Handshakes.Serverbound;

using UnityEngine;

namespace Cubizer.Net
{
	public sealed class ClientComponent : CubizerComponent<NetworkModels>
	{
		private Task _task;
		private ClientSession _client;
		private ClientPacketRouter _clientRouter = new ClientPacketRouter();
		private CancellationTokenSource _cancellationToken;

		public override bool active
		{
			get
			{
				return model.enabled;
			}
			set
			{
				if (model.enabled != value)
				{
					if (value)
						this.OnEnable();
					else
						this.OnDisable();

					model.enabled = value;
				}
			}
		}

		public bool isCancellationRequested
		{
			get
			{
				return _cancellationToken != null ? _cancellationToken.IsCancellationRequested : true;
			}
		}

		public ClientComponent()
		{
			_clientRouter.onDispatchIncomingPacket = this.OnDispatchIncomingPacket;
			_clientRouter.onDispatchInvalidPacket = this.OnDispatchInvalidPacket;
		}

		public override void OnEnable()
		{
			context.behaviour.events.OnLoadChunkAfter += this.OnLoadChunkDataAfter;
			context.behaviour.events.OnAddBlockAfter += this.OnAddBlockAfter;
			context.behaviour.events.OnRemoveBlockAfter += this.OnRemoveBlockAfter;
		}

		public override void OnDisable()
		{
			context.behaviour.events.OnLoadChunkAfter -= this.OnLoadChunkDataAfter;
			context.behaviour.events.OnAddBlockAfter -= this.OnAddBlockAfter;
			context.behaviour.events.OnRemoveBlockAfter -= this.OnRemoveBlockAfter;
		}

		public bool Connect()
		{
			if (isCancellationRequested)
			{
				_cancellationToken = new CancellationTokenSource();

				try
				{
					_client = new ClientSession(model.settings.client.address, model.settings.client.port, _clientRouter);
					_client.sendTimeout = model.settings.client.sendTimeOut;
					_client.receiveTimeout = model.settings.client.receiveTimeout;
					_client.onStartClientListener = OnStartClientListener;
					_client.onStopClientListener = OnStopClientListener;

					if (!_client.Connect())
					{
						_cancellationToken.Cancel();
						return false;
					}

					_client.Start(_cancellationToken.Token);

					_clientRouter.status = SessionStatus.Login;

					_client.SendOutcomingPacket(new Handshake(model.settings.network.version, model.settings.client.address, model.settings.client.port, SessionStatus.Login));
					_client.SendOutcomingPacket(new LoginStart("test"));

					return _client.connected;
				}
				catch (Exception e)
				{
					_cancellationToken.Cancel();
					_cancellationToken = null;
					throw e;
				}
			}
			else
			{
				throw new InvalidOperationException("A client has already working now.");
			}
		}

		public void Disconnect()
		{
			if (_cancellationToken != null)
			{
				_cancellationToken.Token.Register(_client.Close);
				_cancellationToken.Cancel();
				_cancellationToken = null;
			}
		}

		private void OnLoadChunkDataAfter(ChunkPrimer chunk)
		{
		}

		private void OnAddBlockAfter(ChunkPrimer chunk, int x, int y, int z, VoxelMaterial voxel)
		{
		}

		private void OnRemoveBlockAfter(ChunkPrimer chunk, int x, int y, int z, VoxelMaterial voxel)
		{
		}

		private void OnStartClientListener()
		{
			Debug.Log("Starting client listener...");
		}

		private void OnStopClientListener()
		{
			Debug.Log("Stop client listener...");

			if (_cancellationToken != null)
			{
				_cancellationToken.Cancel();
				_cancellationToken = null;
			}
		}

		private void OnDispatchInvalidPacket(SessionStatus status, UncompressedPacket packet)
		{
			UnityEngine.Debug.Log($"Invalid Packet: {packet.packetId}.Length:[{packet.data.Count}byte]");
		}

		private void OnDispatchIncomingPacket(SessionStatus status, IPacketSerializable packet)
		{
			if (packet.GetType() == typeof(Protocol.Login.Clientbound.LoginSuccess))
			{
				_clientRouter.status = SessionStatus.Play;
			}
			else if (packet.GetType() == typeof(Protocol.Play.Clientbound.KeepAlive))
			{
				_client.SendOutcomingPacket(new KeepAlive { keepAliveID = (uint)new System.Random().Next() });
			}
			else if (packet.GetType() == typeof(Protocol.Play.Clientbound.TimeUpdate))
			{
			}

			UnityEngine.Debug.Log($"Packet：{packet.GetType().Name}");
		}
	}
}