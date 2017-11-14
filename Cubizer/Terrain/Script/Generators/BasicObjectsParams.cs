﻿namespace Cubizer
{
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
		public int floorHeightLismit = 10;

		public float thresholdSand = 0.5f;
		public float thresholdTree = 0.84f;
		public float thresholdCloud = 0.75f;
	}
}