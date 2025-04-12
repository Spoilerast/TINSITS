using System;
using Extensions;
using NotMonos.Databases;

namespace NotMonos.Backstage
{
internal sealed class SelectionController
{
	internal event Action UnitSelected;
	internal event Action<UnitId> UnitSelectedId;
	internal UnitId SelectedUnit { get; private set; }

	public void AllySelected(UnitId initiatorId)
	{
		if (IsAlreadySelected(initiatorId))
			return;

		SelectedUnit = initiatorId;
		UnitSelected.SafeInvoke();
		UnitSelectedId.SafeInvoke(initiatorId);
	}

	public void RivalSelected(UnitId rivalId)
	{
		if (!DataCenter.Grid.IsReachable(SelectedUnit, rivalId))
			return;

		PeekLogger.LogName("rival reachable!");
	}

	public void UnselectCurrent() { SelectedUnit = null; }

	private bool IsAlreadySelected(UnitId initiatorId)
	{
		string currentSelected = SelectedUnit == null
			? "NONE"
			: SelectedUnit.ToRichString;
		PeekLogger.Log($"new selected: {initiatorId.ToRichString}; current selected: {currentSelected}");
		return SelectedUnit && initiatorId.Equals(SelectedUnit);
	}
}
}