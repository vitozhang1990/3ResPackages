using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈将在指定的持续时间（秒）内冻结时间刻度。我通常选择0.01或0.02秒，但可以根据您的喜好随意调整。它需要场景中的MMTimeManager才能工作。
	/// This feedback will trigger a freeze frame event when played, pausing the game for the specified duration (usually short, but not necessarily)
	/// This feedback will freeze the timescale for the specified duration (in seconds). I usually go with 0.01s or 0.02s, but feel free to tweak it to your liking. It requires a MMTimeManager in your scene to work.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将在指定的持续时间（秒）内冻结时间刻度。我通常选择0.01或0.02秒，但可以根据您的喜好随意调整。它需要场景中的MMTimeManager才能工作。")]
	[FeedbackPath("Time/Freeze Frame(冻结时间帧)")]
	public class MMFeedbackFreezeFrame : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TimeColor; } }
		#endif

		[Header("Freeze Frame")]
		/// the duration of the freeze frame
		[Tooltip("the duration of the freeze frame")]
		public float FreezeFrameDuration = 0.02f;
		/// the minimum value the timescale should be at for this freeze frame to happen. This can be useful to avoid triggering freeze frames when the timescale is already frozen. 
		[Tooltip("the minimum value the timescale should be at for this freeze frame to happen. This can be useful to avoid triggering freeze frames when the timescale is already frozen.")]
		public float MinimumTimescaleThreshold = 0.1f;

		/// the duration of this feedback is the duration of the freeze frame
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(FreezeFrameDuration); } set { FreezeFrameDuration = value; } }

		/// <summary>
		/// On Play we trigger a freeze frame event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (Time.timeScale < MinimumTimescaleThreshold)
			{
				return;
			}
            
			MMFreezeFrameEvent.Trigger(FeedbackDuration);
		}
	}
}