﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 通过此反馈，您可以触发对目标MMRadioSignal的播放（通常由MMRadioBroadcaster使用，以发出一个值，然后可由MMRadio接收器收听。从此反馈中，您还可以指定持续时间、时间刻度和乘数。
	/// This feedback will let you trigger a play on a target MMRadioSignal (usually used by a MMRadioBroadcaster to emit a value that can then be listened to by MMRadioReceivers. From this feedback you can also specify a duration, timescale and multiplier.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("通过此反馈，您可以触发对目标MMRadioSignal的播放（通常由MMRadioBroadcaster使用，以发出一个值，然后可由MMRadio接收器收听。从此反馈中，您还可以指定持续时间、时间刻度和乘数。")]
	[FeedbackPath("GameObject（游戏对象）/MMRadioSignal（反馈事件广播）")]
	public class MMFeedbackRadioSignal : MMFeedback
	{
		/// the duration of this feedback is the duration of the light, or 0 if instant
		public override float FeedbackDuration { get { return 0f; } }

		/// The target MMRadioSignal to trigger
		[Tooltip("The target MMRadioSignal to trigger")]
		public MMRadioSignal TargetSignal;
		/// the timescale to operate on
		[Tooltip("the timescale to operate on")]
		public MMRadioSignal.TimeScales TimeScale = MMRadioSignal.TimeScales.Unscaled;
        
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float Duration = 1f;
		/// a global multiplier to apply to the end result of the combination
		[Tooltip("a global multiplier to apply to the end result of the combination")]
		public float GlobalMultiplier = 1f;

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif
        

		/// <summary>
		/// On Play we set the values on our target signal and make it start shaking its level
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active)
			{
				if (TargetSignal != null)
				{
					float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                    
					TargetSignal.Duration = Duration;
					TargetSignal.GlobalMultiplier = GlobalMultiplier * intensityMultiplier;
					TargetSignal.TimeScale = TimeScale;
					TargetSignal.StartShaking();
				}
			}
		}

		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active)
			{
				if (TargetSignal != null)
				{
					TargetSignal.Stop();
				}
			}
		}
	}
}