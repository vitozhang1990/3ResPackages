using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 播放时，此反馈将触发闪光事件（由MMFlash捕捉）
	/// This feedback will trigger a flash event (to be caught by a MMFlash) when played
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("在播放时，此反馈将播放MMFlash事件。" +
		"如果您创建了一个带有MMFlash组件的UI图像（请参见演示场景中的示例），它将截获该事件，并闪烁（通常您希望它占据整个屏幕，但这不是强制性的）。" +
		"在反馈的检查器中，您可以定义闪光灯的颜色、持续时间、alpha和闪光灯ID。您的反馈和MMFlash上的FlashID必须相同，才能使它们协同工作。这允许您在场景中使用多个MMFlash，并分别进行闪烁。")]
	[FeedbackPath("Camera（相机）/Flash（闪光）")]
	public class MMF_Flash : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText { get { return "Channel "+Channel;  } }
		#endif
		/// the duration of this feedback is the duration of the flash
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlashDuration); } set { FlashDuration = value; } }
		public override bool HasChannel => true;

		[MMFInspectorGroup("Flash", true, 37)]
		/// the color of the flash
		[Tooltip("the color of the flash")]
		public Color FlashColor = Color.white;
		/// the flash duration (in seconds)
		[Tooltip("the flash duration (in seconds)")]
		public float FlashDuration = 0.2f;
		/// the alpha of the flash
		[Tooltip("the alpha of the flash")]
		public float FlashAlpha = 1f;
		/// the ID of the flash (usually 0). You can specify on each MMFlash object an ID, allowing you to have different flash images in one scene and call them separately (one for damage, one for health pickups, etc)
		[Tooltip("the ID of the flash (usually 0). You can specify on each MMFlash object an ID, allowing you to have different flash images in one scene and call them separately (one for damage, one for health pickups, etc)")]
		public int FlashID = 0;

		/// <summary>
		/// On Play we trigger a flash event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			MMFlashEvent.Trigger(FlashColor, FeedbackDuration * intensityMultiplier, FlashAlpha, FlashID, Channel, Timing.TimescaleMode);
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
			MMFlashEvent.Trigger(FlashColor, FeedbackDuration, FlashAlpha, FlashID, Channel, Timing.TimescaleMode, stop:true);
		}
	}
}