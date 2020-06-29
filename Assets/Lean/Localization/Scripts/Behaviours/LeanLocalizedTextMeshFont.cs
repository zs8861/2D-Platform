using UnityEngine;

namespace Lean.Localization
{
	/// <summary>This component will update a TextMesh component's Font with a localized font, or use a fallback if none is found.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(TextMesh))]
	[AddComponentMenu(LeanLocalization.ComponentPathPrefix + "Localized TextMesh Font")]
	public class LeanLocalizedTextMeshFont : LeanLocalizedBehaviour
	{
		[Tooltip("If PhraseName couldn't be found, this font asset will be used")]
		public Font FallbackFont;

		// This gets called every time the translation needs updating
		public override void UpdateTranslation(LeanTranslation translation)
		{
			// Get the TextMesh component attached to this GameObject
			var text = GetComponent<TextMesh>();

			// Use translation?
			if (translation != null && translation.Data is Font)
			{
				text.font = (Font)translation.Data;
			}
			// Use fallback?
			else
			{
				text.font = FallbackFont;
			}
		}

		protected virtual void Awake()
		{
			// Should we set FallbackFont?
			if (FallbackFont == null)
			{
				// Get the TextMesh component attached to this GameObject
				var text = GetComponent<TextMesh>();

				// Copy current text to fallback
				FallbackFont = text.font;
			}
		}
	}
}