﻿using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace Cubizer
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Cubizer/CubizerBehaviour")]
	public class CubizerBehaviour : MonoBehaviour
	{
		[SerializeField]
		private CubizerProfile _profile;
		private CubizerContext _context;

		private LiveManagerComponent _lives;
		private ChunkManagerComponent _chunkManager;
		private BiomeManagerComponent _biomeManager;
		private DbComponent _database;
		private ServerComponent _server;
		private ClientComponent _client;

		private readonly List<ICubizerComponent> _components = new List<ICubizerComponent>();

		private readonly PlayerManagerModel _players = new PlayerManagerModel();
		private readonly CubizerDelegates _events = new CubizerDelegates();
		private readonly static IVoxelMaterialManager _materialFactory = VoxelMaterialManager.GetInstance();

		public CubizerProfile profile
		{
			get { return _profile; }
		}

		public CubizerDelegates events
		{
			get { return _events; }
		}

		public BiomeManagerComponent biomeManager
		{
			get { return _biomeManager; }
		}

		public ChunkManagerComponent chunkManager
		{
			get { return _chunkManager; }
		}

		public ServerComponent server
		{
			get { return _server; }
		}

		public ClientComponent client
		{
			get { return _client; }
		}

		public void Connection(IPlayer player)
		{
			if (player != null)
			{
				_players.settings.players.Add(player);

				if (this.events.OnPlayerConnection != null)
					this.events.OnPlayerConnection(player);
			}
			else
			{
				throw new System.ArgumentNullException("Connection() fail");
			}
		}

		public void Disconnect(IPlayer player)
		{
			if (player != null)
			{
				_players.settings.players.Remove(player);

				if (this.events.OnPlayerDisconnect != null)
					this.events.OnPlayerDisconnect(player);
			}
			else
			{
				throw new System.ArgumentNullException("Disconnect() fail");
			}
		}

		private void OnLoadChunkDataBefore(int x, int y, int z, ref ChunkPrimer chunk)
		{
			if (this.events.OnLoadChunkBefore != null)
				this.events.OnLoadChunkBefore(x, y, z, ref chunk);
		}

		private void OnLoadChunkDataAfter(ChunkPrimer chunk)
		{
			if (this.events.OnLoadChunkAfter != null)
				this.events.OnLoadChunkAfter(chunk);
		}

		private void OnDestroyChunkData(ChunkPrimer chunk)
		{
			if (this.events.OnDestroyChunk != null)
				this.events.OnDestroyChunk(chunk);
		}

		private void OnAddBlockBefore(ChunkPrimer chunk, int x, int y, int z, VoxelMaterial voxel)
		{
			if (this.events.OnAddBlockBefore != null)
				this.events.OnAddBlockBefore(chunk, x, y, z, voxel);
		}

		private void OnAddBlockAfter(ChunkPrimer chunk, int x, int y, int z, VoxelMaterial voxel)
		{
			if (this.events.OnAddBlockAfter != null)
				this.events.OnAddBlockAfter(chunk, x, y, z, voxel);
		}

		private void OnRemoveBlockBefore(ChunkPrimer chunk, int x, int y, int z, VoxelMaterial voxel)
		{
			if (this.events.OnRemoveBlockBefore != null)
				this.events.OnRemoveBlockBefore(chunk, x, y, z, voxel);
		}

		private void OnRemoveBlockAfter(ChunkPrimer chunk, int x, int y, int z, VoxelMaterial voxel)
		{
			if (this.events.OnRemoveBlockAfter != null)
				this.events.OnRemoveBlockAfter(chunk, x, y, z, voxel);
		}

		private void EnableComponents()
		{
			foreach (var component in _components)
			{
				var model = component.GetModel();
				if (component.Active && model != null)
					component.OnEnable();
			}
		}

		private void DisableComponents()
		{
			foreach (var component in _components)
			{
				var model = component.GetModel();
				if (component.Active && model != null)
					component.OnDisable();
			}
		}

		private void UpdateComponents()
		{
			foreach (var component in _components)
			{
				var model = component.GetModel();
				if (component.Active && model != null)
					component.Update();
			}
		}

		private T AddComponent<T>(T component) where T : ICubizerComponent
		{
			Debug.Assert(component != null);
			_components.Add(component);
			return component;
		}

		private bool RemoveComponent<T>(T component) where T : ICubizerComponent
		{
			Debug.Assert(component != null);
			return _components.Remove(component);
		}

		private IEnumerator UpdateComponentsWithCoroutine()
		{
			yield return new WaitForSeconds(profile.terrain.settings.repeatRate);

			UpdateComponents();

			StartCoroutine("UpdateComponentsWithCoroutine");
		}

		public void Start()
		{
			if (_profile == null)
				Debug.LogError("Please drag a CubizerProfile into Inspector.");

			Debug.Assert(_profile.chunk.settings.chunkSize > 0);

			_context = new CubizerContext();
			_context.profile = _profile;
			_context.behaviour = this;
			_context.materialFactory = _materialFactory;
			_context.players = _players;

			_lives = AddComponent(new LiveManagerComponent());
			_lives.Init(_context, _profile.lives);

			_chunkManager = AddComponent(new ChunkManagerComponent());
			_chunkManager.Init(_context, _profile.chunk);
			_chunkManager.callbacks.OnLoadChunkBefore += this.OnLoadChunkDataBefore;
			_chunkManager.callbacks.OnLoadChunkAfter += this.OnLoadChunkDataAfter;
			_chunkManager.callbacks.OnDestroyChunk += this.OnDestroyChunkData;
			_chunkManager.callbacks.OnAddBlockBefore += this.OnAddBlockBefore;
			_chunkManager.callbacks.OnAddBlockAfter += this.OnAddBlockAfter;
			_chunkManager.callbacks.OnRemoveBlockBefore += this.OnRemoveBlockBefore;
			_chunkManager.callbacks.OnRemoveBlockAfter += this.OnRemoveBlockAfter;

			_biomeManager = AddComponent(new BiomeManagerComponent());
			_biomeManager.Init(_context, _profile.biome);

			_database = AddComponent(new DbComponent());
			_database.Init(_context, _profile.database);

			_server = AddComponent(new ServerComponent());
			_server.Init(_context, _profile.network);

			_client = AddComponent(new ClientComponent());
			_client.Init(_context, _profile.network);

			Math.Noise.simplex_seed(_profile.terrain.settings.seed);

			this.EnableComponents();

			StartCoroutine("UpdateComponentsWithCoroutine");
		}

		public void OnDestroy()
		{
			this.DisableComponents();

			_components.Clear();
			_materialFactory.Dispose();

			StopCoroutine("UpdateComponentsWithCoroutine");
		}
	}
}