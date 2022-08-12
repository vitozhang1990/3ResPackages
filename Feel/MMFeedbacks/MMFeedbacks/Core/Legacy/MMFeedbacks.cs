using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using System.Linq;
using UnityEditor.Experimental;
using UnityEngine.Events;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 反馈组，自定义一组反馈来一起播放
    /// A collection of MMFeedback, meant to be played altogether.
    /// This class provides a custom inspector to add and customize feedbacks, and public methods to trigger them, stop them, etc.
    /// You can either use it on its own, or bind it from another class and trigger it from there.
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/MMFeedbacks")]
    public class MMFeedbacks : MonoBehaviour
    {
        /// <summary>
        /// 反馈的播放方向
        /// </summary>
        public enum Directions
        {
            /// <summary>
            /// 从上到下
            /// </summary>
            TopToBottom,
            /// <summary>
            /// 从下到上
            /// </summary>
            BottomToTop
        }
        /// <summary>
        /// 安全模式（将执行检查以确保没有序列化错误损坏它们）
        /// </summary>
        public enum SafeModes
        {
            /// <summary>
            /// 不安全的
            /// </summary>
            Nope,
            /// <summary>
            /// 在启用时执行检查(on enable)
            /// </summary>
            EditorOnly,
            /// <summary>
            /// 在唤醒时执行检查(on Awake)
            /// </summary>
            RuntimeOnly,
            /// <summary>
            /// 执行编辑器和运行时检查(建议设置)
            /// </summary>
            Full
        }

        /// <summary>
        /// 要触发的反馈列表
        /// </summary>
        public List<MMFeedback> Feedbacks = new List<MMFeedback>();

        /// <summary>
        /// 初始化的方式
        /// 如果使用脚本，则必须通过调用初始化方法并将其传递给所有者来手动初始化
        /// 否则，您可以让该组件在唤醒或启动时初始化自身，在这种情况下，所有者将是MMFeedbacks本身
        /// the possible initialization modes. If you use Script, you'll have to initialize manually by calling the Initialization method and passing it an owner
        /// Otherwise, you can have this component initialize itself at Awake or Start, and in this case the owner will be the MMFeedbacks itself
        /// </summary>
        public enum InitializationModes
        {
            /// <summary>
            /// 手动调用初始化
            /// </summary>
            Script,
            Awake,
            Start
        }
        /// the chosen initialization mode
        [Tooltip("选择的初始化模式。如果使用Script，则必须通过调用初始化方法并向其传递所有者。否则，你可以让这个组件在Awake或Start中初始化自己，在这种情况下，所有者将是mmfeedback本身")]
        public InitializationModes InitializationMode = InitializationModes.Start;
        /// the selected safe mode
        [Tooltip("选择的安全模式")]
        public SafeModes SafeMode = SafeModes.Full;
        /// the selected direction
        [Tooltip("反馈的播放方向设置")]
        public Directions Direction = Directions.TopToBottom;
        /// whether or not this MMFeedbacks should invert its direction when all feedbacks have played
        [Tooltip("当所有的反馈播放完毕后，MMFeedbacks是否应该反转它的方向")]
        public bool AutoChangeDirectionOnEnd = false;
        /// whether or not to play this feedbacks automatically on Start
        [Tooltip("是否在开始时自动播放这个反馈（OnStart）")]
        public bool AutoPlayOnStart = false;
        /// whether or not to play this feedbacks automatically on Enable
        [Tooltip("是否在启用时自动播放此反馈（on Enable）")]
        public bool AutoPlayOnEnable = false;

        /// if this is true, all feedbacks within that player will work on the specified ForcedTimescaleMode, regardless of their individual settings 
        [Tooltip("如果这是真的，则该播放器内的所有反馈将在指定的ForcedTimescaleMode(时间刻度模式，下边那个参数)上工作，而不管其单独设置如何")]
        public bool ForceTimescaleMode = false;
        /// the time scale mode all feedbacks on this player should work on, if ForceTimescaleMode is true
        [Tooltip("如果ForceTimescaleMode（上边那个参数）为真，这个播放器上的所有反馈都应该工作在时间刻度模式上")]
        [MMFCondition("ForceTimescaleMode", true)]
        public TimescaleModes ForcedTimescaleMode = TimescaleModes.Unscaled;
        /// a time multiplier that will be applied to all feedback durations (initial delay, duration, delay between repeats...)
        [Tooltip("一个时间乘数，将应用于所有反馈持续时间(初始延迟，持续时间，重复之间的延迟…)")]
        public float DurationMultiplier = 1f;
        /// if this is true, more editor-only, detailed info will be displayed per feedback in the duration slot
        [Tooltip("如果是true，则在持续时间槽中的每个反馈都将显示更多的仅限编辑器的详细信息")]
        public bool DisplayFullDurationDetails = false;
        /// the timescale at which the player itself will operate. This notably impacts sequencing and pauses duration evaluation.
        [Tooltip("玩家自己操作的时间尺度。这明显影响了排序和暂停持续时间的评估。")]
        public TimescaleModes PlayerTimescaleMode = TimescaleModes.Unscaled;

        /// a duration, in seconds, during which triggering a new play of this MMFeedbacks after it's been played once will be impossible
        [Tooltip("播放冷却时间")]
        public float CooldownDuration = 0f;
        /// a duration, in seconds, to delay the start of this MMFeedbacks' contents play
        [Tooltip("一个持续时间，以秒为单位，延迟MMFeedbacks内容播放的开始")]
        public float InitialDelay = 0f;
        /// whether this player can be played or not, useful to temporarily prevent play from another class, for example
        [Tooltip("此播放器是否可以播放？")]
        public bool CanPlay = true;
        /// if this is true, you'll be able to trigger a new Play while this feedback is already playing, otherwise you won't be able to
        [Tooltip("如果这是真的，您将能够在该反馈已经播放时触发新播放，否则您将无法触发")]
        public bool CanPlayWhileAlreadyPlaying = true;
        /// the chance of this sequence happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)
        [Tooltip("这一序列发生的概率（百分比：100:一直发生，0:从未发生，50:每两次调用发生一次，以此类推）")]
        [Range(0, 100)]
        public float ChanceToPlay = 100f;

        /// the intensity at which to play this feedback. That value will be used by most feedbacks to tune their amplitude. 1 is normal, 0.5 is half power, 0 is no effect.
        /// Note that what this value controls depends from feedback to feedback, don't hesitate to check the code to see what it does exactly.  
        [Tooltip("播放此反馈的强度。大多数反馈将使用该值来调整其振幅。1为正常值，0.5为半幂，0为无效。请注意，此值控制的内容取决于不同的反馈，请检查代码以了解其确切功能。")]
        public float FeedbacksIntensity = 1f;

        /// a number of UnityEvents that can be triggered at the various stages of this MMFeedbacks 
        [Tooltip("可在该MMFeedbacks的各个阶段触发的多个UnityEvents")]
        public MMFeedbacksEvents Events;

        /// a global switch used to turn all feedbacks on or off globally
        [Tooltip("用于全局打开或关闭所有反馈的全局开关")]
        public static bool GlobalMMFeedbacksActive = true;


        [HideInInspector]
        /// <summary>
        /// MMFeedbacks是否处于调试模式
        /// whether or not this MMFeedbacks is in debug mode
        /// </summary>
        public bool DebugActive = false;
        /// <summary>
        /// 这个MMFeedbacks是否正在播放-这意味着它还没有停止。如果你不停止你的MMFeedbacks，它当然会继续存在
        /// whether or not this MMFeedbacks is playing right now - meaning it hasn't been stopped yet.
        /// if you don't stop your MMFeedbacks it'll remain true of course
        /// </summary>
        public bool IsPlaying { get; protected set; }
        /// <summary>
        /// 如果这个MMFeedbacks是在它开始播放的时间
        /// if this MMFeedbacks is playing the time since it started playing
        /// </summary>
        public float ElapsedTime => IsPlaying ? GetTime() - _lastStartAt : 0f;
        /// <summary>
        /// MMFeedbacks被播放的次数
        /// the amount of times this MMFeedbacks has been played
        /// </summary>
        public int TimesPlayed { get; protected set; }
        /// <summary>
        /// 是否阻止执行此MMFeedbacks序列并等待Resume（）调用
        /// whether or not the execution of this MMFeedbacks' sequence is being prevented and waiting for a Resume() call
        /// </summary>
        public bool InScriptDrivenPause { get; set; }
        /// <summary>
        /// 如果MMFeedbacks包含至少一个循环，则为真
        /// true if this MMFeedbacks contains at least one loop
        /// </summary>
        public bool ContainsLoop { get; set; }
        /// <summary>
        /// 如果这个反馈应该在下一次播放时改变播放方向，则为True
        /// true if this feedback should change play direction next time it's played
        /// </summary>
        public bool ShouldRevertOnNextPlay { get; set; }
        /// <summary>
        /// MMFeedbacks中所有主动反馈的总持续时间(以秒为单位)
        /// </summary>
        /// The total duration (in seconds) of all the active feedbacks in this MMFeedbacks
        public virtual float TotalDuration
        {
            get
            {
                float total = 0f;
                foreach (MMFeedback feedback in Feedbacks)
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

        public virtual float GetTime() { return (PlayerTimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
        public virtual float GetDeltaTime() { return (PlayerTimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

        protected float _startTime = 0f;
        protected float _holdingMax = 0f;
        protected float _lastStartAt = -float.MaxValue;
        protected bool _pauseFound = false;
        protected float _totalDuration = 0f;
        protected bool _shouldStop = false;

        #region INITIALIZATION（初始化）

        /// <summary>
        /// 在唤醒(Awake)时，如果处于自动模式，则初始化反馈
        /// On Awake we initialize our feedbacks if we're in auto mode
        /// </summary>
        protected virtual void Awake()
        {
            //如果我们的MMFeedbacks处于自动布局模式，我们会向它添加一个小助手，如果父游戏对象被关闭并再次打开，它将在需要时重新启用
            // if our MMFeedbacks is in AutoPlayOnEnable mode, we add a little helper to it that will re-enable it if needed if the parent game object gets turned off and on again
            if (AutoPlayOnEnable)
            {
                MMFeedbacksEnabler enabler = GetComponent<MMFeedbacksEnabler>();
                if (enabler == null)
                {
                    enabler = this.gameObject.AddComponent<MMFeedbacksEnabler>();
                }
                enabler.TargetMMFeedbacks = this;
            }

            if ((InitializationMode == InitializationModes.Awake) && (Application.isPlaying))
            {
                Initialization(this.gameObject);
            }
            CheckForLoops();
        }

        /// <summary>
        /// 在开始(Start)时，如果我们处于自动模式，我们将初始化反馈
        /// On Start we initialize our feedbacks if we're in auto mode
        /// </summary>
        protected virtual void Start()
        {
            if ((InitializationMode == InitializationModes.Start) && (Application.isPlaying))
            {
                Initialization(this.gameObject);
            }
            if (AutoPlayOnStart && Application.isPlaying)
            {
                PlayFeedbacks();
            }
            CheckForLoops();
        }

        /// <summary>
        /// 启用(OnEnable)时，如果处于自动模式，则初始化反馈
        /// On Enable we initialize our feedbacks if we're in auto mode
        /// </summary>
        protected virtual void OnEnable()
        {
            if (AutoPlayOnEnable && Application.isPlaying)
            {
                PlayFeedbacks();
            }
        }

        /// <summary>
        /// 初始化MMFeedbacks，将此MMFeedbacks设置为所有者
        /// Initializes the MMFeedbacks, setting this MMFeedbacks as the owner
        /// </summary>
        public virtual void Initialization()
        {
            Initialization(this.gameObject);
        }

        /// <summary>
        /// 用于初始化反馈的公共方法，指定所有者，该所有者将用作反馈位置和层次结构的参考
        /// A public method to initialize the feedback, specifying an owner that will be used as the reference for position and hierarchy by feedbacks
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="feedbacksOwner"></param>
        public virtual void Initialization(GameObject owner)
        {
            if ((SafeMode == MMFeedbacks.SafeModes.RuntimeOnly) || (SafeMode == MMFeedbacks.SafeModes.Full))
            {
                AutoRepair();
            }

            IsPlaying = false;
            TimesPlayed = 0;
            _lastStartAt = -float.MaxValue;

            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if (Feedbacks[i] != null)
                {
                    Feedbacks[i].Initialization(owner);
                }
            }
        }

        #endregion

        #region PLAY（播放）

        /// <summary>
        /// 使用MMFeedbacks的位置作为参考播放所有反馈，无衰减
        /// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation
        /// </summary>
        public virtual void PlayFeedbacks()
        {
            PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity);
        }

        /// <summary>
        /// 播放所有反馈并等待完成
        /// Plays all feedbacks and awaits until completion
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <param name="forceRevert"></param>
        public virtual async System.Threading.Tasks.Task PlayFeedbacksTask(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            PlayFeedbacks(position, feedbacksIntensity, forceRevert);
            while (IsPlaying)
            {
                await System.Threading.Tasks.Task.Yield();
            }
        }

        /// <summary>
        /// 播放所有反馈，指定位置和强度。该位置可由每个反馈使用，并被考虑用于例如激发粒子或播放声音。反馈强度是每个反馈可以用来降低其强度的一个因子，通常，您需要根据时间或距离定义衰减（使用较低的强度值表示距离玩家较远的反馈）。此外，您可以强制反馈反向播放，忽略其当前条件
        /// Plays all feedbacks, specifying a position and intensity. The position may be used by each Feedback and taken into account to spark a particle or play a sound for example.
        /// The feedbacks intensity is a factor that can be used by each Feedback to lower its intensity, usually you'll want to define that attenuation based on time or distance (using a lower 
        /// intensity value for feedbacks happening further away from the Player).
        /// Additionally you can force the feedback to play in reverse, ignoring its current condition
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksOwner"></param>
        /// <param name="feedbacksIntensity"></param>
        public virtual void PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
        }

        /// <summary>
        /// 使用MMFeedbacks的位置作为参考播放所有反馈，不衰减，反向播放（从下到上）
        /// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
        /// </summary>
        public virtual void PlayFeedbacksInReverse()
        {
            PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity, true);
        }

        /// <summary>
        /// 使用MMFeedbacks的位置作为参考播放所有反馈，不衰减，反向播放（从下到上）(自定义参数)
        /// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
        /// </summary>
        public virtual void PlayFeedbacksInReverse(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
        }

        /// <summary>
        /// 播放序列中的所有反馈，但仅当该MMFeedbacks按“相反顺序”播放时
        /// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in reverse order
        /// </summary>
        public virtual void PlayFeedbacksOnlyIfReversed()
        {

            if ((Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
                 || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay))
            {
                PlayFeedbacks();
            }
        }

        /// <summary>
        /// 播放序列中的所有反馈，但仅当该MMFeedbacks按“相反顺序”播放时（自定义参数）
        /// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in reverse order
        /// </summary>
        public virtual void PlayFeedbacksOnlyIfReversed(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {

            if ((Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
                 || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay))
            {
                PlayFeedbacks(position, feedbacksIntensity, forceRevert);
            }
        }

        /// <summary>
        /// 播放序列中的所有反馈，但仅当该MMFeedbacks按“正常顺序”播放时
        /// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in normal order
        /// </summary>
        public virtual void PlayFeedbacksOnlyIfNormalDirection()
        {
            if (Direction == Directions.TopToBottom)
            {
                PlayFeedbacks();
            }
        }

        /// <summary>
        /// 播放序列中的所有反馈，但仅当该MMFeedbacks按“正常顺序”播放时（自定义参数）
        /// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in normal order
        /// </summary>
        public virtual void PlayFeedbacksOnlyIfNormalDirection(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            if (Direction == Directions.TopToBottom)
            {
                PlayFeedbacks(position, feedbacksIntensity, forceRevert);
            }
        }

        /// <summary>
        /// 一个公共的协程，当你想要在你的协程中产生直到mmfeedback停止播放时，你可以从外部调用 
        /// 通常：返回myFeedback.PlayFeedbackCorroutine（this.transform.position，1.0f，false）；
        /// A public coroutine you can call externally when you want to yield in a coroutine of yours until the MMFeedbacks has stopped playing
        /// typically : yield return myFeedback.PlayFeedbacksCoroutine(this.transform.position, 1.0f, false);
        /// </summary>
        /// <param name="position">The position at which the MMFeedbacks should play（MMFeedbacks应该播放的位置 ）</param>
        /// <param name="feedbacksIntensity">The intensity of the feedback（反馈的强度）</param>
        /// <param name="forceRevert">Whether or not the MMFeedbacks should play in reverse or not（MMFeedbacks是否应该反向播放 ）</param>
        /// <returns></returns>
        public virtual IEnumerator PlayFeedbacksCoroutine(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
        {
            PlayFeedbacks(position, feedbacksIntensity, forceRevert);
            while (IsPlaying)
            {
                yield return null;
            }
        }

        #endregion

        #region SEQUENCE（序列）

        /// <summary>
        /// 用于播放反馈的内部方法不应在外部调用
        /// An internal method used to play feedbacks, shouldn't be called externally
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected virtual void PlayFeedbacksInternal(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
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

            // 如果我们有冷却时间，我们会在需要时阻止执行
            // if we have a cooldown we prevent execution if needed
            if (CooldownDuration > 0f)
            {
                if (GetTime() - _lastStartAt < CooldownDuration)
                {
                    return;
                }
            }

            // 如果所有反馈都被全局禁用，我们将停止播放
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
            TimesPlayed++;
            IsPlaying = true;
            _startTime = GetTime();
            _lastStartAt = _startTime;
            _totalDuration = TotalDuration;

            if (InitialDelay > 0f)
            {
                StartCoroutine(HandleInitialDelayCo(position, feedbacksIntensity, forceRevert));
            }
            else
            {
                PreparePlay(position, feedbacksIntensity, forceRevert);
            }
        }

        protected virtual void PreparePlay(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
        {
            Events.TriggerOnPlay(this);

            _holdingMax = 0f;

            // 测试是否发现暂停或保持暂停
            // test if a pause or holding pause is found
            _pauseFound = false;
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if (Feedbacks[i] != null)
                {
                    if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
                    {
                        _pauseFound = true;
                    }
                    if ((Feedbacks[i].HoldingPause == true) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
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

        protected virtual void PlayAllFeedbacks(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
        {
            // 如果没有发现暂停，我们就一次播放所有反馈
            // if no pause was found, we just play all feedbacks at once
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if (FeedbackCanPlay(Feedbacks[i]))
                {
                    Feedbacks[i].Play(position, feedbacksIntensity);
                }
            }
        }

        protected virtual IEnumerator HandleInitialDelayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
        {
            IsPlaying = true;
            yield return MMFeedbacksCoroutine.WaitFor(InitialDelay);
            PreparePlay(position, feedbacksIntensity, forceRevert);
        }

        protected virtual void Update()
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
        /// 如果反馈仍在播放，则返回true
        /// Returns true if feedbacks are still playing
        /// </summary>
        /// <returns></returns>
        public virtual bool HasFeedbackStillPlaying()
        {
            int count = Feedbacks.Count;
            for (int i = 0; i < count; i++)
            {
                if (Feedbacks[i].IsPlaying)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 用于在涉及暂停时处理反馈序列
        /// A coroutine used to handle the sequence of feedbacks if pauses are involved
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        /// <returns></returns>
        protected virtual IEnumerator PausedFeedbacksCo(Vector3 position, float feedbacksIntensity)
        {
            IsPlaying = true;

            int i = (Direction == Directions.TopToBottom) ? 0 : Feedbacks.Count - 1;

            while ((i >= 0) && (i < Feedbacks.Count))
            {
                if (!IsPlaying)
                {
                    yield break;
                }

                if (Feedbacks[i] == null)
                {
                    yield break;
                }

                if (((Feedbacks[i].Active) && (Feedbacks[i].ScriptDrivenPause)) || InScriptDrivenPause)
                {
                    InScriptDrivenPause = true;

                    bool inAutoResume = (Feedbacks[i].ScriptDrivenPauseAutoResume > 0f);
                    float scriptDrivenPauseStartedAt = GetTime();
                    float autoResumeDuration = Feedbacks[i].ScriptDrivenPauseAutoResume;

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
                if ((Feedbacks[i].Active)
                    && ((Feedbacks[i].HoldingPause == true) || (Feedbacks[i].LooperPause == true))
                    && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
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

                // 播放反馈
                // plays the feedback
                if (FeedbackCanPlay(Feedbacks[i]))
                {
                    Feedbacks[i].Play(position, feedbacksIntensity);
                }

                // 处理暂停
                // Handles pause
                if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
                {
                    bool shouldPause = true;
                    if (Feedbacks[i].Chance < 100)
                    {
                        float random = Random.Range(0f, 100f);
                        if (random > Feedbacks[i].Chance)
                        {
                            shouldPause = false;
                        }
                    }

                    if (shouldPause)
                    {
                        yield return Feedbacks[i].Pause;
                        Events.TriggerOnResume(this);
                        _lastStartAt = GetTime();
                        _holdingMax = 0f;
                    }
                }

                // 更新保持最大值
                // updates holding max
                if (Feedbacks[i].Active)
                {
                    if ((Feedbacks[i].Pause == null) && (Feedbacks[i].ShouldPlayInThisSequenceDirection) && (!Feedbacks[i].Timing.ExcludeFromHoldingPauses))
                    {
                        float feedbackDuration = Feedbacks[i].TotalDuration;
                        _holdingMax = Mathf.Max(feedbackDuration, _holdingMax);
                    }
                }

                // 处理循环播放？
                // handles looper
                if ((Feedbacks[i].LooperPause == true)
                    && (Feedbacks[i].Active)
                    && (Feedbacks[i].ShouldPlayInThisSequenceDirection)
                    && (((Feedbacks[i] as MMFeedbackLooper).NumberOfLoopsLeft > 0) || (Feedbacks[i] as MMFeedbackLooper).InInfiniteLoop))
                {
                    // 我们确定了应该重新开始的指数
                    // we determine the index we should start again at
                    bool loopAtLastPause = (Feedbacks[i] as MMFeedbackLooper).LoopAtLastPause;
                    bool loopAtLastLoopStart = (Feedbacks[i] as MMFeedbackLooper).LoopAtLastLoopStart;

                    int newi = 0;

                    int j = (Direction == Directions.TopToBottom) ? i - 1 : i + 1;

                    while ((j >= 0) && (j <= Feedbacks.Count))
                    {
                        // 如果我们在一开始
                        // if we're at the start
                        if (j == 0)
                        {
                            newi = j - 1;
                            break;
                        }
                        if (j == Feedbacks.Count)
                        {
                            newi = j;
                            break;
                        }
                        // 如果我们找到了暂停
                        // if we've found a pause
                        if ((Feedbacks[j].Pause != null)
                            && (Feedbacks[j].FeedbackDuration > 0f)
                            && loopAtLastPause && (Feedbacks[j].Active))
                        {
                            newi = j;
                            break;
                        }
                        // 如果开始循环就执行
                        // if we've found a looper start
                        if ((Feedbacks[j].LooperStart == true)
                            && loopAtLastLoopStart
                            && (Feedbacks[j].Active))
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

        #endregion

        #region STOP（停止）

        /// <summary>
        /// 停止播放所有其他反馈，而不停止单个反馈
        /// Stops all further feedbacks from playing, without stopping individual feedbacks 
        /// </summary>
        public virtual void StopFeedbacks()
        {
            StopFeedbacks(true);
        }

        /// <summary>
        /// 停止播放所有反馈，还可以选择停止个别反馈
        /// Stops all feedbacks from playing, with an option to also stop individual feedbacks
        /// </summary>
        public virtual void StopFeedbacks(bool stopAllFeedbacks = true)
        {
            StopFeedbacks(this.transform.position, 1.0f, stopAllFeedbacks);
        }

        /// <summary>
        /// 停止播放所有反馈，指定反馈可以使用的位置和强度
        /// Stops all feedbacks from playing, specifying a position and intensity that can be used by the Feedbacks 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        public virtual void StopFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool stopAllFeedbacks = true)
        {
            if (stopAllFeedbacks)
            {
                for (int i = 0; i < Feedbacks.Count; i++)
                {
                    Feedbacks[i].Stop(position, feedbacksIntensity);
                }
            }
            IsPlaying = false;
            StopAllCoroutines();
        }

        #endregion

        #region CONTROLS（控制面板）

        /// <summary>
        /// 调用每个反馈的重置方法，如果它们已经定义了一个。其中一个例子是重置闪烁渲染器的初始颜色。
        /// Calls each feedback's Reset method if they've defined one. An example of that can be resetting the initial color of a flickering renderer.
        /// </summary>
        public virtual void ResetFeedbacks()
        {
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if ((Feedbacks[i] != null) && (Feedbacks[i].Active))
                {
                    Feedbacks[i].ResetFeedback();
                }
            }
            IsPlaying = false;
        }

        /// <summary>
        /// 更改此MMFeedbacks的方向
        /// Changes the direction of this MMFeedbacks
        /// </summary>
        public virtual void Revert()
        {
            Events.TriggerOnRevert(this);
            Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
        }

        /// <summary>
        /// 使用此方法可授权或阻止此播放器播放
        /// Use this method to authorize or prevent this player from being played
        /// </summary>
        /// <param name="newState">是否可以播放</param>
        public virtual void SetCanPlay(bool newState)
        {
            CanPlay = newState;
        }

        /// <summary>
        /// 暂停序列的执行，然后可以通过调用ResumeFeedbacks（）继续执行序列
        /// Pauses execution of a sequence, which can then be resumed by calling ResumeFeedbacks()
        /// </summary>
        public virtual void PauseFeedbacks()
        {
            Events.TriggerOnPause(this);
            InScriptDrivenPause = true;
        }

        /// <summary>
        /// 如果正在进行脚本驱动的暂停，则恢复序列的执行
        /// Resumes execution of a sequence if a script driven pause is in progress
        /// </summary>
        public virtual void ResumeFeedbacks()
        {
            Events.TriggerOnResume(this);
            InScriptDrivenPause = false;
        }

        #endregion

        #region MODIFICATION（添加或移除反馈）
        /// <summary>
        /// 添加反馈
        /// </summary>
        /// <param name="feedbackType"></param>
        /// <returns></returns>
        public virtual MMFeedback AddFeedback(System.Type feedbackType)
        {
            MMFeedback newFeedback;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                newFeedback = Undo.AddComponent(this.gameObject, feedbackType) as MMFeedback;
            }
            else
            {
                newFeedback = this.gameObject.AddComponent(feedbackType) as MMFeedback;
            }
#else
                newFeedback = this.gameObject.AddComponent(feedbackType) as MMFeedback;
#endif

            newFeedback.hideFlags = HideFlags.HideInInspector;
            newFeedback.Label = FeedbackPathAttribute.GetFeedbackDefaultName(feedbackType);

            AutoRepair();

            return newFeedback;
        }
        /// <summary>
        /// 通过id移除反馈
        /// </summary>
        /// <param name="id"></param>
        public virtual void RemoveFeedback(int id)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.DestroyObjectImmediate(Feedbacks[id]);
            }
            else
            {
                DestroyImmediate(Feedbacks[id]);
            }
#else
                DestroyImmediate(Feedbacks[id]);
#endif

            Feedbacks.RemoveAt(id);
            AutoRepair();
        }

        #endregion MODIFICATION

        #region HELPERS（辅助功能）

        /// <summary>
        /// 检查反馈是否可以播放
        /// Evaluates the chance of this feedback to play, and returns true if this feedback can play, false otherwise
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateChance()
        {
            if (ChanceToPlay == 0f)
            {
                return false;
            }
            if (ChanceToPlay != 100f)
            {
                // determine the odds
                float random = Random.Range(0f, 100f);
                if (random > ChanceToPlay)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查此反馈是否包含一个或多个循环反馈
        /// Checks whether or not this MMFeedbacks contains one or more looper feedbacks
        /// </summary>
        protected virtual void CheckForLoops()
        {
            ContainsLoop = false;
            for (int i = 0; i < Feedbacks.Count; i++)
            {
                if (Feedbacks[i] != null)
                {
                    if (Feedbacks[i].LooperPause && Feedbacks[i].Active)
                    {
                        ContainsLoop = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 如果指定反馈的定时部分中定义的条件允许其在该MMFeedbacks的当前播放方向播放，则返回true
        /// This will return true if the conditions defined in the specified feedback's Timing section allow it to play in the current play direction of this MMFeedbacks
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        protected bool FeedbackCanPlay(MMFeedback feedback)
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
        /// 准备好MMFeedbacks，以便在下一次播放时恢复方向
        /// Readies the MMFeedbacks to revert direction on the next play
        /// </summary>
        protected virtual void ApplyAutoRevert()
        {
            if (AutoChangeDirectionOnEnd)
            {
                ShouldRevertOnNextPlay = true;
            }
        }

        /// <summary>
        /// 将此反馈的时间乘数应用于持续时间（以秒为单位）
        /// Applies this feedback's time multiplier to a duration (in seconds)
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public virtual float ApplyTimeMultiplier(float duration)
        {
            return duration * DurationMultiplier;
        }

        /// <summary>
        /// Unity有时会出现序列化问题。此方法通过修复可能发生的任何错误同步来修复此问题。
        /// Unity sometimes has serialization issues. 
        /// This method fixes that by fixing any bad sync that could happen.
        /// </summary>
        public virtual void AutoRepair()
        {
            List<Component> components = components = new List<Component>();
            components = this.gameObject.GetComponents<Component>().ToList();
            foreach (Component component in components)
            {
                if (component is MMFeedback)
                {
                    bool found = false;
                    for (int i = 0; i < Feedbacks.Count; i++)
                    {
                        if (Feedbacks[i] == (MMFeedback)component)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Feedbacks.Add((MMFeedback)component);
                    }
                }
            }
        }

        #endregion

        #region EVENTS（Unity MonoBehaviour 事件）

        /// <summary>
        /// On Disable we stop all feedbacks
        /// </summary>
        protected virtual void OnDisable()
        {
            /*if (IsPlaying)
			{
			    StopFeedbacks();
			    StopAllCoroutines();
			}*/
        }

        /// <summary>
        /// On validate, we make sure our DurationMultiplier remains positive
        /// </summary>
        protected virtual void OnValidate()
        {
            //确保我们的持续乘数保持为正
            DurationMultiplier = Mathf.Clamp(DurationMultiplier, 0f, Single.MaxValue);
        }

        /// <summary>
        /// On Destroy, removes all feedbacks from this MMFeedbacks to avoid any leftovers
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 删除此MMFeedbacks中的所有反馈，以避免任何剩余
            IsPlaying = false;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // we remove all binders
                foreach (MMFeedback feedback in Feedbacks)
                {
                    EditorApplication.delayCall += () =>
                    {
                        DestroyImmediate(feedback);
                    };
                }
            }
#endif
        }

        #endregion EVENTS
    }
}