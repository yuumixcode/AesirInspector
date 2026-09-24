namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Draw mode of quaternion fields.
	/// </summary>
	/// <seealso cref="T:Sirenix.Utilities.Editor.SirenixEditorFields" />
	/// <seealso cref="!:Sirenix.OdinInspector.Editor.GeneralDrawerConfig" />
	public enum QuaternionDrawMode
	{
		/// <summary>
		/// Draw the quaterion as euler angles.
		/// </summary>
		Eulers,
		/// <summary>
		/// Draw the quaterion in as an angle and an axis.
		/// </summary>
		AngleAxis,
		/// <summary>
		/// Draw the quaternion as raw x, y, z and w values.
		/// </summary>
		Raw
	}
}
