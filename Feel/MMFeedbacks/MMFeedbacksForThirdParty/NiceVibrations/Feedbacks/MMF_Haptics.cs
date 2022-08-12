using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// 添加此反馈，以便能够通过NiceSvibration库触发触觉反馈。它可以让您创建瞬时或连续振动，通过AHAP文件播放预设或高级模式，并在任何时候停止任何振动。此反馈已被弃用，只是为了避免从旧版本更新时出现错误。使用新的触觉反馈。
	/// Add this feedback to be able to trigger haptic feedbacks via the NiceVibration library.
	/// It'll let you create transient or continuous vibrations, play presets or advanced patterns via AHAP files, and stop any vibration at any time
	/// This feedback has been deprecated, and is just here to avoid errors in case you were to update from an old version. Use the new haptic feedbacks instead.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Haptics（触觉反馈）/Haptics DEPRECATED!（已弃用的旧版触觉反馈，请使用新版触觉反馈）")]
	[FeedbackHelp("添加此反馈，以便能够通过NiceSvibration库触发触觉反馈。它可以让您创建瞬时或连续振动，通过AHAP文件播放预设或高级模式，并在任何时候停止任何振动。此反馈已被弃用，只是为了避免从旧版本更新时出现错误。使用新的触觉反馈。")]
	public class MMF_Haptics : MMF_Feedback
	{
		[Header("Deprecated Feedback")] 
		/// if this is true, this feedback will output a warning when played
		public bool OutputDeprecationWarning = true;
	    
		/// <summary>
		/// When this feedback gets played
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active)
			{
				return;
			}

			if (OutputDeprecationWarning)
			{
				Debug.LogWarning(Owner.name + " : the haptic feedback on this object is using the old version of Nice Vibrations, and won't work anymore. Replace it with any of the new haptic feedbacks.");
			}
		}
	}
}