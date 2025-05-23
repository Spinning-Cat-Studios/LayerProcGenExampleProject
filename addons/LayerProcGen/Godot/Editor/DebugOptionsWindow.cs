using System.Collections;
using Godot;
using Runevision.Common;
using System.Collections.Generic;
using System.Linq;

namespace Runevision.LayerProcGen;

public partial class DebugOptionsWindow : ScrollContainer
{
	[Export]
	public Texture2D ButtonTexture { get; set; }

	[Export]
	public Texture2D ToggleOn { get; set; }

	[Export]
	public Texture2D ToggleOff { get; set; }

	protected Dictionary<DebugOption, TreeItem> matchingOptions = new Dictionary<DebugOption, TreeItem>();

	protected Tree wrapper;
	protected Vector2 calculatedMinSize = new Vector2();
	private int oldChildrenCount;
	private TreeItem treeRoot;

	public override void _Ready()
	{
		if (GetChildCount() == 0)
		{
			wrapper = new Tree();
			AddChild(wrapper);
		}
		else
			wrapper = GetChild<Tree>(0);

		DebugOption.UIChanged += RefreshUI;

		wrapper.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		wrapper.SizeFlagsVertical = SizeFlags.ExpandFill;

		treeRoot = wrapper.CreateItem();
		wrapper.HideRoot = true;
		wrapper.SelectMode = Tree.SelectModeEnum.Multi;
		wrapper.ItemCollapsed += WrapperOnCollapsed;

		void WrapperOnCollapsed(TreeItem item)
		{
			RefreshUI();
		}

	}

	public override void _Process(double delta)
	{
		if (oldChildrenCount != ChildrenRecursive(DebugOption.root).Count())
		{
			treeRoot.Free();
			treeRoot = wrapper.CreateItem();
			foreach (DebugOption option in DebugOption.root.children)
				CreateOption(treeRoot, option, 0);
			CustomMinimumSize = calculatedMinSize with { X = calculatedMinSize.X * 1.3f }; //add some buffer... I know this should be calculated based on level and scrollbar etc.
			oldChildrenCount = ChildrenRecursive(DebugOption.root).Count();
		}
	}

	public void RefreshUI()
	{
		foreach ((DebugOption? debugOption, TreeItem? treeItem) in matchingOptions)
		{
			if (treeItem.GetButtonCount(0) > 0 && debugOption is DebugToggle foldout)
			{
				treeItem.SetButton(0, 0, foldout.enabledSelf ? ToggleOn : ToggleOff);
			}
		}
	}

	private void CreateOption(TreeItem parentControl, DebugOption option, int level)
	{
		TreeItem? control = parentControl.CreateChild(0);
		matchingOptions.Add(option, control);
		control.SetSelectable(0, false);
		if (level <= 10)
			switch (option)
			{
				case DebugButton:
				{
					control.AddButton(0, ButtonTexture);
					wrapper.ButtonClicked += WrapperOnButtonClicked;

					void WrapperOnButtonClicked(TreeItem item, long column, long id, long mousebuttonindex)
					{
						if (item == control)
							option.HandleClick();
					}

					break;
				}
				case DebugToggle toggle:
				{
					control.AddButton(0, toggle.enabledSelf ? ToggleOn : ToggleOff);
					wrapper.ButtonClicked += WrapperOnButtonClicked;

					void WrapperOnButtonClicked(TreeItem item, long column, long id, long mousebuttonindex)
					{
						if (item != control) return;
						toggle.HandleClick();
						foreach (var child in ChildrenRecursive(toggle).OfType<DebugToggle>())
							child.SetEnabled(toggle.enabledSelf);
						RefreshUI();
					}

					break;
				}
				case DebugFoldout foldout:
				{
					break;
				}
				default:
					GD.PushWarning($"No case for: {option.GetType()}");
					break;
			}

		Vector2 minSize = GetThemeDefaultFont().GetStringSize(option.name) * ((level + 1) * .7f); //magic number, should be changed to the inset of the foldout
		// aka: Vector2 minSize = GetThemeDefaultFont().GetStringSize(option.name) + ((level + 1) * foldoutWidth);
		if (minSize > calculatedMinSize)
			calculatedMinSize = minSize;
		control.SetText(0, option.name /*+ $" ({option.GetType().Name})"*/);
		control.Visible = !option.hidden;

		if (option is DebugFoldout parent)
		{
			control.Collapsed = !parent.enabledSelf;
			foreach (DebugOption child in parent.children)
				CreateOption(control, child, level + 1);
		}
	}

	private IEnumerable<DebugOption> ChildrenRecursive(DebugOption debugOption)
	{
		yield return debugOption;
		if (debugOption is not DebugFoldout foldout) yield break;
		foreach (var child in foldout.children.SelectMany(ChildrenRecursive))
			yield return child;
	}

	public override void _ExitTree()
	{
		DebugOption.UIChanged -= RefreshUI;
	}
}
