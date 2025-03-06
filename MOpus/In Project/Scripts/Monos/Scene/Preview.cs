using System;
using Extensions;
using NotMonos;
using UnityEngine;

namespace Monos.Scene.Previews
{
	[RequireComponent(typeof(MeshRenderer))]
	internal abstract class Preview : InteractableObject
	{
		[SerializeField] private Material _activeMaterial;
		[SerializeField] private Material _declusterMaterial;
		[SerializeField] private Material _defaultMaterial;
		[SerializeField] private Material _hoverMaterial;
		private MeshRenderer _meshRenderer;

		internal event Action<PreviewId> OnPicked;

		private enum PreviewState
		{ Default, Hover, Active, Decluster }

		public PreviewId Id { get; internal set; }

		private void Awake()
			=> Initialize();

		private void OnMouseDown()
			=> ChangePreviewMaterial(PreviewState.Active);

		private void OnMouseEnter()
			=> ChangePreviewMaterial(PreviewState.Hover);

		private void OnMouseExit()
			=> ChangePreviewMaterial(PreviewState.Default);

		internal Preview Initialize()
		{
			this.TryGetComponentIfNull(ref _meshRenderer);
			return this;
		}

		internal override void Interact()
		{
			if (SceneGlobals.CurrentState is SceneState.PreviewMode or SceneState.SubPreviewMode)
				OnPicked.SafeInvoke(Id);
		}

		internal Preview MakeDeclusterPreview()
		{
			ChangePreviewMaterial(PreviewState.Decluster);
			GetComponent<MeshCollider>().enabled = false;
			return this;
		}

		private void ChangePreviewMaterial(PreviewState decluster)
			=> _meshRenderer.material = decluster switch
			{
				PreviewState.Default => _defaultMaterial,
				PreviewState.Hover => _hoverMaterial,
				PreviewState.Active => _activeMaterial,
				PreviewState.Decluster => _declusterMaterial,
				_ => _meshRenderer.material
			};
	}
}