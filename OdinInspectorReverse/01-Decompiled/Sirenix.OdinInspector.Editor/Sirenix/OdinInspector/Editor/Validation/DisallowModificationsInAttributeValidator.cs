namespace Sirenix.OdinInspector.Editor.Validation
{
	public class DisallowModificationsInAttributeValidator : AttributeValidator<DisallowModificationsInAttribute>
	{
		protected override void Validate(ValidationResult result)
		{
			PrefabKind kind = OdinPrefabUtility.GetPrefabKind(base.Property);
			if ((kind & base.Attribute.PrefabKind) != PrefabKind.None && base.Property.ValueEntry.ValueChangedFromPrefab)
			{
				result.AddError($"Modifications on '{base.Attribute.PrefabKind}' for {base.Property.NiceName} are not allowed.");
			}
		}
	}
}
