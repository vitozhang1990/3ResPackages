using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈触发MMTimeScale事件，如果场景中有MMTimeManager对象，将捕获该事件，并根据指定的设置使用该事件修改时间刻度。这些设置是新的时间刻度（0.5将比正常慢两倍，2倍快，等等），时间刻度修改的持续时间，以及在正常和更改的时间刻度之间转换的可选速度。
	/// This feedback changes the timescale by sending a TimeScale event on play
	/// This feedback triggers a MMTimeScaleEvent, which, if you have a MMTimeManager object in your scene, will be caught and used to modify the timescale according to the specified settings. These settings are the new timescale (0.5 will be twice slower than normal, 2 twice faster, etc), the duration of the timescale modification, and the optional speed at which to transition between normal and altered time scale.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈触发MMTimeScale事件，如果场景中有MMTimeManager对象，将捕获该事件，并根据指定的设置使用该事件修改时间刻度。这些设置是新的时间刻度（0.5将比正常慢两倍，2倍快，等等），时间刻度修改的持续时间，以及在正常和更改的时间刻度之间转换的可选速度。")]
	[FeedbackPath("Time/Timescale Modifier（时间刻度修改器）")]
	public class MMFeedbackTimescaleModifier : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// <summary>
		/// The possible modes for this feedback :
		/// - shake : changes the timescale for a certain duration
		/// - change : sets the timescale to a new value, forever (until you change it again)
		/// - reset : resets the timescale to its previous value
		/// </summary>
		public enum Modes { Shake, Change, Reset }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TimeColor; } }
		#endif

		[Header("Mode")]
		/// the selected mode
		[Tooltip("the selected mode : shake : changes the timescale for a certain duration" +
		         "- change : sets the timescale to a new value, forever (until you change it again)" +
		         "- reset : resets the timescale to its previous value")]
		public Modes Mode = Modes.Shake;

		[Header("Timescale Modifier")]
		/// the new timescale to apply
		[Tooltip("the new timescale to apply")]
		public float TimeScale = 0.5f;
		/// the duration of the timescale modification
		[Tooltip("the duration of the timescale modification")]
		[MMFEnumCondition("Mode", (int)Modes.Shake)]
		public float TimeScaleDuration = 1f;
		/// whether or not we should lerp the timescale
		[Tooltip("whether or not we should lerp the timescale")]
		[MMFEnumCondition("Mode", (int)Modes.Shake, (int)Modes.Change)]
		public bool TimeScaleLerp = false;
		/// the speed at which to lerp the timescale
		[Tooltip("the speed at which to lerp the timescale")]
		[MMFEnumCondition("Mode", (int)Modes.Shake, (int)Modes.Change)]
		public float TimeScaleLerpSpeed = 1f;
		/// whether to reset the timescale on Stop or not
		[Tooltip("whether to reset the timescale on Stop or not")]
		public bool ResetTimescaleOnStop = false;


		/// the duration of this feedback is the duration of the time modification
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(TimeScaleDuration); } set { TimeScaleDuration = value; } }

		/// <summary>
		/// On Play, triggers a time scale event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			switch (Mode)
			{
				case Modes.Shake:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, FeedbackDuration, TimeScaleLerp, TimeScaleLerpSpeed, false);
					break;
				case Modes.Change:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, 0f, TimeScaleLerp, TimeScaleLerpSpeed, true);
					break;
				case Modes.Reset:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
					break;
			}     
		}

		/// <summary>
		/// On stop, we reset timescale if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || !ResetTimescaleOnStop)
			{
				return;
			}
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
		}
	}
}