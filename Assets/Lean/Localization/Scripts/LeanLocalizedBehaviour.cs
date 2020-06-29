using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Localization
{
	/// <summary>This component simplifies the updating process, extend it if you want to cause a specific object to get localized</summary>
	public abstract class LeanLocalizedBehaviour : MonoBehaviour, ILocalizationHandler
	{
		[Tooltip("The name of the phrase we want to use for this localized component")]
		[SerializeField]
		[LeanTranslationName]
		[FormerlySerializedAs("phraseName")]
		[FormerlySerializedAs("translationTitle")]
		private string translationName;

		[System.NonSerialized]
		private HashSet<LeanToken> tokens;

		/// <summary>This is the name of the translation this script uses.</summary>
		public string TranslationName
		{
			set
			{
				if (translationName != value)
				{
					translationName = value;

					UpdateLocalization();
				}
			}

			get
			{
				return translationName;
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

		// This gets called every time the translation needs updating
		// NOTE: translation may be null if it can't be found
		public abstract void UpdateTranslation(LeanTranslation translation);

		/// <summary>If you call this then this component will update using the translation for the specified phrase.</summary>
		[ContextMenu("Update Localization")]
		public void UpdateLocalization()
		{
			UpdateTranslation(LeanLocalization.GetTranslation(translationName));
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

#if UNITY_EDITOR
namespace Lean.Localization
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanLocalizedBehaviour), true)]
	public class LeanLocalizedBehaviour_Inspector : LeanInspector<LeanLocalizedBehaviour>
	{
	}
}
#endif