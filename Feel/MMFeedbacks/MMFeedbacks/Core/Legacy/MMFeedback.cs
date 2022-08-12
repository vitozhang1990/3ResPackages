using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 反馈基类（可以自定义扩展）
    /// A base class, meant to be extended, defining a Feedback. A Feedback is an action triggered by a MMFeedbacks, usually in reaction to the player's input or actions,
    /// to help communicate both emotion and legibility, improving game feel.
    /// To create a new feedback, extend this class and override its Custom methods, declared at the end of this class. You can look at the many examples for reference.
    /// </summary>
    [AddComponentMenu("")]
    [System.Serializable]
    [ExecuteAlways]
    public abstract class MMFeedback : MonoBehaviour
    {
        [Tooltip("这个反馈是否活跃")]
        public bool Active = true;
        [Tooltip("要在检查器中显示的此反馈的名称")]
        public string Label = "MMFeedback";
        [Tooltip("这种反馈发生的几率(百分比:100:总是发生，0:从未发生，50:每两个调用发生一次，等等)")]
        [Range(0, 100)]
        public float Chance = 100f;
        [Tooltip("许多与时间相关的值(延迟、重复等)")]
        public MMFeedbackTiming Timing;
        /// <summary>
        /// 反馈的所有者，在调用初始化方法时定义
        /// the Owner of the feedback, as defined when calling the Initialization method
        /// </summary>
        public GameObject Owner { get; set; }
        [HideInInspector]
        /// whether or not this feedback is in debug mode
        [Tooltip("这个反馈是否处于调试模式")]
        public bool DebugActive = false;
        /// <summary>
        /// 如果您的反馈应该暂停反馈序列的执行，请将此设置为true?
        /// set this to true if your feedback should pause the execution of the feedback sequence
        /// </summary>
        public virtual IEnumerator Pause { get { return null; } }
        /// <summary>
        /// 如果这是真的，这个反馈将等待直到所有之前的反馈运行
        /// if this is true, this feedback will wait until all previous feedbacks have run
        /// </summary>
        public virtual bool HoldingPause { get { return false; } }
        /// <summary>
        /// 如果这是真的，这个反馈将等待直到所有之前的反馈运行，然后再次运行所有之前的反馈(是否暂停循环播放)
        /// if this is true, this feedback will wait until all previous feedbacks have run, then run all previous feedbacks again
        /// </summary>
        public virtual bool LooperPause { get { return false; } }
        /// <summary>
        /// 如果这是真的，这个反馈将暂停并等待，直到它的父级反馈调用Resume()来恢复执行
        /// if this is true, this feedback will pause and wait until Resume() is called on its parent MMFeedbacks to resume execution
        /// </summary>
        public virtual bool ScriptDrivenPause { get; set; }
        /// <summary>
        /// 如果这是一个正值，反馈将在持续时间之后自动恢复，如果它还没有通过脚本恢复
        /// if this is a positive value, the feedback will auto resume after that duration if it hasn't been resumed via script already
        /// </summary>
        public virtual float ScriptDrivenPauseAutoResume { get; set; }
        /// <summary>
        /// 如果这是真的，这个反馈将等待直到所有之前的反馈运行，然后再次运行所有之前的反馈(是否开始循环播放)
        /// if this is true, this feedback will wait until all previous feedbacks have run, then run all previous feedbacks again
        /// </summary>
        public virtual bool LooperStart { get { return false; } }
        /// <summary>
        /// 反馈的可重写颜色，可以根据反馈重新定义。白色是唯一保留的颜色，当留给白色时，反馈将恢复正常(浅色或深色皮肤)
        /// an overridable color for your feedback, that can be redefined per feedback. White is the only reserved color, and the feedback will revert to 
        /// normal (light or dark skin) when left to White
        /// </summary>
#if UNITY_EDITOR
        public virtual Color FeedbackColor { get { return Color.white; } }
#endif
        /// <summary>
        /// 如果此反馈此时处于冷却期(因此不能播放)，则返回true，否则返回false
        /// returns true if this feedback is in cooldown at this time (and thus can't play), false otherwise
        /// </summary>
        public virtual bool InCooldown { get { return (Timing.CooldownDuration > 0f) && (FeedbackTime - _lastPlayTimestamp < Timing.CooldownDuration); } }
        /// <summary>
        /// 如果这是真的，那么这个反馈正在播放
        /// if this is true, this feedback is currently playing
        /// </summary>
        public virtual bool IsPlaying { get; set; }
        /// <summary>
        /// 根据所选的定时设置设置时间(或未缩放时间)
        /// the time (or unscaled time) based on the selected Timing settings
        /// </summary>
        public float FeedbackTime
        {
            get
            {
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
        /// 根据所选的定时设置来调整增量时间(或未缩放的增量时间)
        /// the delta time (or unscaled delta time) based on the selected Timing settings
        /// </summary>
        public float FeedbackDeltaTime
        {
            get
            {
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
        /// 反馈的总时长:
        /// 总时间=初始延迟+持续时间*(重复次数+重复之间的延迟)
        /// The total duration of this feedback :
        /// total = initial delay + duration * (number of repeats + delay between repeats)  
        /// </summary>
        public float TotalDuration
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

                if (Timing.NumberOfRepeats > 0)
                {
                    float delayBetweenRepeats = ApplyTimeMultiplier(Timing.DelayBetweenRepeats);

                    totalTime += (Timing.NumberOfRepeats * FeedbackDuration) + (Timing.NumberOfRepeats * delayBetweenRepeats);
                }

                return totalTime;
            }
        }
        /// <summary>
        /// 最后一次播放此反馈的时间戳
        /// the timestamp at which this feedback was last played
        /// </summary>
        public virtual float FeedbackStartedAt { get { return _lastPlayTimestamp; } }
        /// <summary>
        /// 感知到的反馈持续时间，用于显示进度条，意味着每个反馈都会覆盖有意义的数据
        /// the perceived duration of the feedback, to be used to display its progress bar, meant to be overridden with meaningful data by each feedback
        /// </summary>
        public virtual float FeedbackDuration { get { return 0f; } set { } }
        /// <summary>
        /// 不管这个反馈现在是否在发挥作用
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
        protected int _sequenceTrackID = 0;
        protected MMFeedbacks _hostMMFeedbacks;

        protected float _beatInterval;
        protected bool BeatThisFrame = false;
        protected int LastBeatIndex = 0;
        protected int CurrentSequenceIndex = 0;
        protected float LastBeatTimestamp = 0f;
        protected bool _isHostMMFeedbacksNotNull;

        protected virtual void OnEnable()
        {
            _hostMMFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();
            _isHostMMFeedbacksNotNull = _hostMMFeedbacks != null;
        }

        /// <summary>
        /// 初始化反馈及其与时间相关的变量
        /// Initializes the feedback and its timing related variables
        /// </summary>
        /// <param name="owner"></param>
        public virtual void Initialization(GameObject owner)
        {
            _initialized = true;
            Owner = owner;
            _playsLeft = Timing.NumberOfRepeats + 1;
            _hostMMFeedbacks = this.gameObject.GetComponent<MMFeedbacks>();

            SetInitialDelay(Timing.InitialDelay);
            SetDelayBetweenRepeats(Timing.DelayBetweenRepeats);
            SetSequence(Timing.Sequence);

            CustomInitialization(owner);
        }

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

            // we check the cooldown
            if (InCooldown)
            {
                return;
            }

            if (Timing.InitialDelay > 0f)
            {
                _playCoroutine = StartCoroutine(PlayCoroutine(position, feedbacksIntensity));
            }
            else
            {
                _lastPlayTimestamp = FeedbackTime;
                RegularPlay(position, feedbacksIntensity);
            }
        }

        /// <summary>
        /// 延迟反馈初始播放的内部协程
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
            _lastPlayTimestamp = FeedbackTime;
            RegularPlay(position, feedbacksIntensity);
        }

        /// <summary>
        /// 如果需要，触发延迟协程
        /// Triggers delaying coroutines if needed
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected virtual void RegularPlay(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Chance == 0f)
            {
                return;
            }
            if (Chance != 100f)
            {
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
                _infinitePlayCoroutine = StartCoroutine(InfinitePlay(position, feedbacksIntensity));
                return;
            }
            if (Timing.NumberOfRepeats > 0)
            {
                _repeatedPlayCoroutine = StartCoroutine(RepeatedPlay(position, feedbacksIntensity));
                return;
            }
            if (Timing.Sequence == null)
            {
                CustomPlayFeedback(position, feedbacksIntensity);
            }
            else
            {
                _sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));
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
                _lastPlayTimestamp = FeedbackTime;
                if (Timing.Sequence == null)
                {
                    CustomPlayFeedback(position, feedbacksIntensity);
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
                    _sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));

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
                _lastPlayTimestamp = FeedbackTime;
                _playsLeft--;
                if (Timing.Sequence == null)
                {
                    CustomPlayFeedback(position, feedbacksIntensity);

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
                    _sequenceCoroutine = StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));

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

        /// <summary>
        /// 用于在序列上播放此反馈的协同程序
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
        /// 停止播放所有反馈。将停止重复反馈，并调用自定义停止实现
        /// Stops all feedbacks from playing. Will stop repeating feedbacks, and call custom stop implementations
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        public virtual void Stop(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (_playCoroutine != null) { StopCoroutine(_playCoroutine); }
            if (_infinitePlayCoroutine != null) { StopCoroutine(_infinitePlayCoroutine); }
            if (_repeatedPlayCoroutine != null) { StopCoroutine(_repeatedPlayCoroutine); }
            if (_sequenceCoroutine != null) { StopCoroutine(_sequenceCoroutine); }

            _lastPlayTimestamp = 0f;
            _playsLeft = Timing.NumberOfRepeats + 1;
            if (Timing.InterruptsOnStop)
            {
                CustomStopFeedback(position, feedbacksIntensity);
            }
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
                        return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
                    case MMFeedbackTiming.PlayDirections.AlwaysNormal:
                        return true;
                    case MMFeedbackTiming.PlayDirections.AlwaysRewind:
                        return false;
                    case MMFeedbackTiming.PlayDirections.OppositeMMFeedbacksDirection:
                        return !(_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
                }
                return true;
            }
        }

        /// <summary>
        /// 根据其mmFeedbackDirectionCondition（基于主反馈的方向来确定当前反馈的播放方式）设置，如果此反馈应在当前父级反馈方向播放，则返回true
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
                        return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.TopToBottom);
                    case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards:
                        return (_hostMMFeedbacks.Direction == MMFeedbacks.Directions.BottomToTop);
                }
                return true;
            }
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
        /// 将主反馈的时间乘数应用于此反馈
        /// Applies the host MMFeedbacks' time multiplier to this feedback
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        protected virtual float ApplyTimeMultiplier(float duration)
        {
            if (_isHostMMFeedbacksNotNull)
            {
                return _hostMMFeedbacks.ApplyTimeMultiplier(duration);
            }

            return duration;
        }

        /// <summary>
        /// 该方法描述了反馈所需的所有自定义初始化过程，以及主初始化方法
        /// This method describes all custom initialization processes the feedback requires, in addition to the main Initialization method
        /// </summary>
        /// <param name="owner"></param>
        protected virtual void CustomInitialization(GameObject owner) { }

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
        /// 此方法描述了当反馈复位时发生的情况
        /// This method describes what happens when the feedback gets reset
        /// </summary>
        protected virtual void CustomReset() { }
    }
}