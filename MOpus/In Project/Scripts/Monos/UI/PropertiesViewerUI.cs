using Extensions;
using NotMonos;
using NotMonos.Databases;
using UnityEngine;
using UnityEngine.UIElements;

namespace Monos.Systems
{
[RequireComponent(typeof(UIDocument))]
public sealed class PropertiesViewerUI : SceneUI
{
	private UnitId _current = UnitId.Zero;
	private PropertiesGroup _group;
	private bool _isInitialized;

	private void OnEnable()
	{
		if (!_isInitialized)
			return;

		DataCenter.Selection.UnitSelectedId += UnitSelected;
	}

	private void OnDisable() { DataCenter.Selection.UnitSelectedId -= UnitSelected; }

	internal void Initialize()
	{
		if (!isActiveAndEnabled)
			return;

		var uiDocument = GetComponent<UIDocument>();
		uiDocument.FindFirstVisualElement(out GroupBox group);

		group.FindVisualElement("UIDField", out IntegerField uid);
		group.FindVisualElement("TIDField", out IntegerField tid);
		group.FindVisualElement("TypeGroup", out RadioButtonGroup type);
		group.FindVisualElement("IntField", out FloatField intgrt);
		group.FindVisualElement("CapField", out FloatField cap);
		group.FindVisualElement("ChargeField", out FloatField charge);
		group.FindVisualElement("ResField", out FloatField res);

		_group = new(ref uid, ref tid, ref type, ref charge, ref intgrt, ref cap, ref res);
		_isInitialized = true;
		OnEnable(); //todo possible double subscription
	}

	private void SetProperties(ushort u,
							   byte t,
							   PrismType tp,
							   float i,
							   float c,
							   float ch,
							   float r)
	{
		_group.SetUID(u);
		_group.SetTID(t);
		_group.SetType(tp);
		_group.SetCharge(ch);
		_group.SetIntegrity(i);
		_group.SetCapacity(c);
		_group.SetResistance(r);
	}

	private void UnitSelected(UnitId id)
	{
		PeekLogger.LogName($" {_current} {id}");
		if (_current.Equals(id))
			return;

		_current = id;
		(TeamId, PrismType, float, float, float, float) values = DataCenter.Properties.GetProperties(id);
		SetProperties(id.Value, values.Item1.Value, values.Item2, values.Item3, values.Item4, values.Item5,
					  values.Item6);
	}

	private class PropertiesGroup
	{
		private readonly FloatField _cap;
		private readonly FloatField _charge;
		private readonly FloatField _int;
		private readonly FloatField _res;
		private readonly IntegerField _tId;
		private readonly RadioButtonGroup _type;
		private readonly IntegerField _uId;

		public PropertiesGroup(ref IntegerField uId,
							   ref IntegerField tId,
							   ref RadioButtonGroup type,
							   ref FloatField charge,
							   ref FloatField intgrt,
							   ref FloatField cap,
							   ref FloatField res)
		{
			(_uId, _tId, _type, _charge, _int, _cap, _res)
				= (uId, tId, type, charge, intgrt, cap, res);
		}

		internal void SetCapacity(float value) { _cap.value = value; }

		internal void SetCharge(float value) { _charge.value = value; }

		internal void SetIntegrity(float value) { _int.value = value; }

		internal void SetResistance(float value) { _res.value = value; }

		internal void SetTID(ushort tid) { _tId.value = tid; }

		internal void SetType(PrismType type) { _type.value = (int)type; }

		internal void SetUID(ushort uid) { _uId.value = uid; }
	}
}
}