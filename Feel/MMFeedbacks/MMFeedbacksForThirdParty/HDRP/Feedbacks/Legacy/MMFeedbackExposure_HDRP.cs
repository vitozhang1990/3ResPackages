﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// 此反馈允许您随时间控制HDRP曝光强度。它要求场景中有一个具有激活曝光的体积的对象和一个MMExposureShaker_HDRP组件。
	/// This feedback allows you to control HDRP exposure intensity over time.
	/// It requires you have in your scene an object with a Volume 
	/// with Exposure active, and a MMExposureShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("PostProcess（后处理）/Exposure HDRP（HDRP曝光强度）")]
	[FeedbackHelp("此反馈允许您随时间控制HDRP曝光强度。它要求场景中有一个具有激活曝光的体积的对象和一个MMExposureShaker_HDRP组件。")]
	public class MMFeedbackExposure_HDRP : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		#endif

		[Header("Exposure")]
		/// the channel to emit on
		[Tooltip("the channel to emit on")]
		public int Channel = 0;
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float Duration = 0.2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;

		[Header("Intensity")]
		/// the curve to animate the intensity on
		[Tooltip("the curve to animate the intensity on")]
		public AnimationCurve FixedExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0)); 
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapFixedExposureZero = 8.5f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapFixedExposureOne = 6f;
		/// whether or not to add to the initial intensity
		[Tooltip("whether or not to add to the initial intensity")]
		public bool RelativeFixedExposure = false;

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		/// <summary>
		/// Triggers a Exposure shake
		/// </summary>
		/// <param name="position"></param>
		/// <param name="attenuation"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			MMExposureShakeEvent_HDRP.Trigger(FixedExposure, FeedbackDuration, RemapFixedExposureZero, RemapFixedExposureOne, RelativeFixedExposure, intensityMultiplier,
				Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            
		}
        
		/// <summary>
		/// On stop we stop our transition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			MMExposureShakeEvent_HDRP.Trigger(FixedExposure, FeedbackDuration, RemapFixedExposureZero, RemapFixedExposureOne, RelativeFixedExposure, channel:Channel, stop:true);
            
		}
	}
}