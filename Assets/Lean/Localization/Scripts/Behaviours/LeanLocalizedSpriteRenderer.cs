using UnityEngine;

namespace Lean.Localization
{
	/// <summary>This component will update a SpriteRenderer component with a localized sprite, or use a fallback if none is found</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(SpriteRenderer))]
	[HelpURL(LeanLocalization.HelpUrlPrefix + "LeanLocalizedSpriteRenderer")]
	[AddComponentMenu(LeanLocalization.ComponentPathPrefix + "Localized SpriteRenderer")]
	public class LeanLocalizedSpriteRenderer : LeanLocalizedBehaviour
	{
		[Tooltip("If PhraseName couldn't be found, this sprite will be used")]
		public Sprite FallbackSprite;

		// This gets called every time the translation needs updating
		public override void UpdateTranslation(LeanTranslation translation)
		{
			// Get the SpriteRenderer component attached to this GameObject
			var spriteRenderer = GetComponent<SpriteRenderer>();

			// Use translation?
			if (translation != null && translation.Data is Sprite)
			{
				spriteRenderer.sprite = (Sprite)translation.Data;
			}
			// Use fallback?
			else
			{
				spriteRenderer.sprite = FallbackSprite;
			}
		}

		protected virtual void Awake()
		{
			// Should we set FallbackSprite?
			if (FallbackSprite == null)
			{
				// Get the SpriteRenderer component attached to this GameObject
				var spriteRenderer = GetComponent<SpriteRenderer>();

				// Copy current sprite to fallback
				FallbackSprite = spriteRenderer.sprite;
			}
		}
	}
}