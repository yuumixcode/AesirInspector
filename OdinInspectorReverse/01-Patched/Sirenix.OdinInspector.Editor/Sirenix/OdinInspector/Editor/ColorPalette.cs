using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// A color palette.
	/// </summary>
	[Serializable]
	public class ColorPalette
	{
		[SerializeField]
		[PropertyOrder(0f)]
		private string name;

		[SerializeField]
		private bool showAlpha;

		[ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowPaging = false, ShowItemCount = true)]
		[PropertyOrder(3f)]
		[SerializeField]
		private List<Color> colors = new List<Color>();

		/// <summary>
		/// Name of the color palette.
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		/// <summary>
		/// The colors.
		/// </summary>
		public List<Color> Colors
		{
			get
			{
				return colors;
			}
			set
			{
				colors = value;
			}
		}

		/// <summary>
		/// Whether to show the alpha channel.
		/// </summary>
		public bool ShowAlpha
		{
			get
			{
				return showAlpha;
			}
			set
			{
				showAlpha = value;
			}
		}
	}
}
