using UnityEngine;
using UnityEngine.UI;

namespace Lean.Localization
{
	/// <summary>This component will update a UI.Text component with localized text, or use a fallback if none is found.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Text))]
	[HelpURL(LeanLocalization.HelpUrlPrefix + "LeanLocalizedText")]
	[AddComponentMenu(LeanLocalization.ComponentPathPrefix + "Localized Text")]
	public class LeanLocalizedText : LeanLocalizedBehaviour
	{
		[Tooltip("If PhraseName couldn't be found, this text will be used")]
		public string FallbackText;

		// This gets called every time the translation needs updating
		public override void UpdateTranslation(LeanTranslation translation)
		{
			// Get the Text component attached to this GameObject
			var text = GetComponent<Text>();

			// Use translation?
			if (translation != null && translation.Data is string)
			{
				text.text = LeanTranslation.FormatText((string)translation.Data, text.text, this);
			}
			// Use fallback?
			else
			{
				text.text = LeanTranslation.FormatText(FallbackText, text.text, this);
			}
		}

		protected virtual void Awake()
		{
			// Should we set FallbackText?
			if (string.IsNullOrEmpty(FallbackText) == true)
			{
				// Get the Text component attached to this GameObject
				var text = GetComponent<Text>();

				// Copy current text to fallback
				FallbackText = text.text;
			}
		}
	}
}