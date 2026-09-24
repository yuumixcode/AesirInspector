namespace Sirenix.OdinInspector.Editor
{
	public abstract class StateUpdater
	{
		private InspectorProperty property;

		private bool initialized;

		public string ErrorMessage;

		public InspectorProperty Property => property;

		public virtual bool CanUpdateProperty(InspectorProperty property)
		{
			return true;
		}

		public virtual void OnStateUpdate()
		{
		}

		public void Initialize(InspectorProperty property)
		{
			if (initialized)
			{
				return;
			}
			this.property = property;
			try
			{
				Initialize();
			}
			finally
			{
				initialized = true;
			}
		}

		protected virtual void Initialize()
		{
		}
	}
}
