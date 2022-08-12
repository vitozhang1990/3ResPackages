using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
#if MM_CINEMACHINE
using Cinemachine;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// 此反馈可让您触发Cinemachine脉冲清除，立即停止可能正在播放的任何脉冲。
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Camera（相机）/Cinemachine Impulse Clear（触发Cinemachine脉冲清除）")]
	//[FeedbackHelp("This feedback lets you trigger a Cinemachine Impulse clear, stopping instantly any impulse that may be playing.")]
	[FeedbackHelp("此反馈可让您触发Cinemachine脉冲清除，立即停止可能正在播放的任何脉冲。")]
	public class MMFeedbackCinemachineImpulseClear : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif

		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			#if MM_CINEMACHINE
			CinemachineImpulseManager.Instance.Clear();
			#endif
		}
	}
}