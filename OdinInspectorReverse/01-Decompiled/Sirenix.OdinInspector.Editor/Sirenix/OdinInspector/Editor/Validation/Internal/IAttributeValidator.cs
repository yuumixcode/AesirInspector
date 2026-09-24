using System;

namespace Sirenix.OdinInspector.Editor.Validation.Internal
{
	public interface IAttributeValidator
	{
		Type AttributeType { get; }

		int AttributeNumber { get; }

		void SetAttributeInstanceAndNumber(Attribute attribute, int number);
	}
}
