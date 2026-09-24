namespace Sirenix.OdinValidator.Editor
{
	internal class AverageSampler
	{
		private static int globalSampleIndexCounter;

		public double SampleSum;

		public double LatestSample;

		public int SampleCount;

		public int GlobalSampleIndex;

		public double SampleAverage => SampleSum / (double)SampleCount;

		public void Add(double sample)
		{
			LatestSample = sample;
			SampleSum += sample;
			SampleCount++;
			GlobalSampleIndex = globalSampleIndexCounter++;
		}

		public void Reset()
		{
			LatestSample = 0.0;
			SampleCount = 0;
			SampleSum = 0.0;
		}
	}
}
