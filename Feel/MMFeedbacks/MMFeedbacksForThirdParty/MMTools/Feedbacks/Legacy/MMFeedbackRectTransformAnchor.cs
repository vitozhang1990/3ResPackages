﻿using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 通过此反馈，可以随时间控制矩形变换的最小和最大定位。这是左下角和右上角定位到的父矩形变换中的归一化位置。
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("通过此反馈，可以随时间控制矩形变换的最小和最大定位。这是左下角和右上角定位到的父矩形变换中的归一化位置。")]
	[FeedbackPath("UI/RectTransform Anchor（控制矩形变换的最小和最大定位）")]
	public class MMFeedbackRectTransformAnchor : MMFeedbackBase
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		[Header("Target")]
		/// the target RectTransform to control
		[Tooltip("the target RectTransform to control")]
		public RectTransform TargetRectTransform;

		[Header("Anchor Min")]
		/// whether or not to modify the min anchor
		[Tooltip("whether or not to modify the min anchor")]
		public bool ModifyAnchorMin = true;
		/// the curve to animate the min anchor on
		[Tooltip("the curve to animate the min anchor on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType AnchorMinCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the min anchor curve's 0 on
		[Tooltip("the value to remap the min anchor curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 AnchorMinRemapZero = Vector2.zero;
		/// the value to remap the min anchor curve's 1 on
		[Tooltip("the value to remap the min anchor curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 AnchorMinRemapOne = Vector2.one;
        
		[Header("Anchor Max")]
		/// whether or not to modify the max anchor
		[Tooltip("whether or not to modify the max anchor")]
		public bool ModifyAnchorMax = true;
		/// the curve to animate the max anchor on
		[Tooltip("the curve to animate the max anchor on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType AnchorMaxCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the max anchor curve's 0 on
		[Tooltip("the value to remap the max anchor curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 AnchorMaxRemapZero = Vector2.zero;
		/// the value to remap the max anchor curve's 1 on
		[Tooltip("the value to remap the max anchor curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 AnchorMaxRemapOne = Vector2.one;
        
		protected override void FillTargets()
		{
			if (TargetRectTransform == null)
			{
				return;
			}
            
			MMFeedbackBaseTarget targetMin = new MMFeedbackBaseTarget();
			MMPropertyReceiver receiverMin = new MMPropertyReceiver();
			receiverMin.TargetObject = TargetRectTransform.gameObject;
			receiverMin.TargetComponent = TargetRectTransform;
			receiverMin.TargetPropertyName = "anchorMin";
			receiverMin.RelativeValue = RelativeValues;
			receiverMin.Vector2RemapZero = AnchorMinRemapZero;
			receiverMin.Vector2RemapOne = AnchorMinRemapOne;
			receiverMin.ShouldModifyValue = ModifyAnchorMin;
			targetMin.Target = receiverMin;
			targetMin.LevelCurve = AnchorMinCurve;
			targetMin.RemapLevelZero = 0f;
			targetMin.RemapLevelOne = 1f;
			targetMin.InstantLevel = 1f;

			_targets.Add(targetMin);
            
			MMFeedbackBaseTarget targetMax = new MMFeedbackBaseTarget();
			MMPropertyReceiver receiverMax = new MMPropertyReceiver();
			receiverMax.TargetObject = TargetRectTransform.gameObject;
			receiverMax.TargetComponent = TargetRectTransform;
			receiverMax.TargetPropertyName = "anchorMax";
			receiverMax.RelativeValue = RelativeValues;
			receiverMax.Vector2RemapZero = AnchorMaxRemapZero;
			receiverMax.Vector2RemapOne = AnchorMaxRemapOne;
			receiverMax.ShouldModifyValue = ModifyAnchorMax;
			targetMax.Target = receiverMax;
			targetMax.LevelCurve = AnchorMaxCurve;
			targetMax.RemapLevelZero = 0f;
			targetMax.RemapLevelOne = 1f;
			targetMax.InstantLevel = 1f;

			_targets.Add(targetMax);
		}

	}
}