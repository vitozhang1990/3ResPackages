using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 新版反馈基类
	/// </summary>
	[Serializable]
	public abstract class MMF_Feedback
	{
		#region Properties

		[MMFInspectorGroup("Feedback Settings（反馈设置）", true, 0, false, true)]
		/// whether or not this feedback is active
		[Tooltip("这个反馈是否活跃")]
		public bool Active = true;
		[HideInInspector]
		public int UniqueID;
		/// the name of this feedback to display in the inspector
		[Tooltip("要在检查器中显示的此反馈的名称")]
		public string Label = "MMFeedback";
		/// the ID of the channel on which this feedback will communicate 
		[Tooltip("此反馈将在其上通信的信道的ID")]
		public int Channel = 0;
		/// the chance of this feedback happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)
		[Tooltip("这种反馈发生的概率（百分比：100:一直发生，0:从未发生，50:每两次呼叫发生一次，等等）")]
		[Range(0,100)]
		public float Chance = 100f;
		/// use this color to customize the background color of the feedback in the MMF_Player's list
		[Tooltip("使用此颜色自定义MMF_ Player列表中反馈的背景颜色")]
		public Color DisplayColor = Color.black;
		/// a number of timing-related values (delay, repeat, etc)
		[Tooltip("许多定时相关值（延迟、重复等）")]
		public MMFeedbackTiming Timing;
		/// <summary>
		/// 反馈的所有者，在调用初始化方法时定义
		/// the Owner of the feedback, as defined when calling the Initialization method
		/// </summary>
		[HideInInspector]
		public MMF_Player Owner;
		[HideInInspector]
		/// <summary>
		/// 无论此反馈是否处于调试模式
		/// whether or not this feedback is in debug mode
		/// </summary>
		public bool DebugActive = false;
		/// <summary>
		/// 如果您的反馈应该暂停反馈序列的执行，请将此设置为true
		/// set this to true if your feedback should pause the execution of the feedback sequence
		/// </summary>
		public virtual IEnumerator Pause => null;
		/// <summary>
		/// 如果这是真的，该反馈将等待所有以前的反馈运行
		/// if this is true, this feedback will wait until all previous feedbacks have run
		/// </summary>
		public virtual bool HoldingPause => false;
		/// <summary>
		/// 如果这是真的，则该反馈将等待所有以前的反馈运行完毕，然后再次运行所有先前的反馈
		/// if this is true, this feedback will wait until all previous feedbacks have run, then run all previous feedbacks again
		/// </summary>
		public virtual bool LooperPause => false;
		/// <summary>
		/// 如果这是真的，该反馈将暂停并等待，直到对其父MMFeedbacks调用Resume（）以恢复执行
		/// if this is true, this feedback will pause and wait until Resume() is called on its parent MMFeedbacks to resume execution
		/// </summary>
		public virtual bool ScriptDrivenPause { get; set; }
		/// <summary>
		/// 如果该值为正值，则如果尚未通过脚本恢复反馈，反馈将在该持续时间后自动恢复
		/// if this is a positive value, the feedback will auto resume after that duration if it hasn't been resumed via script already
		/// </summary>
		public virtual float ScriptDrivenPauseAutoResume { get; set; }
		/// <summary>
		/// 如果这是真的，则该反馈将等待所有以前的反馈运行完毕，然后再次运行所有先前的反馈
		/// if this is true, this feedback will wait until all previous feedbacks have run, then run all previous feedbacks again
		/// </summary>
		public virtual bool LooperStart => false;
		/// <summary>
		/// 如果为true，则将显示通道属性，否则将隐藏该属性
		/// if this is true, the Channel property will be displayed, otherwise it'll be hidden        
		/// </summary>
		public virtual bool HasChannel => false;
        /// <summary>
        /// 
        /// </summary>
        public virtual bool HasCustomInspectors => false;

#if UNITY_EDITOR
        /// <summary>
        /// 反馈的可覆盖颜色，可根据反馈重新定义。白色是唯一保留的颜色，当保留为白色时，反馈将恢复为正常（浅肤色或暗肤色）
        /// an overridable color for your feedback, that can be redefined per feedback. White is the only reserved color, and the feedback will revert to 
        /// normal (light or dark skin) when left to White
        /// </summary>
        public virtual Color FeedbackColor { get => Color.white; }
#endif
		/// <summary>
		/// 如果此反馈此时处于冷却状态（因此无法播放），则返回true，否则返回false
		/// returns true if this feedback is in cooldown at this time (and thus can't play), false otherwise
		/// </summary>
		public virtual bool InCooldown { get { return (Timing.CooldownDuration > 0f) && (FeedbackTime - _lastPlayTimestamp < Timing.CooldownDuration); } }
		/// <summary>
		/// 如果这是真的，则当前正在播放此反馈
		/// if this is true, this feedback is currently playing
		/// </summary>
		public virtual bool IsPlaying { get; set; }
		/// <summary>
		/// 基于所选定时设置的时间（或未缩放时间）
		/// the time (or unscaled time) based on the selected Timing settings
		/// </summary>
		public virtual float FeedbackTime 
		{ 
			get 
			{
				#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					return (float)EditorApplication.timeSinceStartup;
				}
				#endif
	            
				if (Owner.ForceTimescaleMode)
				{
					if (Owner.ForcedTimescaleMode == TimescaleModes.Scaled)
					{
						return Time.time;
					}
					else
					{
						return Time.unscaledTime;
					} 
				}
	            
				if (Timing.TimescaleMode == TimescaleModes.Scaled)
				{
					return Time.time;
				}
				else
				{
					return Time.unscaledTime;
				}
			} 
		}
		/// <summary>
		/// 基于所选定时设置的增量时间（或未缩放增量时间）
		/// the delta time (or unscaled delta time) based on the selected Timing settings
		/// </summary>
		public virtual float FeedbackDeltaTime
		{
			get
			{
				if (Owner.ForceTimescaleMode)
				{
					if (Owner.ForcedTimescaleMode == TimescaleModes.Scaled)
					{
						return Time.deltaTime;
					}
					else
					{
						return Time.unscaledDeltaTime;
					} 
				}
				if (Owner.SkippingToTheEnd)
				{
					return float.MaxValue;
				}
				if (Timing.TimescaleMode == TimescaleModes.Scaled)
				{
					return Time.deltaTime;
				}
				else
				{
					return Time.unscaledDeltaTime;
				}
			}
		}


		/// <summary>
		/// 此反馈的总持续时间：
		/// The total duration of this feedback :
		/// total = initial delay + duration * (number of repeats + delay between repeats)  
		/// </summary>
		public virtual float TotalDuration
		{
			get
			{
				if ((Timing != null) && (!Timing.ContributeToTotalDuration))
				{
					return 0f;
				}
				float totalTime = 0f;

				if (Timing == null)
				{
					return 0f;
				}
                
				if (Timing.InitialDelay != 0)
				{
					totalTime += ApplyTimeMultiplier(Timing.InitialDelay);
				}
            
				totalTime += FeedbackDuration;

				if (Timing.NumberOfRepeats != 0)
				{
					float delayBetweenRepeats = ApplyTimeMultiplier(Timing.DelayBetweenRepeats); 
                    
					totalTime += (Timing.NumberOfRepeats * FeedbackDuration) + (Timing.NumberOfRepeats  * delayBetweenRepeats);
				}

				return totalTime;
			}
		}

		/// <summary>
		/// 用于确定反馈是否具有所需的全部功能，或是否需要额外设置的标志。
		/// 如果未准备好播放反馈，则此标志将用于在检查器中显示警告图标。
		/// A flag used to determine if a feedback has all it needs, or if it requires some extra setup.
		/// This flag will be used to display a warning icon in the inspector if the feedback is not ready to be played.
		/// </summary>
		public bool RequiresSetup { get { return _requiresSetup;  } }
		public string RequiredTarget { get { return _requiredTarget;  } }
		public virtual void CacheRequiresSetup() { _requiresSetup = EvaluateRequiresSetup(); _requiredTarget = RequiredTargetText == "" ? "" : "["+RequiredTargetText+"]"; }
		public virtual bool DrawGroupInspectors { get { return true;  } }
        
		public virtual string RequiresSetupText { get { return "This feedback requires some additional setup."; } }
		public virtual string RequiredTargetText { get { return ""; } }

		/// <summary>
		/// 覆盖此方法以确定反馈是否需要设置
		/// Override this method to determine if a feedback requires setup 
		/// </summary>
		/// <returns></returns>
		public virtual bool EvaluateRequiresSetup() { return false;  }
		/// <summary>
		/// 上次播放此反馈的时间戳
		/// the timestamp at which this feedback was last played
		/// </summary>
		public virtual float FeedbackStartedAt { get { return Application.isPlaying ? _lastPlayTimestamp : -1f; } }
        /// <summary>
        /// 反馈的感知持续时间，用于显示其进度条，意味着每个反馈都将用有意义的数据覆盖
        /// the perceived duration of the feedback, to be used to display its progress bar, meant to be overridden with meaningful data by each feedback
        /// </summary>
        public virtual float FeedbackDuration { get { return 0f; } set { } }
		/// <summary>
		/// 无论这种反馈现在是否起作用
		/// whether or not this feedback is playing right now
		/// </summary>
		public virtual bool FeedbackPlaying { get { return ((FeedbackStartedAt > 0f) && (Time.time - FeedbackStartedAt < FeedbackDuration)); } }

		protected float _lastPlayTimestamp = -1f;
		protected int _playsLeft;
		protected bool _initialized = false;
		protected Coroutine _playCoroutine;
		protected Coroutine _infinitePlayCoroutine;
		protected Coroutine _sequenceCoroutine;
		protected Coroutine _repeatedPlayCoroutine;
		protected bool _requiresSetup = false;
		protected string _requiredTarget = "";
        
		protected int _sequenceTrackID = 0;
		protected float _beatInterval;
		protected bool BeatThisFrame = false;
		protected int LastBeatIndex = 0;
		protected int CurrentSequenceIndex = 0;
		protected float LastBeatTimestamp = 0f;

		#endregion Properties

		#region Initialization（初始化）

		/// <summary>
		/// 初始化反馈及其定时相关变量
		/// Initializes the feedback and its timing related variables
		/// </summary>
		/// <param name="owner"></param>
		public virtual void Initialization(MMF_Player owner)
		{
			if (Timing == null)
			{
				Timing = new MMFeedbackTiming();
			}
			
			_lastPlayTimestamp = -1f;
			_initialized = true;
			Owner = owner;
			_playsLeft = Timing.NumberOfRepeats + 1;
            
			SetInitialDelay(Timing.InitialDelay);
			SetDelayBetweenRepeats(Timing.DelayBetweenRepeats);
			SetSequence(Timing.Sequence);

			CustomInitialization(owner);            
		}

		#endregion Initialization
        
		#region Play（播放）
        
		/// <summary>
		/// 播放这个反馈
		/// Plays the feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void Play(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active)
			{
				return;
			}

			if (!_initialized)
			{
				Debug.LogWarning("The " + this + " feedback is being played without having been initialized. Call Initialization() first.");
			}
            //是否正在冷却
			// we check the cooldown
			if (InCooldown)
			{
				return;
			}

			if (Timing.InitialDelay > 0f) 
			{
				_playCoroutine = Owner.StartCoroutine(PlayCoroutine(position, feedbacksIntensity));
			}
			else
			{
				RegularPlay(position, feedbacksIntensity);
				_lastPlayTimestamp = FeedbackTime;
			}  
		}

		/// <summary>
		/// 延迟反馈初始播放
		/// An internal coroutine delaying the initial play of the feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator PlayCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Timing.TimescaleMode == TimescaleModes.Scaled)
			{
				yield return MMFeedbacksCoroutine.WaitFor(Timing.InitialDelay);
			}
			else
			{
				yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.InitialDelay);
			}
			RegularPlay(position, feedbacksIntensity);
			_lastPlayTimestamp = FeedbackTime;
		}

		/// <summary>
		/// 定时播放
		/// Triggers delaying coroutines if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity">反馈强度</param>
		protected virtual void RegularPlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Chance == 0f)
			{
				return;
			}
			if (Chance != 100f)
			{
				// 随机概率检测
				// determine the odds
				float random = Random.Range(0f, 100f);
				if (random > Chance)
				{
					return;
				}
			}

			if (Timing.UseIntensityInterval)
			{
				if ((feedbacksIntensity < Timing.IntensityIntervalMin) || (feedbacksIntensity >= Timing.IntensityIntervalMax))
				{
					return;
				}
			}

			if (Timing.RepeatForever)
			{
				_infinitePlayCoroutine = Owner.StartCoroutine(InfinitePlay(position, feedbacksIntensity));
				return;
			}
			if (Timing.NumberOfRepeats > 0)
			{
				_repeatedPlayCoroutine = Owner.StartCoroutine(RepeatedPlay(position, feedbacksIntensity));
				return;
			}            
			if (Timing.Sequence == null)
			{
				CustomPlayFeedback(position, feedbacksIntensity);
			}
			else
			{
				_sequenceCoroutine = Owner.StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));
			}
            
		}

		/// <summary>
		/// 用于无休止地重复播放
		/// Internal coroutine used for repeated play without end
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator InfinitePlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			while (true)
			{
				if (Timing.Sequence == null)
				{
					CustomPlayFeedback(position, feedbacksIntensity);
					_lastPlayTimestamp = FeedbackTime;
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(Timing.DelayBetweenRepeats);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.DelayBetweenRepeats);
					}
				}
				else
				{
					_sequenceCoroutine = Owner.StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));

					float delay = ApplyTimeMultiplier(Timing.DelayBetweenRepeats) + Timing.Sequence.Length;
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(delay);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(delay);
					}
				}
			}
		}

		/// <summary>
		/// 用于重复播放
		/// Internal coroutine used for repeated play
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator RepeatedPlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			while (_playsLeft > 0)
			{
				_playsLeft--;
				if (Timing.Sequence == null)
				{
					CustomPlayFeedback(position, feedbacksIntensity);
					_lastPlayTimestamp = FeedbackTime;
                    
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(Timing.DelayBetweenRepeats);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(Timing.DelayBetweenRepeats);
					}
				}
				else
				{
					_sequenceCoroutine = Owner.StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));
                    
					float delay = ApplyTimeMultiplier(Timing.DelayBetweenRepeats) + Timing.Sequence.Length;
					if (Timing.TimescaleMode == TimescaleModes.Scaled)
					{
						yield return MMFeedbacksCoroutine.WaitFor(delay);
					}
					else
					{
						yield return MMFeedbacksCoroutine.WaitForUnscaled(delay);
					}
				}
			}
			_playsLeft = Timing.NumberOfRepeats + 1;
		}

		#endregion Play

		#region Sequence（反馈序列）

		/// <summary>
		/// 用于在序列上播放此反馈
		/// A coroutine used to play this feedback on a sequence
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator SequenceCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			yield return null;
			float timeStartedAt = FeedbackTime;
			float lastFrame = FeedbackTime;

			BeatThisFrame = false;
			LastBeatIndex = 0;
			CurrentSequenceIndex = 0;
			LastBeatTimestamp = 0f;

			if (Timing.Quantized)
			{
				while (CurrentSequenceIndex < Timing.Sequence.QuantizedSequence[0].Line.Count)
				{
					_beatInterval = 60f / Timing.TargetBPM;

					if ((FeedbackTime - LastBeatTimestamp >= _beatInterval) || (LastBeatTimestamp == 0f))
					{
						BeatThisFrame = true;
						LastBeatIndex = CurrentSequenceIndex;
						LastBeatTimestamp = FeedbackTime;

						for (int i = 0; i < Timing.Sequence.SequenceTracks.Count; i++)
						{
							if (Timing.Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID == Timing.TrackID)
							{
								CustomPlayFeedback(position, feedbacksIntensity);
							}
						}
						CurrentSequenceIndex++;
					}
					yield return null;
				}
			}
			else
			{
				while (FeedbackTime - timeStartedAt < Timing.Sequence.Length)
				{
					foreach (MMSequenceNote item in Timing.Sequence.OriginalSequence.Line)
					{
						if ((item.ID == Timing.TrackID) && (item.Timestamp >= lastFrame) && (item.Timestamp <= FeedbackTime - timeStartedAt))
						{
							CustomPlayFeedback(position, feedbacksIntensity);
						}
					}
					lastFrame = FeedbackTime - timeStartedAt;
					yield return null;
				}
			}
		}

		/// <summary>
		/// 使用此方法在运行时更改此反馈的顺序
		/// Use this method to change this feedback's sequence at runtime
		/// </summary>
		/// <param name="newSequence"></param>

		public virtual void SetSequence(MMSequence newSequence)
		{
			Timing.Sequence = newSequence;
			if (Timing.Sequence != null)
			{
				for (int i = 0; i < Timing.Sequence.SequenceTracks.Count; i++)
				{
					if (Timing.Sequence.SequenceTracks[i].ID == Timing.TrackID)
					{
						_sequenceTrackID = i;
					}
				}
			}
		}

		#endregion Sequence

		#region Controls（控制）

		/// <summary>
		/// 停止播放所有反馈。将停止重复反馈，并调用自定义停止实现
		/// Stops all feedbacks from playing. Will stop repeating feedbacks, and call custom stop implementations
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void Stop(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (_playCoroutine != null) { Owner.StopCoroutine(_playCoroutine); }
			if (_infinitePlayCoroutine != null) { Owner.StopCoroutine(_infinitePlayCoroutine); }
			if (_repeatedPlayCoroutine != null) { Owner.StopCoroutine(_repeatedPlayCoroutine); }            
			if (_sequenceCoroutine != null) { Owner.StopCoroutine(_sequenceCoroutine);  }

			_lastPlayTimestamp = -1f;
			_playsLeft = Timing.NumberOfRepeats + 1;
			if (Timing.InterruptsOnStop)
			{
				CustomStopFeedback(position, feedbacksIntensity);    
			}
		}

		/// <summary>
		/// 在跳到MMF_ Player末尾时调用，在所有反馈上调用自定义跳过
		/// Called when skipping to the end of MMF_Player, calls custom Skip on all feedbacks
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void SkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			CustomSkipToTheEnd(position, feedbacksIntensity);
		}

		/// <summary>
		/// 调用此反馈的自定义重置
		/// Calls this feedback's custom reset 
		/// </summary>
		public virtual void ResetFeedback()
		{
			_playsLeft = Timing.NumberOfRepeats + 1;
			CustomReset();
		}

		#endregion

		#region Time

		/// <summary>
		/// 使用此方法可以在运行时指定重复之间的新延迟
		/// Use this method to specify a new delay between repeats at runtime
		/// </summary>
		/// <param name="delay"></param>
		public virtual void SetDelayBetweenRepeats(float delay)
		{
			Timing.DelayBetweenRepeats = delay;
		}

		/// <summary>
		/// 使用此方法可以在运行时指定新的初始延迟
		/// Use this method to specify a new initial delay at runtime
		/// </summary>
		/// <param name="delay"></param>
		public virtual void SetInitialDelay(float delay)
		{
			Timing.InitialDelay = delay;
		}

		/// <summary>
		/// 返回在该反馈播放时间结束时评估曲线的t值
		/// Returns the t value at which to evaluate a curve at the end of this feedback's play time
		/// </summary>
		protected virtual float FinalNormalizedTime
		{
			get
			{
				return NormalPlayDirection ? 1f : 0f;
			}
		}

		/// <summary>
		/// 将主机MMFeedbacks的时间乘数应用于此反馈
		/// Applies the host MMFeedbacks' time multiplier to this feedback
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		protected virtual float ApplyTimeMultiplier(float duration)
		{
			if (Owner == null)
			{
				return 0f;
			}
			return Owner.ApplyTimeMultiplier(duration);
		}

		#endregion Time

		#region Direction（反馈播放方向相关处理）

		/// <summary>
		/// 基于此反馈的当前播放方向，返回归一化时间的新值
		/// Returns a new value of the normalized time based on the current play direction of this feedback
		/// </summary>
		/// <param name="normalizedTime"></param>
		/// <returns></returns>
		protected virtual float ApplyDirection(float normalizedTime)
		{
			return NormalPlayDirection ? normalizedTime : 1 - normalizedTime;
		}

		/// <summary>
		/// 如果此反馈应正常播放，则返回true；如果应在倒带中播放，则为false
		/// Returns true if this feedback should play normally, or false if it should play in rewind
		/// </summary>
		public virtual bool NormalPlayDirection
		{
			get
			{
				switch (Timing.PlayDirection)
				{
					case MMFeedbackTiming.PlayDirections.FollowMMFeedbacksDirection:
						return (Owner.Direction == MMF_Player.Directions.TopToBottom);
					case MMFeedbackTiming.PlayDirections.AlwaysNormal:
						return true;
					case MMFeedbackTiming.PlayDirections.AlwaysRewind:
						return false;
					case MMFeedbackTiming.PlayDirections.OppositeMMFeedbacksDirection:
						return !(Owner.Direction == MMF_Player.Directions.TopToBottom);
				}
				return true;
			}
		}

		/// <summary>
		/// 根据其mmFeedbackDirectionCondition设置，如果此反馈应在当前父MMFeedbacks方向播放，则返回true
		/// Returns true if this feedback should play in the current parent MMFeedbacks direction, according to its MMFeedbacksDirectionCondition setting
		/// </summary>
		public virtual bool ShouldPlayInThisSequenceDirection
		{
			get
			{
				switch (Timing.MMFeedbacksDirectionCondition)
				{
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.Always:
						return true;
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards:
						return (Owner.Direction == MMF_Player.Directions.TopToBottom);
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards:
						return (Owner.Direction == MMF_Player.Directions.BottomToTop);
				}
				return true;
			}
		}

		#endregion Direction

		#region Overrides（开发给子类从写的函数）

		/// <summary>
		/// 该方法描述了反馈所需的所有自定义初始化过程，以及主初始化方法
		/// This method describes all custom initialization processes the feedback requires, in addition to the main Initialization method
		/// </summary>
		/// <param name="owner"></param>
		protected virtual void CustomInitialization(MMF_Player owner) { }

		/// <summary>
		/// 该方法描述了当播放反馈时发生的情况
		/// This method describes what happens when the feedback gets played
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected abstract void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f);

		/// <summary>
		/// 此方法描述了当反馈停止时发生的情况
		/// This method describes what happens when the feedback gets stopped
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected virtual void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }

		/// <summary>
		/// 这个方法描述了当反馈被跳过到最后时会发生什么
		/// This method describes what happens when the feedback gets skipped to the end
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected virtual void CustomSkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f) { }

		/// <summary>
		/// 此方法描述了当反馈复位时发生的情况
		/// This method describes what happens when the feedback gets reset
		/// </summary>
		protected virtual void CustomReset() { }

		/// <summary>
		/// 使用此方法初始化您可能拥有的任何自定义属性
		/// Use this method to initialize any custom attributes you may have
		/// </summary>
		public virtual void InitializeCustomAttributes() { }

		#endregion Overrides

		#region Event functions（Unity 事件函数的映射）

		/// <summary>
		/// 当检查器中发生更改时触发
		/// Triggered when a change happens in the inspector
		/// </summary>
		public virtual void OnValidate()
		{
			InitializeCustomAttributes();
		}

		/// <summary>
		/// Triggered when that feedback gets destroyed
		/// </summary>
		public virtual void OnDestroy()
		{
            
		}

		/// <summary>
		/// Triggered when the host MMF Player gets disabled
		/// </summary>
		public virtual void OnDisable()
		{
	        
		}

		#endregion

	}    
}