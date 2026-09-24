using System;

namespace Sirenix.OdinValidator.Editor
{
	public class ValidationSessionAssetHandle : IDisposable
	{
		public readonly ValidationProfile Asset;

		private bool isAquired;

		public bool IsDisposed { get; private set; }

		public ValidationSession Session
		{
			get
			{
				AquireSession();
				return Asset.session;
			}
		}

		public ValidationSessionAssetHandle(ValidationProfile asset)
		{
			Asset = asset;
		}

		public void AquireSession()
		{
			if (IsDisposed)
			{
				throw new NullReferenceException();
			}
			if (!isAquired)
			{
				if (Asset.handles.Count == 0)
				{
					SessionConfig config = new SessionConfig();
					config.SessionData.Add(Asset);
					config.SessionData.Add(MainLocalValidationProfile.Instance);
					Asset.session = new ValidationSession(Asset.name, config);
				}
				Asset.handles.Add(this);
				isAquired = true;
			}
		}

		public void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}
			IsDisposed = true;
			if (Asset.handles.Remove(this) && Asset.handles.Count == 0)
			{
				if (!Asset.session.IsDisposed)
				{
					Asset.session.Dispose();
				}
				Asset.session = null;
			}
		}
	}
}
