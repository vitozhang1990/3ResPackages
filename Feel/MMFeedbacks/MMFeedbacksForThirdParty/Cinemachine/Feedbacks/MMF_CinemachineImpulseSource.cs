﻿using UnityEngine;
using MoreMountains.Feedbacks;
#if MM_CINEMACHINE
using Cinemachine;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// 此反馈允许您在Cinemachine脉冲源上产生脉冲。你需要在相机上安装Cinemachine Pulse监听器才能正常工作。
	/// </summary>
	[AddComponentMenu("")]
	#if MM_CINEMACHINE
	[FeedbackPath("Camera（相机）/Cinemachine Impulse Source（在Cinemachine脉冲源上产生脉冲）")]
	#endif
	[FeedbackHelp("此反馈允许您在Cinemachine脉冲源上产生脉冲。你需要在相机上安装Cinemachine Pulse监听器才能正常工作。")]
	public class MMF_CinemachineImpulseSource : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#if MM_CINEMACHINE
		public override bool EvaluateRequiresSetup() { return (ImpulseSource == null); }
		public override string RequiredTargetText { get { return ImpulseSource != null ? ImpulseSource.name : "";  } }
		#endif
		public override string RequiresSetupText { get { return "This feedback requires that an ImpulseSource be set to be able to work properly. You can set one below."; } }
		#endif

		[MMFInspectorGroup("Cinemachine Impulse Source", true, 28)]

		/// the velocity to apply to the impulse shake
		[Tooltip("the velocity to apply to the impulse shake")]
		public Vector3 Velocity = new Vector3(1f,1f,1f);
		#if MM_CINEMACHINE
		/// the impulse definition to broadcast
		[Tooltip("the impulse definition to broadcast")]
		public CinemachineImpulseSource ImpulseSource;
		#endif
		/// whether or not to clear impulses (stopping camera shakes) when the Stop method is called on that feedback
		[Tooltip("whether or not to clear impulses (stopping camera shakes) when the Stop method is called on that feedback")]
		public bool ClearImpulseOnStop = false;
        
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			#if MM_CINEMACHINE
			if (ImpulseSource != null)
			{
				ImpulseSource.GenerateImpulse(Velocity);
			}
			#endif
		}

		/// <summary>
		/// Stops the animation if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || !ClearImpulseOnStop)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			#if MM_CINEMACHINE
			CinemachineImpulseManager.Instance.Clear();
			#endif
		}
	}
}