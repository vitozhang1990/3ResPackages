using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 允许您在播放时更改（3D）相机的缩放
	/// A feedback that will allow you to change the zoom of a (3D) camera when played
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("定义缩放属性：对于将在特定时间内将缩放设置为指定参数的对象，设置将永远保持这种状态。" +
		"缩放属性包括视场、缩放过渡的持续时间（秒）和缩放持续时间（相机应保持放大的时间，秒）。" +
		"要使此功能正常工作，您需要在相机中添加MMCameraZoom组件，如果您使用的是虚拟相机，则需要添加MMCinemachineZoom组件。")]
	[FeedbackPath("Camera（相机）/Camera Zoom（相机缩放）")]
	public class MMF_CameraZoom : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText { get { return "Channel "+Channel;  } }
		#endif

		/// the duration of this feedback is the duration of the zoom
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(ZoomDuration); } set { ZoomDuration = value; } }
		public override bool HasChannel => true;

		[MMFInspectorGroup("Camera Zoom", true, 72)]
		/// the zoom mode (for : forward for TransitionDuration, static for Duration, backwards for TransitionDuration)
		[Tooltip("the zoom mode (for : forward for TransitionDuration, static for Duration, backwards for TransitionDuration)")]
		public MMCameraZoomModes ZoomMode = MMCameraZoomModes.For;
		/// the target field of view
		[Tooltip("the target field of view")]
		public float ZoomFieldOfView = 30f;
		/// the zoom transition duration
		[Tooltip("the zoom transition duration")]
		public float ZoomTransitionDuration = 0.05f;
		/// the duration for which the zoom is at max zoom
		[Tooltip("the duration for which the zoom is at max zoom")]
		public float ZoomDuration = 0.1f;
		/// whether or not ZoomFieldOfView should add itself to the current camera's field of view value
		[Tooltip("whether or not ZoomFieldOfView should add itself to the current camera's field of view value")]
		public bool RelativeFieldOfView = false;

		/// <summary>
		/// On Play, triggers a zoom event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, FeedbackDuration, Channel, Timing.TimescaleMode == TimescaleModes.Unscaled, false, RelativeFieldOfView);
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
			MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, FeedbackDuration, Channel, Timing.TimescaleMode == TimescaleModes.Unscaled, stop:true);
		}
	}
}