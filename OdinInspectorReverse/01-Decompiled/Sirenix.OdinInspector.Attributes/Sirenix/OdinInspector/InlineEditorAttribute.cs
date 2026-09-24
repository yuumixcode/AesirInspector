using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>InlineAttribute is used on any property or field with a type that inherits from UnityEngine.Object. This includes components and assets etc.</para>
	/// </summary>
	/// <example>
	/// <code>
	/// public class InlineEditorExamples : MonoBehaviour
	/// {
	///     [DisableInInlineEditors]
	///     public Vector3 DisabledInInlineEditors;
	///
	///     [HideInInlineEditors]
	///     public Vector3 HiddenInInlineEditors;
	///
	///     [InlineEditor]
	///     public Transform InlineComponent;
	///
	///     [InlineEditor(InlineEditorModes.FullEditor)]
	///     public Material FullInlineEditor;
	///
	///     [InlineEditor(InlineEditorModes.GUIAndHeader)]
	///     public Material InlineMaterial;
	///
	///     [InlineEditor(InlineEditorModes.SmallPreview)]
	///     public Material[] InlineMaterialList;
	///
	///     [InlineEditor(InlineEditorModes.LargePreview)]
	///     public GameObject InlineObjectPreview;
	///
	///     [InlineEditor(InlineEditorModes.LargePreview)]
	///     public Mesh InlineMeshPreview;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.DisableInInlineEditorsAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.HideInInlineEditorsAttribute" />
	[AttributeUsage(AttributeTargets.All)]
	[Conditional("UNITY_EDITOR")]
	public class InlineEditorAttribute : Attribute
	{
		private bool expanded;

		/// <summary>
		/// Draw the header editor header inline.
		/// </summary>
		public bool DrawHeader;

		/// <summary>
		/// Draw editor GUI inline.
		/// </summary>
		public bool DrawGUI;

		/// <summary>
		/// Draw editor preview inline.
		/// </summary>
		public bool DrawPreview;

		/// <summary>
		/// Maximum height of the inline editor. If the inline editor exceeds the specified height, a scrollbar will appear.
		/// Values less or equals to zero will let the InlineEditor expand to its full size.
		/// </summary>
		public float MaxHeight;

		/// <summary>
		/// The size of the editor preview if drawn together with GUI.
		/// </summary>
		public float PreviewWidth = 100f;

		/// <summary>
		/// The size of the editor preview if drawn alone.
		/// </summary>
		public float PreviewHeight = 35f;

		/// <summary>
		/// If false, this will prevent the InlineEditor attribute from incrementing the InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth.
		/// This is helpful in cases where you want to draw the entire editor, and disregard attributes
		/// such as [<see cref="T:Sirenix.OdinInspector.HideInInlineEditorsAttribute" />] and [<see cref="T:Sirenix.OdinInspector.DisableInInlineEditorsAttribute" />].
		/// </summary>
		[LabelWidth(220f)]
		public bool IncrementInlineEditorDrawerDepth = true;

		/// <summary>
		/// Whether to set GUI.enabled = false when drawing an editor for an asset that is locked by source control. Defaults to true.
		/// </summary>
		[LabelWidth(220f)]
		public bool DisableGUIForVCSLockedAssets = true;

		/// <summary>
		/// How the InlineEditor attribute drawer should draw the object field.
		/// </summary>
		public InlineEditorObjectFieldModes ObjectFieldMode;

		/// <summary>
		/// Where to draw the preview.
		/// </summary>
		public PreviewAlignment PreviewAlignment = PreviewAlignment.Right;

		/// <summary>
		/// If true, the inline editor will start expanded.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "expanded", "ExpandedHasValue" })]
		public bool Expanded
		{
			get
			{
				return expanded;
			}
			set
			{
				expanded = value;
				ExpandedHasValue = true;
			}
		}

		public bool ExpandedHasValue { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.InlineEditorAttribute" /> class.
		/// </summary>
		/// <param name="inlineEditorMode">The inline editor mode.</param>
		/// <param name="objectFieldMode">How the object field should be drawn.</param>
		public InlineEditorAttribute(InlineEditorModes inlineEditorMode = InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes objectFieldMode = InlineEditorObjectFieldModes.Boxed)
		{
			ObjectFieldMode = objectFieldMode;
			switch (inlineEditorMode)
			{
			case InlineEditorModes.GUIOnly:
				DrawGUI = true;
				break;
			case InlineEditorModes.GUIAndHeader:
				DrawGUI = true;
				DrawHeader = true;
				break;
			case InlineEditorModes.GUIAndPreview:
				DrawGUI = true;
				DrawPreview = true;
				break;
			case InlineEditorModes.SmallPreview:
				expanded = true;
				DrawPreview = true;
				break;
			case InlineEditorModes.LargePreview:
				expanded = true;
				DrawPreview = true;
				PreviewHeight = 170f;
				break;
			case InlineEditorModes.FullEditor:
				DrawGUI = true;
				DrawHeader = true;
				DrawPreview = true;
				break;
			default:
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.InlineEditorAttribute" /> class.
		/// </summary>
		/// <param name="objectFieldMode">How the object field should be drawn.</param>
		public InlineEditorAttribute(InlineEditorObjectFieldModes objectFieldMode)
			: this(InlineEditorModes.GUIOnly, objectFieldMode)
		{
		}
	}
}
