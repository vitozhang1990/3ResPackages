using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 时间刻度的模式
    /// </summary>
    public enum TimescaleModes
    {
        /// <summary>
        /// 按比例？
        /// </summary>
        Scaled,
        /// <summary>
        /// 不按比例？
        /// </summary>
        Unscaled
    }

    /// <summary>
    /// 一个收集延迟，冷却和重复值的类，用来定义每个反馈的行为
    /// A class collecting delay, cooldown and repeat values, to be used to define the behaviour of each MMFeedback
    /// </summary>
    [System.Serializable]
    public class MMFeedbackTiming
    {
        /// <summary>
        /// 基于主反馈的方向来确定当前反馈的播放方式
        /// </summary>
        public enum MMFeedbacksDirectionConditions
        {
            /// <summary>
            /// 这种反馈总是有效的
            /// </summary>
            Always,
            /// <summary>
            /// 当前反馈将只在主反馈在从上到下的方向播放时播放(向前)
            /// </summary>
            OnlyWhenForwards,
            /// <summary>
            /// 只有当主反馈在底部到顶部播放时，当前反馈才会播放 (向后)
            /// </summary>
            OnlyWhenBackwards
        };
        /// <summary>
        /// 基于主反馈的播放方式，来确定当前反馈的播放方式
        /// </summary>
        public enum PlayDirections
        {
            /// <summary>
            /// 当主反馈向前播放时当前反馈会正常播放，当主反馈向后播放时当前反馈会倒带播放
            /// </summary>
            FollowMMFeedbacksDirection,
            /// <summary>
            /// 当主反馈向前播放时，当前反馈将以倒带方式播放（通常在主反馈向后播放时播放当前反馈）
            /// </summary>
            OppositeMMFeedbacksDirection,
            /// <summary>
            /// 无论主反馈的方向如何，当前反馈都将始终正常播放
            /// </summary>
            AlwaysNormal,
            /// <summary>
            /// 无论主反馈的方向如何，当前反馈都将始终以倒带方式播放
            /// </summary>
            AlwaysRewind
        }

        [Header("时间刻度")]
        [Tooltip("时间刻度的模式")]
        public TimescaleModes TimescaleMode = TimescaleModes.Scaled;

        [Header("异常")]
        [Tooltip("如果这是真的，保持暂停将不会等待这个反馈完成")]
        public bool ExcludeFromHoldingPauses = false;
        /// whether to count this feedback in the parent MMFeedbacks(Player) total duration or not
        [Tooltip("是否将此反馈计入上级反馈的总持续时间中")]
        public bool ContributeToTotalDuration = true;

        [Header("延迟")]
        /// the initial delay to apply before playing the delay (in seconds)
        [Tooltip("在播放延迟之前应用的初始延迟(以秒为单位)")]
        public float InitialDelay = 0f;
        /// the cooldown duration mandatory between two plays
        [Tooltip("两个反馈之间的强制冷却时间？")]
        public float CooldownDuration = 0f;

        [Header("停止")]
        /// if this is true, this feedback will interrupt itself when Stop is called on its parent MMFeedbacks, otherwise it'll keep running
        [Tooltip("如果这是真的，当前反馈将中断自己当停止调用它的父级反馈，否则它将继续运行")]
        public bool InterruptsOnStop = true;

        [Header("重复")]
        /// the repeat mode, whether the feedback should be played once, multiple times, or forever
        [Tooltip("重复模式，反馈是应该播放一次，多次还是永远")]
        public int NumberOfRepeats = 0;
        /// if this is true, the feedback will be repeated forever
        [Tooltip("如果这是真的，反馈将永远重复")]
        public bool RepeatForever = false;
        /// the delay (in seconds) between two firings of this feedback. This doesn't include the duration of the feedback. 
        [Tooltip("两次触发此反馈之间的延迟(以秒为单位)。这还不包括反馈的持续时间。")]
        public float DelayBetweenRepeats = 1f;

        [Header("播放方向")]
        /// this defines how this feedback should play when the host MMFeedbacks is played :
        /// - always (default) : this feedback will always play
        /// - OnlyWhenForwards : this feedback will only play if the host MMFeedbacks is played in the top to bottom direction (forwards)
        /// - OnlyWhenBackwards : this feedback will only play if the host MMFeedbacks is played in the bottom to top direction (backwards)
        [Tooltip("这定义了当主反馈被播放时当前反馈应该如何播放:" +
                 "- always (default) : 这种反馈总是有效的" +
                 "- OnlyWhenForwards : 当前反馈将只在主反馈在从上到下的方向播放时播放(向前)。" +
                 "- OnlyWhenBackwards : 只有当主反馈在底部到顶部播放时，当前反馈才会播放 (向后)")]
        public MMFeedbacksDirectionConditions MMFeedbacksDirectionCondition = MMFeedbacksDirectionConditions.Always;
        /// this defines the way this feedback will play. It can play in its normal direction, or in rewind (a sound will play backwards, 
        /// an object normally scaling up will scale down, a curve will be evaluated from right to left, etc)
        /// - BasedOnMMFeedbacksDirection : will play normally when the host MMFeedbacks is played forwards, in rewind when it's played backwards
        /// - OppositeMMFeedbacksDirection : will play in rewind when the host MMFeedbacks is played forwards, and normally when played backwards
        /// - Always Normal : will always play normally, regardless of the direction of the host MMFeedbacks
        /// - Always Rewind : will always play in rewind, regardless of the direction of the host MMFeedbacks
        [Tooltip("定义了反馈的作用方式。它可以按正常方向播放，也可以倒带播放 (a sound will play backwards," +
                 "- BasedOnMMFeedbacksDirection : 当主反馈向前播放时当前反馈会正常播放，当主反馈向后播放时当前反馈会倒带播放" +
                 "- OppositeMMFeedbacksDirection : 当主反馈向前播放时，当前反馈将以倒带方式播放（通常在主反馈向后播放时播放当前反馈）" +
                 "- Always Normal : 无论主反馈的方向如何，当前反馈都将始终正常播放" +
                 "- Always Rewind : 无论主反馈的方向如何，当前反馈都将始终以倒带方式播放")]
        public PlayDirections PlayDirection = PlayDirections.FollowMMFeedbacksDirection;

        [Header("强度")]
        /// if this is true, intensity will be constant, even if the parent MMFeedbacks is played at a lower intensity
        [Tooltip("如果这是真的，强度将是恒定的，即使父级反馈以较低的强度播放")]
        public bool ConstantIntensity = false;
        /// if this is true, this feedback will only play if its intensity is higher or equal to IntensityIntervalMin and lower than IntensityIntervalMax
        [Tooltip("如果这是真的，这个反馈将只在强度高于或等于IntensityIntervalMin（最小强度）和低于IntensityIntervalMax（最大强度）时播放")]
        public bool UseIntensityInterval = false;
        /// the minimum intensity required for this feedback to play
        [Tooltip("此反馈播放所需的最小强度")]
        [MMFCondition("UseIntensityInterval", true)]
        public float IntensityIntervalMin = 0f;
        /// the maximum intensity required for this feedback to play
        [Tooltip("此反馈播放所需的最大强度")]
        [MMFCondition("UseIntensityInterval", true)]
        public float IntensityIntervalMax = 0f;

        [Header("序列")]
        /// A MMSequence to use to play these feedbacks on
        [Tooltip("用于播放这些反馈的M序列")]
        public MMSequence Sequence;
        /// The MMSequence's TrackID to consider
        [Tooltip("反馈序列的轨迹ID")]
        public int TrackID = 0;
        /// whether or not to use the quantized version of the target sequence
        [Tooltip("是否使用量化版本的目标序列")]
        public bool Quantized = false;
        /// if using the quantized version of the target sequence, the BPM to apply to the sequence when playing it
        [Tooltip("如果使用目标序列的量化版本，则BPM应用于播放时的序列")]
        [MMFCondition("Quantized", true)]
        public int TargetBPM = 120;
    }
}