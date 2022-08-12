using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈将“保持”或等待，直到所有以前的反馈都已执行，然后在指定的时间内暂停MMFeedbacks序列的执行
	/// this feedback will "hold", or wait, until all previous feedbacks have been executed, and will then pause the execution of your MMFeedbacks sequence, for the specified duration
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将“保持”或等待，直到所有以前的反馈都已执行，然后在指定的时间内暂停MMFeedbacks序列的执行")]
	[FeedbackPath("Pause/Holding Pause（等待所有以前的反馈执行完后暂停反馈序列）")]
	public class MMFeedbackHoldingPause : MMFeedbackPause
	{
		/// sets the color of this feedback in the inspector
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.HoldingPauseColor; } }
		#endif
		public override bool HoldingPause { get { return true; } }
                
		/// the duration of this feedback is the duration of the pause
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }
        
		/// <summary>
		/// On custom play we just play our pause
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			StartCoroutine(PlayPause());
		}
	}
}