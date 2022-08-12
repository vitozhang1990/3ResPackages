using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈将允许您在指定时间内转换到目标音频混合器快照
	/// This feedback will let you transition to a target AudioMixer Snapshot over a specified time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将允许您在指定时间内转换到目标音频混合器快照")]
	[FeedbackPath("Audio（音效）/AudioMixer Snapshot Transition（转换到目标音频混合器快照）")]
	public class MMFeedbackAudioMixerSnapshotTransition : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif
        
		[Header("AudioMixer Snapshot")]
		/// the target audio mixer snapshot we want to transition to 
		[Tooltip("the target audio mixer snapshot we want to transition to")]
		public AudioMixerSnapshot TargetSnapshot;
		/// the audio mixer snapshot we want to transition from, optional, only needed if you plan to play this feedback in reverse 
		[Tooltip("the audio mixer snapshot we want to transition from, optional, only needed if you plan to play this feedback in reverse")]
		public AudioMixerSnapshot OriginalSnapshot;
		/// the duration, in seconds, over which to transition to the selected snapshot
		[Tooltip("the duration, in seconds, over which to transition to the selected snapshot")]
		public float TransitionDuration = 1f;
        
		/// <summary>
		/// On play we transition to the selected snapshot
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetSnapshot == null)
			{
				return;
			}

			if (!NormalPlayDirection)
			{
				if (OriginalSnapshot != null)
				{
					OriginalSnapshot.TransitionTo(TransitionDuration);     
				}
				else
				{
					TargetSnapshot.TransitionTo(TransitionDuration);
				}
			}
			else
			{
				TargetSnapshot.TransitionTo(TransitionDuration);     
			}
		}
	}
}