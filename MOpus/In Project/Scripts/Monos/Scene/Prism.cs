using System;
using Extensions;
using Monos.Systems;
using NotMonos;
using NotMonos.Databases;
using Unity.Collections;
using UnityEngine;

namespace Monos.Scene
{
	[SelectionBase]
	internal sealed class Prism : InteractableObject
	{
		[Header("Warning: changing this in Play Mode will not cause any effect.")]
		[Header("Properties reading once on Scene Load")]
		[Space]
		[Header("Unit Id is generated in Play Mode")]
		[SerializeField, ReadOnly] private ushort _id;//its not readonly!
		[Space]
		[SerializeField] private byte _teamId;
		[SerializeField] private PrismType _type;
		[Space]
		[SerializeField, Range(0.1f, 255f)] private float _integrity;
		[SerializeField, Range(0.1f, 255f)] private float _capacity;
		[SerializeField, Range(0f, 255f)] private float _charge;
		[SerializeField, Range(0.1f, 255f)] private float _resistance;
		private UnitId _unitId;

		internal event Action<UnitId> OnAllySelected;

		internal event Action<UnitId> OnRivalSelected;

		internal UnitId Id
		{
			get => _unitId;
			set
			{
				_unitId = value;
				_id = _unitId.Value;
			}
		}

		private bool IsInCurrentPlayerTeam
			=> DataCenter.Properties.IsInCurrentPlayerTeam(Id);

		public override void DestroySceneObject()
		{
			UnsubcribeSelected();
			Destroy();
		}

		internal void Deconstruct(
			out byte teamId,
			out byte type,
			out float integrity,
			out float capacity,
			out float charge,
			out float resistance)
		{
			teamId = _teamId;
			type = (byte)_type;
			integrity = _integrity;
			capacity = _capacity;
			charge = _charge;
			resistance = _resistance;
		}

		internal void Initialize()
		{
			Id = NotMonos.Processors
				.SceneObjectsToEntitiesProcessor.AddPrism(this);
			EntryPoint subscriber = FindFirstObjectByType<EntryPoint>();
			subscriber.SubscribeOnPrism(this);
		}

		internal override void Interact()
		{
			SceneState state = SceneGlobals.CurrentState;
			if (state is not (SceneState.Default or SceneState.PreviewMode))
				return;

			bool interactToAlly = IsInCurrentPlayerTeam;
			string who = interactToAlly
				? "Ally"
				: "<color=orange>Rival</color>";
			PeekLogger.LogMessageTab($"<size=14>{who} Prism {Id.ToRichString} selected at {Position}</size> {state}");

			if (state is SceneState.Default)
			{
				if (interactToAlly)
					OnAllySelected.SafeInvoke(Id);
				else
					OnRivalSelected.SafeInvoke(Id);

				return;
			}

			if (!interactToAlly)
				OnRivalSelected.SafeInvoke(Id);
		}

		internal void SetId(UnitId unitId)
			=> Id = unitId;

		internal void UnsubcribeSelected()
		{
			OnAllySelected = null;
			OnRivalSelected = null;
		}
	}
}