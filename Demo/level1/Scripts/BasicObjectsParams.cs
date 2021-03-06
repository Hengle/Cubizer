﻿namespace Cubizer
{
	public class NoiseParams
	{
		public int octaves = 6;
		public float loopX = 1.0f;
		public float loopZ = 1.0f;
		public float persistence = 0.5f;
		public float lacunarity = 2.0f;
		public float threshold = 0.84f;
	}

	public class BasicObjectsParams
	{
		public BasicObjectBiomeType layer;

		public bool isGenTree = true;
		public bool isGenWater = true;
		public bool isGenCloud = true;
		public bool isGenFlower = true;
		public bool isGenWeed = true;
		public bool isGenGrass = true;
		public bool isGenObsidian = true;
		public bool isGenSoil = true;
		public bool isGenSand = false;

		public int floorBase = 5;
		public int floorHeightLismit = 15;

		public float thresholdSand = 0.5f;
		public float thresholdCloud = 0.75f;

		public NoiseParams weeds = new NoiseParams { octaves = 4, loopX = -0.1f, loopZ = 0.1f, lacunarity = 2.0f, persistence = 0.8f, threshold = 0.7f };
		public NoiseParams flowers = new NoiseParams { octaves = 4, loopX = 0.05f, loopZ = -0.05f, lacunarity = 2.0f, persistence = 0.8f, threshold = 0.75f };
		public NoiseParams grass = new NoiseParams { octaves = 4, loopX = 0.008f, loopZ = 0.008f, lacunarity = 0.2f, persistence = 0.3f, threshold = 0.84f };
		public NoiseParams tree = new NoiseParams { octaves = 6, loopX = 1.0f, loopZ = 1.0f, lacunarity = 2.0f, persistence = 0.5f, threshold = 0.84f };
	}
}