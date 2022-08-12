﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈允许您通过MMSoundManager触发特定声音的淡入淡出。您需要在场景中安装一个MMSoundManager才能正常工作。
	/// This feedback lets you trigger fades on a specific sound via the MMSoundManager. You will need a MMSoundManager in your scene for this to work.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Audio（音效）/MMSoundManager Sound Fade（指定声音淡入淡出）")]
	[FeedbackHelp("此反馈允许您通过MMSoundManager触发特定声音的淡入淡出。您需要在场景中安装一个MMSoundManager才能正常工作。")]
	public class MMFeedbackMMSoundManagerSoundFade : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif

		[Header("MMSoundManager Sound Fade")]
		/// the ID of the sound you want to fade. Has to match the ID you specified when playing the sound initially
		[Tooltip("the ID of the sound you want to fade. Has to match the ID you specified when playing the sound initially")]
		public int SoundID = 0;
		/// the duration of the fade, in seconds
		[Tooltip("the duration of the fade, in seconds")]
		public float FadeDuration = 1f;
		/// the volume towards which to fade
		[Tooltip("the volume towards which to fade")]
		[Range(MMSoundManagerSettings._minimalVolume,MMSoundManagerSettings._maxVolume)]
		public float FinalVolume = MMSoundManagerSettings._minimalVolume;
		/// the tween to apply over the fade
		[Tooltip("the tween to apply over the fade")]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
        
		protected AudioSource _targetAudioSource;
        
		/// <summary>
		/// On play, we start our fade via a fade event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			MMSoundManagerSoundFadeEvent.Trigger(SoundID, FadeDuration, FinalVolume, FadeTween);
		}
	}
}