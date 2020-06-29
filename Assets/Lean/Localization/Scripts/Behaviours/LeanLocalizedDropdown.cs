using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Lean.Localization
{
	/// <summary>This component will update a <b>UI.Dropdown</b> component with localized text, or use a fallback if none is found.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Dropdown))]
	[HelpURL(LeanLocalization.HelpUrlPrefix + "LeanLocalizedDropdown")]
	[AddComponentMenu(LeanLocalization.ComponentPathPrefix + "Localized Dropdown")]
	public class LeanLocalizedDropdown : MonoBehaviour, ILocalizationHandler
	{
		[System.Serializable]
		public class Option
		{
			[LeanTranslationName]
			public string StringTranslationName;

			[LeanTranslationName]
			public string SpriteTranslationName;

			[Tooltip("If StringTranslationName couldn't be found, this text will be used")]
			public string FallbackText;

			[Tooltip("If SpriteTranslationName couldn't be found, this sprite will be used")]
			public Sprite FallbackSprite;
		}

		[SerializeField]
		private List<Option> options;

		[System.NonSerialized]
		private HashSet<LeanToken> tokens;

		public List<Option> Options
		{
			get
			{
				if (options == null)
				{
					options = new List<Option>();
				}

				return options;
			}
		}

		public void Register(LeanToken token)
		{
			if (token != null)
			{
				if (tokens == null)
				{
					tokens = new HashSet<LeanToken>();
				}

				tokens.Add(token);
			}
		}

		public void Unregister(LeanToken token)
		{
			if (tokens != null)
			{
				tokens.Remove(token);
			}
		}

		public void UnregisterAll()
		{
			if (tokens != null)
			{
				foreach (var token in tokens)
				{
					token.Unregister(this);
				}

				tokens.Clear();
			}
		}

		/// <summary>If you call this then this component will update using the translation for the specified phrase.</summary>
		[ContextMenu("Update Localization")]
		public void UpdateLocalization()
		{
			var dropdown = GetComponent<Dropdown>();
			var dOptions = dropdown.options;

			if (options != null)
			{
				for (var i = 0; i < options.Count; i++)
				{
					var option  = options[i];
					var dOption = default(Dropdown.OptionData);

					if (dOptions.Count == i)
					{
						dOption = new Dropdown.OptionData();

						dOptions.Add(dOption);
					}
					else
					{
						dOption = dOptions[i];
					}

					var stringTranslation = LeanLocalization.GetTranslation(option.StringTranslationName);

					// Use translation?
					if (stringTranslation != null && stringTranslation.Data is string)
					{
						dOption.text = LeanTranslation.FormatText((string)stringTranslation.Data, dOption.text, this);
					}
					// Use fallback?
					else
					{
						dOption.text = LeanTranslation.FormatText(option.FallbackText, dOption.text, this);
					}

					var spriteTranslation = LeanLocalization.GetTranslation(option.StringTranslationName);

					// Use translation?
					if (spriteTranslation != null && spriteTranslation.Data is Sprite)
					{
						dOption.image = (Sprite)spriteTranslation.Data;
					}
					// Use fallback?
					else
					{
						dOption.image = option.FallbackSprite;
					}
				}
			}
			else
			{
				dOptions.Clear();
			}

			dropdown.options = dOptions;
		}

		protected virtual void OnEnable()
		{
			LeanLocalization.OnLocalizationChanged += UpdateLocalization;

			UpdateLocalization();
		}

		protected virtual void OnDisable()
		{
			LeanLocalization.OnLocalizationChanged -= UpdateLocalization;

			UnregisterAll();
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			if (isActiveAndEnabled == true)
			{
				UpdateLocalization();
			}
		}
#endif
	}
}