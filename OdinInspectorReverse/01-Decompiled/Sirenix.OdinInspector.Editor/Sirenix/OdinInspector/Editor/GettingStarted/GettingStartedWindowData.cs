using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.GettingStarted
{
	public static class GettingStartedWindowData
	{
		public static GettingStartedProduct[] Products;

		public static TutorialPage OdinInspectorPage;

		public static WizardPage OdinValidatorWizardPage;

		public static TutorialPage OdinValidatorGettingStartedPage;

		public static TutorialPage OdinSerializerPage;

		private static bool attributeProcessorsTutorialIsImported;

		private static bool rpgSampleProjectIsIsImported;

		private static bool customDrawersTutorialIsImported;

		private static bool editorTutorialIsImported;

		private static Color textColor
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return new Color(0.035f, 0.035f, 0.035f, 1f);
				}
				return new Color(0.769f, 0.769f, 0.769f, 1f);
			}
		}

		static GettingStartedWindowData()
		{
			attributeProcessorsTutorialIsImported = TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Demos.PutAttributesOnAnyType") != null;
			rpgSampleProjectIsIsImported = TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Demos.RPGEditor.RPGEditorWindow") != null;
			customDrawersTutorialIsImported = TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Demos.CustomGroupExample") != null;
			editorTutorialIsImported = TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Demos.OdinMenuEditorWindowExample") != null;
			Type validatorPage = TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinValidator.Editor.SetupWizard");
			if (validatorPage != null)
			{
				OdinValidatorWizardPage = Activator.CreateInstance(validatorPage) as WizardPage;
				OdinValidatorGettingStartedPage = new TutorialPage
				{
					Title = "Get started with Odin Validator",
					TitleIcon = SdfIconType.BookHalf,
					Tutorials = new List<Tutorial>
					{
						new Tutorial
						{
							Title = "Quick Start",
							Description = "Get started quickly with an overview of the basics of Odin Validator.",
							OnClick = delegate
							{
								Application.OpenURL("https://odininspector.com/tutorials/odin-validator/getting-started-with-odin-validator");
							},
							Icon = SdfIconType.Link,
							Difficulty = Difficulty.Beginner
						},
						new Tutorial
						{
							Title = "Validate Using Attributes",
							Description = "Learn the basic usage of Odin Validator; how to customize and specify validation logic using simple attributes.",
							OnClick = delegate
							{
								Application.OpenURL("https://odininspector.com/tutorials/odin-validator/validate-using-attributes");
							},
							Icon = SdfIconType.Link,
							Difficulty = Difficulty.Beginner
						},
						new Tutorial
						{
							Title = "Validator Types Overview",
							Description = "Become familiar with how to customize Odin Validator beyond the basic attributes, by learning about the basic validator types and interfaces.",
							OnClick = delegate
							{
								Application.OpenURL("https://odininspector.com/tutorials/odin-validator/validator-types-overview");
							},
							Icon = SdfIconType.Link,
							Difficulty = Difficulty.Intermediate
						},
						new Tutorial
						{
							Title = "Validators vs Validation Rules",
							Description = "Learn about how to create validation rules, and the distinction between a normal validator and a validation rule.",
							OnClick = delegate
							{
								Application.OpenURL("https://odininspector.com/tutorials/odin-validator/validators-vs-validation-rules");
							},
							Icon = SdfIconType.Link,
							Difficulty = Difficulty.Intermediate
						},
						new Tutorial
						{
							Title = "Creating Custom Fixes",
							Description = "Learn all about how to add custom fixes to validation issues.",
							OnClick = delegate
							{
								Application.OpenURL("https://odininspector.com/tutorials/odin-validator/creating-custom-fixes");
							},
							Icon = SdfIconType.Link,
							Difficulty = Difficulty.Advanced
						},
						new Tutorial
						{
							Title = "Custom Pipeline",
							Description = "Learn about how to integrate the Odin Validator into your custom pipeline.",
							OnClick = delegate
							{
								Application.OpenURL("https://odininspector.com/tutorials/odin-validator/using-the-validator-in-your-custom-pipeline");
							},
							Icon = SdfIconType.Link,
							Difficulty = Difficulty.Intermediate
						},
						new Tutorial
						{
							Title = "Migrating 2.1 Validators to 3.0",
							Description = "Validators written prior to Odin 3.0 may be obsolete and relying on old APIs and functionality that will eventually be removed. This tutorial covers which things changed, and how to upgrade your old validators to the new standards.",
							OnClick = delegate
							{
								Application.OpenURL("https://odininspector.com/tutorials/odin-validator/migrating-2.1-and-3.0-validators-to-3.1");
							},
							Icon = SdfIconType.Link,
							Difficulty = Difficulty.Intermediate
						}
					}
				};
			}
			OdinInspectorPage = new TutorialPage
			{
				Title = "Get started with Odin Inspector",
				TitleIcon = SdfIconType.Book,
				Tutorials = new List<Tutorial>
				{
					new Tutorial
					{
						Title = "Visual Designer",
						Description = "The Visual Designer is a new workflow for <b>customizing how types appear in the Inspector — all without writing code</b>.",
						Icon = SdfIconType.StarFill,
						Difficulty = Difficulty.Beginner,
						OnClick = delegate
						{
							Application.OpenURL("https://odininspector.com/visual-designer-getting-started");
						}
					},
					new Tutorial
					{
						Title = "Odin Attributes Overview",
						Description = "The best way to get started using Odin is to open the Attributes Overview window found at Tools > Odin > Inspector > Attribute Overview.",
						Icon = SdfIconType.Window,
						Difficulty = Difficulty.Beginner,
						OnClick = delegate
						{
							AttributesExampleWindow.OpenWindow();
						}
					},
					new Tutorial
					{
						Title = "The Static Inspector",
						Description = "If you're a programmer, then you're likely going find the static inspector helpful during debugging and testing. Just open up the window, and start using it! You can find the utility under 'Tools > Odin > Inspector > Static Inspector'.",
						Difficulty = Difficulty.Beginner,
						Icon = SdfIconType.Window,
						OnClick = delegate
						{
							StaticInspectorWindow.InspectType(typeof(Time), StaticInspectorWindow.AccessModifierFlags.All, StaticInspectorWindow.MemberTypeFlags.AllButObsolete);
						}
					},
					new Tutorial
					{
						Title = "Odin Editor Windows",
						Description = "Learn how you can use Odin to rapidly create custom Editor Windows to help organize your project data. This is where Odin can really help boost your workflow.",
						Difficulty = Difficulty.Beginner,
						Icon = SdfIconType.FolderFill,
						ChildPage = new TutorialPage
						{
							Title = "Editor Windows",
							TitleIcon = SdfIconType.Window,
							Tutorials = new List<Tutorial>
							{
								new Tutorial
								{
									Title = (EditorTutorialIsNotImported() ? "Import package" : "Package imported"),
									Difficulty = Difficulty.None,
									Icon = SdfIconType.FolderFill,
									Enabled = EditorTutorialIsNotImported,
									OnClick = delegate
									{
										AssetDatabase.ImportPackage(SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows.unitypackage", interactive: true);
									}
								},
								new Tutorial
								{
									Title = "Basic Odin Editor Window",
									Description = "Inherit from OdinEditorWindow instead of EditorWindow. This will enable you to render fields, properties and methods and make editor windows using attributes, without writing any custom editor code.",
									Difficulty = Difficulty.Beginner,
									Icon = SdfIconType.FileEarmarkCodeFill,
									Enabled = EditorTutorialIsImported,
									ActionButtons = new List<(string, Action)>
									{
										("Open window", delegate
										{
											AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.BasicOdinEditorExampleWindow").GetMethod("OpenWindow", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(null, null);
										}),
										("Open script", delegate
										{
											AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows/Scripts/Editor/BasicOdinEditorExampleWindow.cs"));
										})
									}
								},
								new Tutorial
								{
									Title = "Odin Menu Editor Windows",
									Description = "Derive from OdinMenuEditorWindow to create windows that inspect a custom tree of target objects. These are great for organizing your project, and managing Scriptable Objects etc. Odin itself uses this to draw its preferences window.",
									Difficulty = Difficulty.Beginner,
									Icon = SdfIconType.FileEarmarkCodeFill,
									Enabled = EditorTutorialIsImported,
									ActionButtons = new List<(string, Action)>
									{
										("Open window", delegate
										{
											AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.OdinMenuEditorWindowExample").GetMethod("OpenWindow", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(null, null);
										}),
										("Open script", delegate
										{
											AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows/Scripts/Editor/OdinMenuEditorWindowExample.cs"));
										})
									}
								},
								new Tutorial
								{
									Title = "Override GetTargets()",
									Description = "Odin Editor Windows are not limited to drawing themselves; you can override GetTarget() or GetTargets() to make them display scriptable objects, components or any arbitrary types (except value types like structs).",
									Difficulty = Difficulty.Advanced,
									Icon = SdfIconType.FileEarmarkCodeFill,
									Enabled = EditorTutorialIsImported,
									ActionButtons = new List<(string, Action)>
									{
										("Open window", delegate
										{
											AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.OverrideGetTargetsExampleWindow").GetMethod("OpenWindow", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(null, null);
										}),
										("Open script", delegate
										{
											AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows/Scripts/Editor/OverrideGetTargetsExampleWindow.cs"));
										})
									}
								},
								new Tutorial
								{
									Title = "Quickly inspect objects",
									Description = "Call OdinEditorWindow.InspectObject(myObj) to quickly pop up an editor window for any given object. This is a great way to quickly debug objects or make custom editor windows on the spot!",
									Difficulty = Difficulty.Intermediate,
									Icon = SdfIconType.FileEarmarkCodeFill,
									Enabled = EditorTutorialIsImported,
									ActionButtons = new List<(string, Action)>
									{
										("Open window", delegate
										{
											OdinEditorWindow.InspectObject(Activator.CreateInstance(AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.QuicklyInspectObjects")));
										}),
										("Open script", delegate
										{
											AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows/Scripts/Editor/QuicklyInspectObjects.cs"));
										})
									}
								}
							}
						}
					},
					new Tutorial
					{
						Title = "Custom Drawers",
						Description = "Making custom drawers in Odin is 10x faster and 10x more powerful than in vanilla Unity. Drawers are strongly typed, with generic resolution.",
						Icon = SdfIconType.FolderFill,
						Difficulty = Difficulty.Intermediate,
						ChildPage = new TutorialPage
						{
							Title = "Custom Drawers",
							TitleIcon = SdfIconType.Code,
							Tutorials = new List<Tutorial>
							{
								new Tutorial
								{
									Title = "Import example scene",
									Difficulty = Difficulty.None,
									Icon = SdfIconType.FolderFill,
									Enabled = CustomDrawersIsNotImported,
									OnClick = delegate
									{
										AssetDatabase.ImportPackage(SirenixAssetPaths.SirenixPluginPath + "Demos/Custom Drawers.unitypackage", interactive: true);
									}
								},
								new Tutorial
								{
									Title = "Open example scene",
									Difficulty = Difficulty.Intermediate,
									Icon = SdfIconType.SendCheckFill,
									Enabled = CustomDrawersIsImported,
									OnClick = delegate
									{
										OpenScene(SirenixAssetPaths.SirenixPluginPath + "Demos/Custom Drawers/Custom Drawers.unity");
									}
								}
							}
						}
					},
					new Tutorial
					{
						Title = "Attribute Processors",
						Description = "You can take complete control over how Odin finds its members to display and which attributes to put on those members. This can be extremely useful for automation and providing support and editor customizations for third-party libraries you don't own the code for.",
						Icon = SdfIconType.FolderFill,
						Difficulty = Difficulty.Advanced,
						ChildPage = new TutorialPage
						{
							Title = "Attribute Processors",
							TitleIcon = SdfIconType.Code,
							Tutorials = new List<Tutorial>
							{
								new Tutorial
								{
									Title = "Attribute Processors",
									Description = "Need to add attributes to 3rd party code? Can't access source code for an asset and still want to modify the inspector? With custom attribute processors and Odin Inspector, you can do that!",
									Difficulty = Difficulty.Advanced,
									Icon = SdfIconType.Youtube,
									OnClick = delegate
									{
										Application.OpenURL("https://odininspector.com/tutorials/using-property-resolvers-and-attribute-processors/custom-attribute-processors#odin-inspector");
									}
								},
								new Tutorial
								{
									Title = "Property Processors",
									Description = "Odin property processors are the code that tells Odin what data to display and what attributes are associated with that data. This means that creating a custom property processor can allow you to alter the properties that are shown in the inspector. This is done without altering the original code in any way and works with 3rd party code where you may not have access to the source code. Property processors let you define your own rules for how properties are made, both globally or just for select types.",
									Difficulty = Difficulty.Advanced,
									Icon = SdfIconType.Youtube,
									OnClick = delegate
									{
										Application.OpenURL("https://odininspector.com/tutorials/using-property-resolvers-and-attribute-processors/custom-property-processors#odin-inspector");
									}
								},
								new Tutorial
								{
									Title = "Import example scene",
									Difficulty = Difficulty.None,
									Icon = SdfIconType.FolderFill,
									Enabled = AttributeProcessorsIsNotImported,
									OnClick = delegate
									{
										AssetDatabase.ImportPackage(SirenixAssetPaths.SirenixPluginPath + "Demos/Custom Attribute Processors.unitypackage", interactive: true);
									}
								},
								new Tutorial
								{
									Title = "Open example scene",
									Difficulty = Difficulty.Advanced,
									Icon = SdfIconType.SendCheckFill,
									Enabled = AttributeProcessorsIsImported,
									OnClick = delegate
									{
										OpenScene(SirenixAssetPaths.SirenixPluginPath + "Demos/Custom Attribute Processors/Custom Attribute Processors.unity");
									}
								}
							}
						}
					},
					new Tutorial
					{
						Title = "RPG Editor (sample project)",
						Description = "This project showcases Odin Editor Windows, Odin Selectors, various attribute combinations, and custom drawers to build a feature-rich editor window for managing scriptable objects.",
						Icon = SdfIconType.FolderFill,
						Difficulty = Difficulty.Advanced,
						ChildPage = new TutorialPage
						{
							Title = "RPG Editor (sample project)",
							TitleIcon = SdfIconType.Code,
							Tutorials = new List<Tutorial>
							{
								new Tutorial
								{
									Title = (AttributeProcessorsIsNotImported() ? "Import package" : "Package imported"),
									Difficulty = Difficulty.None,
									Icon = SdfIconType.FolderFill,
									Enabled = RPGSampleProjectIsNotImported,
									OnClick = delegate
									{
										AssetDatabase.ImportPackage(SirenixAssetPaths.SirenixPluginPath + "Demos/Sample - RPG Editor.unitypackage", interactive: true);
									}
								},
								new Tutorial
								{
									Title = "Open RPG editor window",
									Description = "You can also find the window under Tools > Odin > Demos",
									Difficulty = Difficulty.Advanced,
									Icon = SdfIconType.Window,
									Enabled = RPGSampleProjectIsImported,
									OnClick = delegate
									{
										AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.RPGEditor.RPGEditorWindow").GetMethod("Open", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(null, null);
									}
								},
								new Tutorial
								{
									Title = "Open script",
									Description = "Go and explore how things are implemented, the script for the window itself is a great place to start",
									Difficulty = Difficulty.Advanced,
									Icon = SdfIconType.FileEarmarkCodeFill,
									Enabled = RPGSampleProjectIsImported,
									OnClick = delegate
									{
										AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SirenixAssetPaths.SirenixPluginPath + "Demos/Sample - RPG Editor/Scripts/Editor/RPGEditorWindow.cs"));
									}
								}
							}
						}
					}
				}
			};
			OdinSerializerPage = new TutorialPage
			{
				Title = "Get started with Odin Serializer",
				TitleIcon = SdfIconType.Book,
				Tutorials = new List<Tutorial>
				{
					new Tutorial
					{
						Title = "Quick start",
						Description = "Getting started with Odin Serializer is very easy; read this to get a quick start and hit the ground running.",
						OnClick = delegate
						{
							Application.OpenURL("https://odininspector.com/tutorials/serialize-anything/odin-serializer-quick-start#odin-serializer");
						},
						Icon = SdfIconType.Link,
						Difficulty = Difficulty.Beginner
					},
					new Tutorial
					{
						Title = "The Serialization Debugger",
						Description = "If you are utilizing Odin's serialization, the Serialization Debugger can show you which members of any given type are being serialized, and whether they are serialized by Unity, Odin or both. You can find the utility under 'Tools > Odin > Serializer > Serialization Debugger' or from the context menu in the inspector.",
						OnClick = SerializationDebuggerWindow.ShowWindow,
						Icon = SdfIconType.Window,
						Difficulty = Difficulty.Beginner
					},
					new Tutorial
					{
						Title = "AOT Serialization",
						Description = "On AOT (ahead-of-time) platforms like IL2CPP, special care is needed to ensure all necessary code for serialization is present at runtime. Learn about Odin Serializer's solutions for easily and automatically generating AOT support.",
						OnClick = delegate
						{
							Application.OpenURL("https://odininspector.com/tutorials/serialize-anything/aot-serialization#odin-serializer");
						},
						Icon = SdfIconType.Link,
						Difficulty = Difficulty.Beginner
					},
					new Tutorial
					{
						Title = "Implementing Odin serialization without inheritance",
						Description = "You don't have to inherit from one of our base classes to get Odin serialization, you can also make your own by implementing Odin's serialization manually. ",
						OnClick = delegate
						{
							Application.OpenURL("https://odininspector.com/tutorials/serialize-anything/implementing-the-odin-serializer#odin-serializer");
						},
						Icon = SdfIconType.Link,
						Difficulty = Difficulty.Intermediate
					},
					new Tutorial
					{
						Title = "Serializing without Unity objects",
						Description = " In this tutorial you'll learn to serialize non-Unity-object types to bytes using the SerializationUtility API.",
						OnClick = delegate
						{
							Application.OpenURL("https://odininspector.com/tutorials/serialize-anything/serializing-without-serialized-base-classes");
						},
						Icon = SdfIconType.Link,
						Difficulty = Difficulty.Intermediate
					}
				}
			};
			CreateProducts();
		}

		private static void CreateProducts()
		{
			Products = new GettingStartedProduct[3];
			ref GettingStartedProduct ins = ref Products[0];
			ref GettingStartedProduct val = ref Products[1];
			ref GettingStartedProduct ser = ref Products[2];
			ins = new GettingStartedProduct
			{
				Name = "Odin Inspector",
				ReviewUrl = "https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041#reviews",
				Enabled = true,
				Logo = OdinEditorResources.OdinInspectorLogo,
				HueColor = SirenixGUIStyles.InspectorOrange,
				Status = "Installed",
				StatusIcon = SdfIconType.CheckCircleFill,
				StatusIconColor = SirenixGUIStyles.ValidatorGreen,
				Pages = new(GettingStartedPage, string)[1] { (OdinInspectorPage, "Get Started") }
			};
			if (OdinValidatorWizardPage == null)
			{
				val = new GettingStartedProduct
				{
					Name = "Odin Validator",
					ReviewUrl = "https://assetstore.unity.com/packages/tools/utilities/odin-validator-227861#reviews",
					Enabled = false,
					Logo = OdinEditorResources.OdinValidatorLogo,
					HueColor = SirenixGUIStyles.ValidatorGreen,
					Status = "Not installed",
					StatusIcon = SdfIconType.XCircleFill,
					StatusIconColor = textColor,
					Pages = new(GettingStartedPage, string)[2]
					{
						(new ButtonPage(OpenProjectValidatorURLAssetStore), "Learn more"),
						(new ButtonPage(OpenProjectValidatorDownloadURL), "Download")
					}
				};
			}
			else
			{
				val = new GettingStartedProduct
				{
					Name = "Odin Validator",
					ReviewUrl = "https://assetstore.unity.com/packages/tools/utilities/odin-validator-227861#reviews",
					Enabled = true,
					Logo = OdinEditorResources.OdinValidatorLogo,
					HueColor = SirenixGUIStyles.ValidatorGreen,
					Status = "Installed",
					StatusIcon = SdfIconType.CheckCircleFill,
					StatusIconColor = SirenixGUIStyles.ValidatorGreen,
					Pages = new(GettingStartedPage, string)[2]
					{
						(OdinValidatorWizardPage, "Setup Wizard"),
						(OdinValidatorGettingStartedPage, "Getting Started")
					}
				};
			}
			if (EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled())
			{
				ser = new GettingStartedProduct
				{
					Name = "Odin Serializer",
					Enabled = false,
					Logo = OdinEditorResources.OdinSerializerLogo,
					HueColor = SirenixGUIStyles.SerializerYellow,
					Status = "Disabled",
					StatusIcon = SdfIconType.XCircleFill,
					StatusIconColor = textColor,
					Pages = new(GettingStartedPage, string)[2]
					{
						(new ButtonPage(OpenEditorOnlyModeToggle), "Configure"),
						(OdinSerializerPage, "Learn more")
					}
				};
			}
			else
			{
				ser = new GettingStartedProduct
				{
					Name = "Odin Serializer",
					Enabled = true,
					Logo = OdinEditorResources.OdinSerializerLogo,
					HueColor = SirenixGUIStyles.SerializerYellow,
					Status = "Enabled",
					StatusIcon = SdfIconType.CheckCircleFill,
					StatusIconColor = SirenixGUIStyles.ValidatorGreen,
					Pages = new(GettingStartedPage, string)[2]
					{
						(new ButtonPage(OpenEditorOnlyModeToggle), "Configure"),
						(OdinSerializerPage, "Getting Started")
					}
				};
			}
		}

		private static bool AttributeProcessorsIsImported()
		{
			return attributeProcessorsTutorialIsImported;
		}

		private static bool AttributeProcessorsIsNotImported()
		{
			return !AttributeProcessorsIsImported();
		}

		private static bool RPGSampleProjectIsImported()
		{
			return rpgSampleProjectIsIsImported;
		}

		private static bool RPGSampleProjectIsNotImported()
		{
			return !RPGSampleProjectIsImported();
		}

		private static bool CustomDrawersIsImported()
		{
			return customDrawersTutorialIsImported;
		}

		private static bool CustomDrawersIsNotImported()
		{
			return !CustomDrawersIsImported();
		}

		private static bool EditorTutorialIsImported()
		{
			return editorTutorialIsImported;
		}

		private static bool EditorTutorialIsNotImported()
		{
			return !EditorTutorialIsImported();
		}

		private static void OpenEditorOnlyModeToggle()
		{
			SirenixPreferencesWindow.OpenWindow(EditorOnlyModeConfig.Instance);
		}

		private static void OpenProjectValidatorURL()
		{
			Application.OpenURL("https://odininspector.com/odin-project-validator");
		}

		private static void OpenProjectValidatorURLAssetStore()
		{
			Application.OpenURL("https://odininspector.com/odin-validator-asset-store-redirect");
		}

		private static void OpenProjectValidatorDownloadURL()
		{
			Application.OpenURL("https://odininspector.com/download");
		}

		private static void OpenScene(string scenePath)
		{
			UnityEditorEventUtility.DelayAction(delegate
			{
				SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
				AssetDatabase.OpenAsset(sceneAsset);
				if ((bool)sceneAsset)
				{
					UnityEditorEventUtility.DelayAction(delegate
					{
						(from x in UnityEngine.Object.FindObjectsOfType<Transform>()
							where x.parent == null && x.childCount > 0
							orderby x.GetSiblingIndex() descending
							select x.transform.GetChild(0).gameObject).ForEach(delegate(GameObject x)
						{
							EditorGUIUtility.PingObject(x);
						});
					});
				}
			}, excludeGuiEventHooks: true);
		}
	}
}
