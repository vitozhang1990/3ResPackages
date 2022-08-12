using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 新版反馈组（用于替换老版本反馈组MMFeedbacks）
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/MMF Player")]
	[DisallowMultipleComponent] 
	public class MMF_Player : MMFeedbacks
	{
		#region PROPERTIES
        
		[SerializeReference]
		public List<MMF_Feedback> FeedbacksList;
        
		public override float TotalDuration
		{
			get
			{
				float total = 0f;
				if (FeedbacksList == null)
				{
					return InitialDelay;
				}
				foreach (MMF_Feedback feedback in FeedbacksList)
				{
					if ((feedback != null) && (feedback.Active))
					{
						if (total < feedback.TotalDuration)
						{
							total = feedback.TotalDuration;    
						}
					}
				}
				return InitialDelay + total;
			}
		}

		public bool KeepPlayModeChanges = false;
		[Tooltip("如果这是真的，那么当反馈播放时，检查器将不会刷新，这将节省性能，但反馈检查器的进度条看起来将不会那么平滑")]
		public bool PerformanceMode = false;
		[Tooltip("如果这是真的，则将对禁用（OnDisable）上的所有反馈调用StopFeedbacks")]
		public bool ForceStopFeedbacksOnDisable = true;

		public bool SkippingToTheEnd { get; protected set; }
        
		protected Type _t;

		#endregion

		#region INITIALIZATION

		/// <summary>
		/// 在唤醒时，如果处于自动模式，则初始化反馈
		/// On Awake we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected override void Awake()
		{
			//如果我们的MMFeedbacks处于自动布局模式，我们会向它添加一个小助手，如果父游戏对象被关闭并再次打开，它将在需要时重新启用
			// if our MMFeedbacks is in AutoPlayOnEnable mode, we add a little helper to it that will re-enable it if needed if the parent game object gets turned off and on again
			if (AutoPlayOnEnable)
			{
				MMF_PlayerEnabler playerEnabler = GetComponent<MMF_PlayerEnabler>(); 
				if (playerEnabler == null)
				{
					playerEnabler = this.gameObject.AddComponent<MMF_PlayerEnabler>();
				}
				playerEnabler.TargetMmfPlayer = this; 
			}
            
			if ((InitializationMode == InitializationModes.Awake) && (Application.isPlaying))
			{
				Initialization();
			}
			
			InitializeFeedbackList();
			ExtraInitializationChecks();
			CheckForLoops();
		}

		/// <summary>
		/// 在开始时，如果我们处于自动模式，我们将初始化反馈
		/// On Start we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected override void Start()
		{
			if ((InitializationMode == InitializationModes.Start) && (Application.isPlaying))
			{
				Initialization();
			}
			if (AutoPlayOnStart && Application.isPlaying)
			{
				PlayFeedbacks();
			}
			CheckForLoops();
		}

		/// <summary>
		/// 初始化反馈列表
		/// We initialize our list of feedbacks
		/// </summary>
		protected virtual void InitializeFeedbackList()
		{
			if (FeedbacksList == null)
			{
				FeedbacksList = new List<MMF_Feedback>();
			}
		}

		/// <summary>
		/// 执行额外检查，主要用于覆盖动态创建的情况
		/// Performs extra checks, mostly to cover cases of dynamic creation
		/// </summary>
		protected virtual void ExtraInitializationChecks()
		{
			if (Events == null)
			{
				Events = new MMFeedbacksEvents();
				Events.Initialization();
			}
		}

		/// <summary>
		/// 启用时，如果处于自动模式，则初始化反馈
		/// On Enable we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected override void OnEnable()
		{
			if (AutoPlayOnEnable && Application.isPlaying)
			{
				PlayFeedbacks();
			}
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				feedback.CacheRequiresSetup();
			}
		}

		/// <summary>
		/// 用于初始化反馈的公共方法，指定所有者，该所有者将用作反馈位置和层次结构的参考
		/// A public method to initialize the feedback, specifying an owner that will be used as the reference for position and hierarchy by feedbacks
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="feedbacksOwner"></param>
		public override void Initialization()
		{
			SkippingToTheEnd = false;
			IsPlaying = false;
			_lastStartAt = -float.MaxValue;

			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i] != null)
				{
					FeedbacksList[i].Initialization(this);
				}                
			}
		}

		#endregion

		#region PLAY

		/// <summary>
		/// 使用MMFeedbacks的位置作为参考播放所有反馈，无衰减
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation
		/// </summary>
		public override void PlayFeedbacks()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity);
		}

		/// <summary>
		/// 播放所有反馈，指定位置和强度。该位置可由每个反馈使用，并被考虑用于例如激发粒子或播放声音。
		/// 反馈强度是每个反馈可以用来降低其强度的一个因子，通常，您需要根据时间或距离定义衰减（使用较低的强度值表示距离玩家较远的反馈）。此外，您可以强制反馈反向播放，忽略其当前条件
		/// Plays all feedbacks, specifying a position and intensity. The position may be used by each Feedback and taken into account to spark a particle or play a sound for example.
		/// The feedbacks intensity is a factor that can be used by each Feedback to lower its intensity, usually you'll want to define that attenuation based on time or distance (using a lower 
		/// intensity value for feedbacks happening further away from the Player).
		/// Additionally you can force the feedback to play in reverse, ignoring its current condition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksOwner"></param>
		/// <param name="feedbacksIntensity"></param>
		public override void PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

		/// <summary>
		/// 使用MMFeedbacks的位置作为参考播放所有反馈，不衰减，反向播放（从下到上）
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
		/// </summary>
		public override void PlayFeedbacksInReverse()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity, true);
		}

		/// <summary>
		/// 使用MMFeedbacks的位置作为参考播放所有反馈，不衰减，反向播放（从下到上）
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
		/// </summary>
		public override void PlayFeedbacksInReverse(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

		/// <summary>
		/// 播放序列中的所有反馈，但仅当该反馈按相反顺序播放时
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in reverse order
		/// </summary>
		public override void PlayFeedbacksOnlyIfReversed()
		{
            
			if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
			     || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
			{
				PlayFeedbacks();
			}
		}

		/// <summary>
		/// 播放序列中的所有反馈，但仅当该反馈按相反顺序播放时(带参数)
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in reverse order
		/// </summary>
		public override void PlayFeedbacksOnlyIfReversed(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
            
			if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
			     || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
			{
				PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			}
		}

		/// <summary>
		/// 播放序列中的所有反馈，但仅当该反馈按正常顺序播放时
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in normal order
		/// </summary>
		public override void PlayFeedbacksOnlyIfNormalDirection()
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks();
			}
		}

		/// <summary>
		/// 播放序列中的所有反馈，但仅当该反馈按正常顺序播放时（带参数）
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in normal order
		/// </summary>
		public override void PlayFeedbacksOnlyIfNormalDirection(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			}
		}

		/// <summary>
		/// 当您想在自己的协同程序中让步，直到MMFeedbacks停止播放时，您可以从外部调用的公共协同程序。通常情况下：yield return myFeedback.playFeedbackcoroutine（this.transform.position，1.0f，false）；
		/// A public coroutine you can call externally when you want to yield in a coroutine of yours until the MMFeedbacks has stopped playing
		/// typically : yield return myFeedback.PlayFeedbacksCoroutine(this.transform.position, 1.0f, false);
		/// </summary>
		/// <param name="position">The position at which the MMFeedbacks should play</param>
		/// <param name="feedbacksIntensity">The intensity of the feedback</param>
		/// <param name="forceRevert">Whether or not the MMFeedbacks should play in reverse or not</param>
		/// <returns></returns>
		public override IEnumerator PlayFeedbacksCoroutine(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			while (IsPlaying)
			{
				yield return null;    
			}
		}

		#endregion

		#region SEQUENCE

		/// <summary>
		/// 用于播放反馈的内部方法不应在外部调用
		/// An internal method used to play feedbacks, shouldn't be called externally
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void PlayFeedbacksInternal(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			if (!CanPlay)
			{
				return;
			}
			
			if (IsPlaying && !CanPlayWhileAlreadyPlaying)
			{
				return;
			}

			if (!EvaluateChance())
			{
				return;
			}
			//如果我们有冷却时间，我们会在需要时阻止执行
			// if we have a cooldown we prevent execution if needed
			if (CooldownDuration > 0f)
			{
				if (GetTime() - _lastStartAt < CooldownDuration)
				{
					return;
				}
			}
            
			SkippingToTheEnd = false;
			//如果所有反馈都被全局禁用，我们将停止播放
			// if all MMFeedbacks are disabled globally, we stop and don't play
			if (!GlobalMMFeedbacksActive)
			{
				return;
			}

			if (!this.gameObject.activeInHierarchy)
			{
				return;
			}
            
			if (ShouldRevertOnNextPlay)
			{
				Revert();
				ShouldRevertOnNextPlay = false;
			}

			if (forceRevert)
			{
				Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
			}
            
			ResetFeedbacks();
			this.enabled = true;
			IsPlaying = true;
			_startTime = GetTime();
			_lastStartAt = _startTime;
			_totalDuration = TotalDuration;
            
			if (Time.frameCount < 2)
			{
				StartCoroutine(FrameOnePlayCo(position, feedbacksIntensity, forceRevert));
				return;
			}

			if (InitialDelay > 0f)
			{
				StartCoroutine(HandleInitialDelayCo(position, feedbacksIntensity, forceRevert));
			}
			else
			{
				PreparePlay(position, feedbacksIntensity, forceRevert);
			}
		}
        /// <summary>
		/// 第一帧播放
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="forceRevert"></param>
		/// <returns></returns>
		protected virtual IEnumerator FrameOnePlayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			yield return null;
			_startTime = GetTime();
			_lastStartAt = _startTime;
			IsPlaying = true;
			yield return MMFeedbacksCoroutine.WaitForUnscaled(InitialDelay);
			PreparePlay(position, feedbacksIntensity, forceRevert);
		}
		/// <summary>
		/// 准备播放
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="forceRevert"></param>
		protected override void PreparePlay(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			Events.TriggerOnPlay(this);

			_holdingMax = 0f;
			//测试是否发现暂停或保持暂停
			// test if a pause or holding pause is found
			_pauseFound = false;
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i] != null)
				{
					if ((FeedbacksList[i].Pause != null) && (FeedbacksList[i].Active) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}
					if ((FeedbacksList[i].HoldingPause == true) && (FeedbacksList[i].Active) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}    
				}
			}
			
			if (!_pauseFound)
			{
				PlayAllFeedbacks(position, feedbacksIntensity, forceRevert);
			}
			else
			{
				// 如果发现至少一个暂停
				// if at least one pause was found
				StartCoroutine(PausedFeedbacksCo(position, feedbacksIntensity));
			}
		}
		/// <summary>
		/// 播放所有反馈
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="forceRevert"></param>
		protected override void PlayAllFeedbacks(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			// 如果没有发现暂停，我们就一次播放所有反馈
			// if no pause was found, we just play all feedbacks at once
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbackCanPlay(FeedbacksList[i]))
				{
					FeedbacksList[i].Play(position, feedbacksIntensity);
				}
			}
		}
		/// <summary>
		/// 处理初始延迟
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="forceRevert"></param>
		/// <returns></returns>
		protected override IEnumerator HandleInitialDelayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			IsPlaying = true;
			yield return MMFeedbacksCoroutine.WaitForUnscaled(InitialDelay);
			PreparePlay(position, feedbacksIntensity, forceRevert);
		}
        
		protected override void Update()
		{
			if (_shouldStop)
			{
				if (HasFeedbackStillPlaying())
				{
					return;
				}
				IsPlaying = false;
				Events.TriggerOnComplete(this);
				ApplyAutoRevert();
				this.enabled = false;
				_shouldStop = false;
			}
			if (IsPlaying)
			{
				if (!_pauseFound)
				{
					if (GetTime() - _startTime > _totalDuration)
					{
						_shouldStop = true;
					}    
				}
			}
			else
			{
				this.enabled = false;
			}
		}

		/// <summary>
		/// 用于在涉及暂停时处理反馈序列
		/// A coroutine used to handle the sequence of feedbacks if pauses are involved
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected override IEnumerator PausedFeedbacksCo(Vector3 position, float feedbacksIntensity)
		{
			IsPlaying = true;

			int i = (Direction == Directions.TopToBottom) ? 0 : FeedbacksList.Count-1;

			int count = FeedbacksList.Count;
			while ((i >= 0) && (i < count))
			{
				if (!IsPlaying)
				{
					yield break;
				}

				if (FeedbacksList[i] == null)
				{
					yield break;
				}
                
				if (((FeedbacksList[i].Active) && (FeedbacksList[i].ScriptDrivenPause)) || InScriptDrivenPause)
				{
					InScriptDrivenPause = true;

					bool inAutoResume = (FeedbacksList[i].ScriptDrivenPauseAutoResume > 0f); 
					float scriptDrivenPauseStartedAt = GetTime();
					float autoResumeDuration = FeedbacksList[i].ScriptDrivenPauseAutoResume;
                    
					while (InScriptDrivenPause)
					{
						if (inAutoResume && (GetTime() - scriptDrivenPauseStartedAt > autoResumeDuration))
						{
							ResumeFeedbacks();
						}
						yield return null;
					} 
				}
				// 控制暂停
				// handles holding pauses
				if ((FeedbacksList[i].Active)
				    && ((FeedbacksList[i].HoldingPause == true) || (FeedbacksList[i].LooperPause == true))
				    && (FeedbacksList[i].ShouldPlayInThisSequenceDirection))
				{
					Events.TriggerOnPause(this);
					// 我们一直呆在这里，直到所有以前的反馈都完成
					// we stay here until all previous feedbacks have finished
					while (GetTime() - _lastStartAt < _holdingMax)
					{
						yield return null;
					}
					_holdingMax = 0f;
					_lastStartAt = GetTime();
				}
				// 播放这个反馈
				// plays the feedback
				if (FeedbackCanPlay(FeedbacksList[i]))
				{
					FeedbacksList[i].Play(position, feedbacksIntensity);
				}
				// 处理暂停
				// Handles pause
				if ((FeedbacksList[i].Pause != null) && (FeedbacksList[i].Active) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection))
				{
					bool shouldPause = true;
					if (FeedbacksList[i].Chance < 100)
					{
						float random = Random.Range(0f, 100f);
						if (random > FeedbacksList[i].Chance)
						{
							shouldPause = false;
						}
					}

					if (shouldPause)
					{
						yield return FeedbacksList[i].Pause;
						Events.TriggerOnResume(this);
						_lastStartAt = GetTime();
						_holdingMax = 0f;
					}
				}
				// 更新保持最大值
				// updates holding max
				if (FeedbacksList[i].Active)
				{
					if ((FeedbacksList[i].Pause == null) && (FeedbacksList[i].ShouldPlayInThisSequenceDirection) && (!FeedbacksList[i].Timing.ExcludeFromHoldingPauses))
					{
						float feedbackDuration = FeedbacksList[i].TotalDuration;
						_holdingMax = Mathf.Max(feedbackDuration, _holdingMax);
					}
				}
				// 处理循环
				// handles looper
				if ((FeedbacksList[i].LooperPause == true)
				    && (FeedbacksList[i].Active)
				    && (FeedbacksList[i].ShouldPlayInThisSequenceDirection)
				    && (((FeedbacksList[i] as MMF_Looper).NumberOfLoopsLeft > 0) || (FeedbacksList[i] as MMF_Looper).InInfiniteLoop))
				{
					while (HasFeedbackStillPlaying())
					{
						yield return null;
					}
					// 确定了应该重新开始的指数
					// we determine the index we should start again at
					bool loopAtLastPause = (FeedbacksList[i] as MMF_Looper).LoopAtLastPause;
					bool loopAtLastLoopStart = (FeedbacksList[i] as MMF_Looper).LoopAtLastLoopStart;
                    
					int newi = 0;

					int j = (Direction == Directions.TopToBottom) ? i - 1 : i + 1;

					int listCount = FeedbacksList.Count;
					while ((j >= 0) && (j <= listCount))
					{
						// 如果在一开始
						// if we're at the start
						if (j == 0)
						{
							newi = j - 1;
							break;
						}
						if (j == listCount)
						{
							newi = j ;
							break;
						}
						//如果是暂停
						// if we've found a pause
						if ((FeedbacksList[j].Pause != null)
						    && (FeedbacksList[j].FeedbackDuration > 0f)
						    && loopAtLastPause && (FeedbacksList[j].Active))
						{
							newi = j;
							break;
						}
						//如果是循环开始
						// if we've found a looper start
						if ((FeedbacksList[j].LooperStart == true)
						    && loopAtLastLoopStart
						    && (FeedbacksList[j].Active))
						{
							newi = j;
							break;
						}

						j += (Direction == Directions.TopToBottom) ? -1 : 1;
					}
					i = newi;
				}
				i += (Direction == Directions.TopToBottom) ? 1 : -1;
			}
			float unscaledTimeAtEnd = GetTime();
			while (GetTime() - unscaledTimeAtEnd < _holdingMax)
			{
				yield return null;
			}
			while (HasFeedbackStillPlaying())
			{
				yield return null;
			}
			IsPlaying = false;
			Events.TriggerOnComplete(this);
			ApplyAutoRevert();
		}
		/// <summary>
		/// 跳到最后
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator SkipToTheEndCo()
		{
			SkippingToTheEnd = true;
			Events.TriggerOnSkip(this);
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].Active))
				{
					FeedbacksList[i].SkipToTheEnd(this.transform.position);    
				}
			}
			yield return null;
			yield return null;
			SkippingToTheEnd = false;
			StopFeedbacks();
		}

		#endregion

		#region STOP（停止）

		/// <summary>
		/// 停止播放所有其他反馈，而不停止单个反馈
		/// Stops all further feedbacks from playing, without stopping individual feedbacks 
		/// </summary>
		public override void StopFeedbacks()
		{
			StopFeedbacks(true);
		}

		/// <summary>
		/// 停止播放所有反馈，还可以选择停止个别反馈
		/// Stops all feedbacks from playing, with an option to also stop individual feedbacks
		/// </summary>
		public override void StopFeedbacks(bool stopAllFeedbacks = true)
		{
			StopFeedbacks(this.transform.position, 1.0f, stopAllFeedbacks);
		}

		/// <summary>
		/// 停止播放所有反馈，指定反馈可以使用的位置和强度
		/// Stops all feedbacks from playing, specifying a position and intensity that can be used by the Feedbacks 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public override void StopFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool stopAllFeedbacks = true)
		{
			if (stopAllFeedbacks)
			{
				int count = FeedbacksList.Count;
				for (int i = 0; i < count; i++)
				{
					FeedbacksList[i].Stop(position, feedbacksIntensity);
				}    
			}
			IsPlaying = false;
			StopAllCoroutines();
		}

		#endregion

		#region CONTROLS（控制）

		/// <summary>
		/// 调用每个反馈的重置方法，如果它们已经定义了一个。其中一个例子是重置闪烁渲染器的初始颜色。
		/// Calls each feedback's Reset method if they've defined one. An example of that can be resetting the initial color of a flickering renderer.
		/// </summary>
		public override void ResetFeedbacks()
		{
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if ((FeedbacksList[i] != null) && (FeedbacksList[i].Active))
				{
					FeedbacksList[i].ResetFeedback();    
				}
			}
			IsPlaying = false;
		}

		/// <summary>
		/// 更改此反馈的方向
		/// Changes the direction of this MMFeedbacks
		/// </summary>
		public override void Revert()
		{
			Events.TriggerOnRevert(this);
			Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
		}

		/// <summary>
		/// 暂停序列的执行，然后可以通过调用ResumeFeedbacks（）继续执行序列
		/// Pauses execution of a sequence, which can then be resumed by calling ResumeFeedbacks()
		/// </summary>
		public override void PauseFeedbacks()
		{
			Events.TriggerOnPause(this);
			InScriptDrivenPause = true;
		}

		/// <summary>
		/// 跳到最后
		/// </summary>
		public virtual void SkipToTheEnd()
		{
			StartCoroutine(SkipToTheEndCo());
		}

		/// <summary>
		/// 如果正在进行脚本驱动的暂停，则恢复序列的执行
		/// Resumes execution of a sequence if a script driven pause is in progress
		/// </summary>
		public override void ResumeFeedbacks()
		{
			Events.TriggerOnResume(this);
			InScriptDrivenPause = false;
		}

		#endregion
        
		#region MODIFICATION（修改列表）

		/// <summary>
		/// Adds the specified MMF_Feedback to the player
		/// </summary>
		/// <param name="newFeedback"></param>
		public virtual void AddFeedback(MMF_Feedback newFeedback)
		{
			InitializeFeedbackList();
			newFeedback.Owner = this;
			newFeedback.UniqueID = Guid.NewGuid().GetHashCode();
			FeedbacksList.Add(newFeedback);
			newFeedback.CacheRequiresSetup();
			newFeedback.InitializeCustomAttributes();
		}
        
		/// <summary>
		/// Adds a feedback of the specified type to the player
		/// </summary>
		/// <param name="feedbackType"></param>
		/// <returns></returns>
		public new MMF_Feedback AddFeedback(System.Type feedbackType)
		{
			InitializeFeedbackList();
			MMF_Feedback newFeedback = (MMF_Feedback)Activator.CreateInstance(feedbackType);
			newFeedback.Label = FeedbackPathAttribute.GetFeedbackDefaultName(feedbackType);
			newFeedback.Owner = this;
			newFeedback.UniqueID = Guid.NewGuid().GetHashCode();
			FeedbacksList.Add(newFeedback);
			newFeedback.InitializeCustomAttributes();
			newFeedback.CacheRequiresSetup();
			return newFeedback;
		}
        
		/// <summary>
		/// Removes the feedback at the specified index
		/// </summary>
		/// <param name="id"></param>
		public override void RemoveFeedback(int id)
		{
			if (FeedbacksList.Count < id)
			{
				return;
			}
			FeedbacksList.RemoveAt(id);
		}
        
		#endregion MODIFICATION

		#region HELPERS
        
		/// <summary>
		/// Returns true if feedbacks are still playing
		/// </summary>
		/// <returns></returns>
		public override bool HasFeedbackStillPlaying()
		{
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i].IsPlaying && !FeedbacksList[i].Timing.ExcludeFromHoldingPauses)
				{
					return true;
				}
			}
			return false;
		}
        
		/// <summary>
		/// Checks whether or not this MMFeedbacks contains one or more looper feedbacks
		/// </summary>
		protected override void CheckForLoops()
		{
			ContainsLoop = false;
			int count = FeedbacksList.Count;
			for (int i = 0; i < count; i++)
			{
				if (FeedbacksList[i] != null)
				{
					if (FeedbacksList[i].LooperPause && FeedbacksList[i].Active)
					{
						ContainsLoop = true;
						return;
					}
				}                
			}
		}
        
		/// <summary>
		/// This will return true if the conditions defined in the specified feedback's Timing section allow it to play in the current play direction of this MMFeedbacks
		/// </summary>
		/// <param name="feedback"></param>
		/// <returns></returns>
		protected bool FeedbackCanPlay(MMF_Feedback feedback)
		{
			if (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.Always)
			{
				return true;
			}
			else if (((Direction == Directions.TopToBottom) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards))
			         || ((Direction == Directions.BottomToTop) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards)))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Readies the MMFeedbacks to revert direction on the next play
		/// </summary>
		protected override void ApplyAutoRevert()
		{
			if (AutoChangeDirectionOnEnd)
			{
				ShouldRevertOnNextPlay = true;
			}
		}
        
		/// <summary>
		/// Applies this feedback's time multiplier to a duration (in seconds)
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public override float ApplyTimeMultiplier(float duration)
		{
			return duration * DurationMultiplier;
		}

		/// <summary>
		/// Lets you destroy objects from feedbacks
		/// </summary>
		/// <param name="gameObjectToDestroy"></param>
		public virtual void ProxyDestroy(GameObject gameObjectToDestroy)
		{
			Destroy(gameObjectToDestroy);
		}
        
		/// <summary>
		/// Lets you destroy objects after a delay from feedbacks
		/// </summary>
		/// <param name="gameObjectToDestroy"></param>
		/// <param name="delay"></param>
		public virtual void ProxyDestroy(GameObject gameObjectToDestroy, float delay)
		{
			Destroy(gameObjectToDestroy, delay);
		}

		/// <summary>
		/// Lets you DestroyImmediate objects from feedbacks
		/// </summary>
		/// <param name="gameObjectToDestroy"></param>
		public virtual void ProxyDestroyImmediate(GameObject gameObjectToDestroy)
		{
			DestroyImmediate(gameObjectToDestroy);
		}
        
		#endregion

		#region ACCESS

		/// <summary>
		/// Returns the first feedback of the searched type on this MMF_Player
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual T GetFeedbackOfType<T>() where T:MMF_Feedback
		{
			_t = typeof(T);
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if (feedback.GetType() == _t)
				{
					return (T)feedback;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns a list of all the feedbacks of the searched type on this MMF_Player
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual List<T> GetFeedbacksOfType<T>() where T:MMF_Feedback
		{
			_t = typeof(T);
			List<T> list = new List<T>();
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if (feedback.GetType() == _t)
				{
					list.Add((T)feedback);
				}
			}
			return list;
		}

		/// <summary>
		/// Returns the first feedback of the searched type on this MMF_Player
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual T GetFeedbackOfType<T>(string searchedLabel) where T:MMF_Feedback
		{
			_t = typeof(T);
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if ((feedback.GetType() == _t) && (feedback.Label == searchedLabel))
				{
					return (T)feedback;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns a list of all the feedbacks of the searched type on this MMF_Player
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual List<T> GetFeedbacksOfType<T>(string searchedLabel) where T:MMF_Feedback
		{
			_t = typeof(T);
			List<T> list = new List<T>();
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				if ((feedback.GetType() == _t) && (feedback.Label == searchedLabel))
				{
					list.Add((T)feedback);
				}
			}
			return list;
		}

		#endregion
        
		#region EVENTS

		/// <summary>
		/// On Disable we stop all feedbacks
		/// </summary>
		protected override void OnDisable()
		{
			if (IsPlaying)
			{
				if (ForceStopFeedbacksOnDisable)
				{
					StopFeedbacks();    
				}
				StopAllCoroutines();
				for (int i = FeedbacksList.Count - 1; i >= 0; i--)
				{
					FeedbacksList[i].OnDisable();
				}
			}
		}

		/// <summary>
		/// On validate, we make sure our DurationMultiplier remains positive
		/// </summary>
		protected override void OnValidate()
		{
			RefreshCache();
		}

		/// <summary>
		/// Refreshes cached feedbacks
		/// </summary>
		public virtual void RefreshCache()
		{
			if (FeedbacksList == null)
			{
				return;
			}
            
			DurationMultiplier = Mathf.Clamp(DurationMultiplier, 0f, Single.MaxValue);
            
			for (int i = FeedbacksList.Count - 1; i >= 0; i--)
			{
				if (FeedbacksList[i] == null)
				{
					FeedbacksList.RemoveAt(i);
				}
				else
				{
					FeedbacksList[i].Owner = this;
					FeedbacksList[i].CacheRequiresSetup();
					FeedbacksList[i].OnValidate();	
				}
			}
		}

		/// <summary>
		/// On Destroy, removes all feedbacks from this MMFeedbacks to avoid any leftovers
		/// </summary>
		protected override void OnDestroy()
		{
			IsPlaying = false;
            
			foreach (MMF_Feedback feedback in FeedbacksList)
			{
				feedback.OnDestroy();
			}
		}

		#endregion EVENTS
	}    
}