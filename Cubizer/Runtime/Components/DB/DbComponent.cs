﻿using UnityEngine;
using System.Threading.Tasks;

namespace Cubizer
{
	public class DbComponent : CubizerComponent<DbModels>
	{
		private string _dbUrl;
		private string _dbName;

		private IDbManager _dbManager;

		private bool _active;

		public override bool active
		{
			get
			{
				return _active;
			}
			set
			{
				if (_active != value)
				{
					if (value)
						this.OnEnable();
					else
						this.OnDisable();

					_active = value;
				}
			}
		}

		public DbComponent()
		{
			_active = true;
		}

		public override void OnEnable()
		{
			_dbUrl = model.settings.url;
			if (string.IsNullOrEmpty(_dbUrl) || _dbUrl == "localhost")
			{
				_dbUrl = Application.persistentDataPath + "/";

				if (string.IsNullOrEmpty(_dbUrl))
					model.SetDefaultURL("localhost");
			}

			_dbName = model.settings.name;
			if (string.IsNullOrEmpty(_dbName))
			{
				_dbName = "cubizer.db";
				model.SetDefaultName(_dbName);
			}

			context.behaviour.events.OnLoadChunkAfter += this.OnLoadChunkDataAfter;
			context.behaviour.events.OnAddBlockBefore += this.OnAddBlockBefore;
			context.behaviour.events.OnRemoveBlockBefore += this.OnRemoveBlockBefore;

			_dbManager = new DbSqlite(_dbUrl + _dbName);
		}

		public override void OnDisable()
		{
			context.behaviour.events.OnLoadChunkAfter -= this.OnLoadChunkDataAfter;
			context.behaviour.events.OnAddBlockBefore -= this.OnAddBlockBefore;
			context.behaviour.events.OnRemoveBlockBefore -= this.OnRemoveBlockBefore;

			if (_dbManager != null)
			{
				_dbManager.Dispose();
				_dbManager = null;
			}
		}

		private void OnLoadChunkDataAfter(ChunkPrimer chunk)
		{
			if (chunk != null)
				_dbManager.LoadChunk(chunk, chunk.position.x, chunk.position.y, chunk.position.z);
		}

		private void OnAddBlockBefore(ChunkPrimer chunk, int x, int y, int z, VoxelMaterial voxel)
		{
			Task.Run(() =>
			{
				_dbManager.InsertBlock(chunk.position.x, chunk.position.y, chunk.position.z, x, y, z, voxel.GetInstanceID());
			});
		}

		private void OnRemoveBlockBefore(ChunkPrimer chunk, int x, int y, int z, VoxelMaterial voxel)
		{
			Task.Run(() =>
			{
				_dbManager.InsertBlock(chunk.position.x, chunk.position.y, chunk.position.z, x, y, z, 0);
			});
		}
	}
}