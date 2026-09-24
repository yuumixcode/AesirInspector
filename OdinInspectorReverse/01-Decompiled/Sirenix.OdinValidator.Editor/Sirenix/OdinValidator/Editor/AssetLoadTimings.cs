using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sirenix.OdinValidator.Editor
{
	public struct AssetLoadTimings : IDisposable
	{
		public struct LoadTimeData
		{
			public string AssetPath;

			public DateTime LoadDate;

			public double LoadMs;

			public LoadTimeData(string assetPath, DateTime loadDate, double loadMs)
			{
				AssetPath = assetPath;
				LoadDate = loadDate;
				LoadMs = loadMs;
			}
		}

		public static int MaxTimings = 50;

		public static List<LoadTimeData> Timings = new List<LoadTimeData>();

		public string AssetPath;

		public DateTime LoadDate;

		public Stopwatch Stopwatch;

		public static AssetLoadTimings Time(string assetPath)
		{
			return new AssetLoadTimings
			{
				AssetPath = assetPath,
				LoadDate = DateTime.Now,
				Stopwatch = Stopwatch.StartNew()
			};
		}

		public void Dispose()
		{
			if (Stopwatch != null)
			{
				Stopwatch.Stop();
				RegisterTiming(AssetPath, LoadDate, Stopwatch.Elapsed.TotalMilliseconds);
			}
		}

		public static void RegisterTiming(string path, DateTime date, double ms)
		{
			bool inserted = false;
			for (int i = 0; i < Timings.Count; i++)
			{
				if (ms > Timings[i].LoadMs)
				{
					inserted = true;
					Timings.Insert(i, new LoadTimeData(path, date, ms));
					break;
				}
			}
			if (!inserted && Timings.Count < MaxTimings)
			{
				Timings.Add(new LoadTimeData(path, date, ms));
				return;
			}
			while (Timings.Count > MaxTimings)
			{
				Timings.RemoveAt(Timings.Count - 1);
			}
		}
	}
}
