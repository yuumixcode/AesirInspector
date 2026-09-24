using System;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class Fix
	{
		public bool OfferInInspector = true;

		public string Title = "Fix";

		public Delegate Action;

		public Type ArgType;

		public Fix()
		{
		}

		public Fix(Action action, bool offerInInspector)
		{
			Action = action;
			OfferInInspector = offerInInspector;
		}

		public Fix(string title, Action action, bool offerInInspector)
		{
			Action = action;
			Title = title;
			OfferInInspector = offerInInspector;
		}

		public static Fix Create(Action action, bool offerInInspector = true)
		{
			return new Fix(action, offerInInspector);
		}

		public static Fix Create<T>(Action<T> fix, bool offerInInspector = true) where T : new()
		{
			Fix fix2 = new Fix();
			fix2.Action = fix;
			fix2.OfferInInspector = offerInInspector;
			fix2.ArgType = typeof(T);
			return fix2;
		}

		public static Fix Create(string title, Action action, bool offerInInspector = true)
		{
			return new Fix(title, action, offerInInspector);
		}

		public static Fix Create<T>(string title, Action<T> fix, bool offerInInspector = true) where T : new()
		{
			Fix fix2 = new Fix();
			fix2.Title = title;
			fix2.Action = fix;
			fix2.OfferInInspector = offerInInspector;
			fix2.ArgType = typeof(T);
			return fix2;
		}

		internal FixIdentifier CreateIdentifier(string name)
		{
			return new FixIdentifier(name, Action.Method);
		}

		internal FixIdentifier CreateIdentifier()
		{
			return new FixIdentifier(Action.Method);
		}

		public object CreateEditorObject()
		{
			if (ArgType != null)
			{
				return Activator.CreateInstance(ArgType);
			}
			return null;
		}
	}
}
