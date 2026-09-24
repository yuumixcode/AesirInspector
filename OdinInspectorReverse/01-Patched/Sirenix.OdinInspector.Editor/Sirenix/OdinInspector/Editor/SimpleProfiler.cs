using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector.Editor
{
	internal struct SimpleProfiler : IDisposable
	{
		public string Name;

		public Stopwatch Watch;

		public static SimpleProfiler Section(string name)
		{
			return default(SimpleProfiler);
		}

		public void Dispose()
		{
		}
	}
}
