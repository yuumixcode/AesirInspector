namespace Sirenix.OdinInspector.Editor.Examples
{
	public class MyDockableGameDashboard : OdinEditorWindow
	{
		private const string DEFAULT_GROUP = "TabGroup/Default/BtnGroup";

		private const string UNIFORM_GROUP = "TabGroup/Uniform/BtnGroup";

		[TabGroup("TabGroup", "Default", false, 0f)]
		[TabGroup("TabGroup", "Uniform", false, 0f)]
		public bool Toggle;

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup", DefaultButtonSize = ButtonSizes.Large)]
		[TabGroup("TabGroup", "Default", false, 0f, Paddingless = false)]
		public void PepperPepperPepper()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void Thud()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void WaldoWaldo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void Fred()
		{
		}

		[DisableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void FooFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void BarBar()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void BazBazBaz()
		{
		}

		[DisableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void QuxQux()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void QuuxQuuxQuux()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		[EnableIf("Toggle")]
		public void CorgeCorge()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void Uier()
		{
		}

		[EnableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		[Button(ButtonSizes.Small)]
		public void A()
		{
		}

		[EnableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		[Button(ButtonSizes.Small)]
		public void B()
		{
		}

		[Button(ButtonSizes.Small)]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		[ShowIf("Toggle", false)]
		public void C()
		{
		}

		[EnableIf("Toggle")]
		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void Henk()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void Def()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Default/BtnGroup")]
		public void DefDefDef()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup", UniformLayout = true)]
		[TabGroup("TabGroup", "Uniform", false, 0f)]
		public void FooPepper()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void FooThud()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void WaldoFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void FredFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		[DisableIf("Toggle")]
		public void Fooooo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void BarFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void BazFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		[DisableIf("Toggle")]
		public void FooQux()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void QuuxFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void UierFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		[EnableIf("Toggle")]
		public void CorgeFoo()
		{
		}

		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		[EnableIf("Toggle")]
		public void FooGrapl()
		{
		}

		[Button(ButtonSizes.Large)]
		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void FooDef()
		{
		}

		[Button(ButtonSizes.Large)]
		[ResponsiveButtonGroup("TabGroup/Uniform/BtnGroup")]
		public void DefFoo()
		{
		}
	}
}
