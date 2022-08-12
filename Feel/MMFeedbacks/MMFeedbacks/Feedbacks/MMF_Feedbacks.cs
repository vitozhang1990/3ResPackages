using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈允许您触发目标MMFeedbacks，或特定范围内指定通道上的任何MMFeedbacks。你需要在他们身上安装一个反馈震动器MMFeedbacksShaker。
	/// This feedback allows you to trigger a target MMFeedbacks, or any MMFeedbacks on the specified Channel within a certain range. You'll need an MMFeedbacksShaker on them.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈允许您触发目标MMFeedbacks，或特定范围内指定通道上的任何MMFeedbacks。你需要在他们身上安装一个反馈震动器MMFeedbacksShaker。")]
	[FeedbackPath("GameObject（游戏对象）/Feedbacks Player（触发指定MMFeedbacks）")]
	public class MMF_Feedbacks : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override string RequiredTargetText { get { return "Channel "+Channel;  } }
		#endif
		/// the duration of this feedback is the duration of the light, or 0 if instant
		public override float FeedbackDuration 
		{
			get
			{
				if ((Mode == Modes.PlayTargetFeedbacks) && (TargetFeedbacks != null))
				{
					return TargetFeedbacks.TotalDuration;
				}
				else
				{
					return 0f;    
				}
			} 
		}
		public override bool HasChannel => true;
        
		public enum Modes { PlayFeedbacksInArea, PlayTargetFeedbacks }
        
		[MMFInspectorGroup("Feedbacks", true, 79)]
        
		/// the selected mode for this feedback
		[Tooltip("the selected mode for this feedback")]
		public Modes Mode = Modes.PlayFeedbacksInArea;
        
		/// a specific MMFeedbacks / MMF_Player to play
		[MMFEnumCondition("Mode", (int)Modes.PlayTargetFeedbacks)]
		[Tooltip("a specific MMFeedbacks / MMF_Player to play")]
		public MMFeedbacks TargetFeedbacks;
        
		/// whether or not to use a range
		[MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
		[Tooltip("whether or not to use a range")]
		public bool UseRange = false;
		/// the range of the event, in units
		[MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
		[Tooltip("the range of the event, in units")]
		public float EventRange = 100f;
		/// the transform to use to broadcast the event as origin point
		[MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
		[Tooltip("the transform to use to broadcast the event as origin point")]
		public Transform EventOriginTransform;

		/// <summary>
		/// On init we turn the light off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
            
			if (EventOriginTransform == null)
			{
				EventOriginTransform = owner.transform;
			}
		}

		/// <summary>
		/// On Play we turn our light on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Mode == Modes.PlayFeedbacksInArea)
			{
				MMFeedbacksShakeEvent.Trigger(Channel, UseRange, EventRange, EventOriginTransform.position);    
			}
			else if (Mode == Modes.PlayTargetFeedbacks)
			{
				TargetFeedbacks?.PlayFeedbacks(position, feedbacksIntensity);
			}
		}
	}
}