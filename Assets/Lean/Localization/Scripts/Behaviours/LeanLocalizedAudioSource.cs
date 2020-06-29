using UnityEngine;

namespace Lean.Localization
{
	/// <summary>This component will update an AudioSource component with localized text, or use a fallback if none is found.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(AudioSource))]
	[HelpURL(LeanLocalization.HelpUrlPrefix + "LeanLocalizedAudioSource")]
	[AddComponentMenu(LeanLocalization.ComponentPathPrefix + "Localized AudioSource")]
	public class LeanLocalizedAudioSource : LeanLocalizedBehaviour
	{
		[Tooltip("If PhraseName couldn't be found, this clip will be used")]
		public AudioClip FallbackAudioClip;

		// This gets called every time the translation needs updating
		public override void UpdateTranslation(LeanTranslation translation)
		{
			// Get the AudioSource component attached to this GameObject
			var audioSource = GetComponent<AudioSource>();

			// Use translation?
			if (translation != null && translation.Data is AudioClip)
			{
				audioSource.clip = (AudioClip)translation.Data;
			}
			// Use fallback?
			else
			{
				audioSource.clip = FallbackAudioClip;
			}
		}

		protected virtual void Awake()
		{
			// Should we set FallbackAudioClip?
			if (FallbackAudioClip == null)
			{
				// Get the AudioSource component attached to this GameObject
				var audioSource = GetComponent<AudioSource>();

				// Copy current sprite to fallback
				FallbackAudioClip = audioSource.clip;
			}
		}
	}
}