using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈可以作为暂停，也可以作为循环的起点。在下面添加一个反馈循环器（在几次反馈之后），您的反馈将在两者之间循环
	/// This feedback can act as a pause but also as a start point for your loops. Add a FeedbackLooper below this (and after a few feedbacks) and your MMFeedbacks will loop between both
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈可以作为暂停，也可以作为循环的起点。在下面添加一个反馈循环器（在几次反馈之后），您的反馈将在两者之间循环")]
	[FeedbackPath("Loop/Looper Start（循环开始或者暂停）")]
	public class MMF_LooperStart : MMF_Pause
	{
		/// sets the color of this feedback in the inspector
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.LooperStartColor; } }
		#endif
		public override bool LooperStart { get { return true; } }

		/// the duration of this feedback is the duration of the pause
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }

		/// <summary>
		/// Overrides the default value
		/// </summary>
		protected virtual void Reset()
		{
			PauseDuration = 0;
		}

		/// <summary>
		/// On play we run our pause
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active)
			{
				Owner.StartCoroutine(PlayPause());
			}
		}

	}
}