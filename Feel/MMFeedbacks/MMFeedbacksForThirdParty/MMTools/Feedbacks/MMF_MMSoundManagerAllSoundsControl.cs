using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 用于同时控制MMSoundManager上播放的所有声音的反馈。它可以让您暂停、播放、停止和释放（停止并将音频源返回到池中）声音。您需要在场景中安装一个MMSoundManager才能正常工作。
	/// A feedback used to control all sounds playing on the MMSoundManager at once. It'll let you pause, play, stop and free (stop and returns the audiosource to the pool) sounds.  You will need a MMSoundManager in your scene for this to work.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Audio（音效）/MMSoundManager All Sounds Control（所有声音控制器）")]
	[FeedbackHelp("用于同时控制MMSoundManager上播放的所有声音的反馈。它可以让您暂停、播放、停止和释放（停止并将音频源返回到池中）声音。您需要在场景中安装一个MMSoundManager才能正常工作。")]
	public class MMF_MMSoundManagerAllSoundsControl : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get { return ControlMode.ToString();  } }
		#endif
        
		[MMFInspectorGroup("MMSoundManager All Sounds Control", true, 30)]
		/// The selected control mode. 
		[Tooltip("The selected control mode")]
		public MMSoundManagerAllSoundsControlEventTypes ControlMode = MMSoundManagerAllSoundsControlEventTypes.Pause;

		/// <summary>
		/// On Play, we call the specified event, to be caught by the MMSoundManager
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			switch (ControlMode)
			{
				case MMSoundManagerAllSoundsControlEventTypes.Pause:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Pause);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Play:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Play);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Stop:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Stop);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Free:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Free);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.FreeAllButPersistent:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.FreeAllButPersistent);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.FreeAllLooping:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.FreeAllLooping);
					break;
			}
		}
	}
}