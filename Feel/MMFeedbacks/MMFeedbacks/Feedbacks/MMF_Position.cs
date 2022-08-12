using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 该反馈将在指定的持续时间内，从选定的初始位置到选定的目的地，随时间对目标对象的位置设置动画。这些可以是相对于反馈位置的相对矢量3偏移，也可以是变换。如果指定变换，将忽略Vector3值。
	/// This feedback will animate the target object's position over time, for the specified duration, from the chosen initial position to the chosen destination. These can either be relative Vector3 offsets from the Feedback's position, or Transforms. If you specify transforms, the Vector3 values will be ignored.
	/// </summary>
	[AddComponentMenu("该反馈将在指定的持续时间内，从选定的初始位置到选定的目的地，随时间对目标对象的位置设置动画。这些可以是相对于反馈位置的相对矢量3偏移，也可以是变换。如果指定变换，将忽略Vector3值。")]
	[FeedbackHelp("")]
	[FeedbackPath("Transform/Position（位置移动）")]
	public class MMF_Position : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (AnimatePositionTarget == null); }
		public override string RequiredTargetText { get { return AnimatePositionTarget != null ? AnimatePositionTarget.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a AnimatePositionTarget and a Destination be set to be able to work properly. You can set one below."; } }
		#endif
		public enum Spaces { World, Local, RectTransform, Self }
		public enum Modes { AtoB, AlongCurve, ToDestination }
		public enum TimeScales { Scaled, Unscaled }

		[MMFInspectorGroup("Position Target（目标）", true, 61, true)]
		/// the object this feedback will animate the position for
		[Tooltip("目标对象")]
		public GameObject AnimatePositionTarget;

		[MMFInspectorGroup("Transition", true, 63)]
		/// the mode this animation should follow (either going from A to B, or moving along a curve)
		[Tooltip("此动画应遵循的模式（AtoB：从A到B，AlongCurve：沿曲线移动，ToDestination：从当前位置移动到目标点）")]
		public Modes Mode = Modes.AtoB;
		/// the space in which to move the position in
		[Tooltip("移动位置的空间")]
		public Spaces Space = Spaces.World;

		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		/// whether or not to randomize remap values between their base and alt values on play, useful to add some variety every time you play this feedback
		[Tooltip("是否在播放时随机化基本值和alt值之间的重映射值，在每次播放此反馈时添加一些变化非常有用")]
		public bool RandomizeRemap = false;
		/// the duration of the animation on play
		[Tooltip("播放动画的持续时间")]
		public float AnimatePositionDuration = 0.2f;

		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.ToDestination)]
		/// the acceleration of the movement
		[Tooltip("运动的加速度曲线")]
		public AnimationCurve AnimatePositionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		/// the value to remap the curve's 0 value to
		[Tooltip("要将曲线的0值重新映射到的值")]
		public float RemapCurveZero = 0f;

		[MMFCondition("RandomizeRemap", true)]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		/// in randomize remap mode, the value to remap the curve's 0 value to (randomized between this and RemapCurveZero)
		[Tooltip("在“随机化重映射”模式下，将曲线的0值重映射到的值（在此值和重映射曲线之间随机化）")]
		public float RemapCurveZeroAlt = 0f;

		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		[FormerlySerializedAs("CurveMultiplier")]
        /// the value to remap the curve's 1 value to
        [Tooltip("要将曲线的1值重新映射到的值")]
        public float RemapCurveOne = 1f;

		[MMFCondition("RandomizeRemap", true)]
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		/// in randomize remap mode, the value to remap the curve's 1 value to (randomized between this and RemapCurveOne)
		[Tooltip("在“随机化重映射”模式下，将曲线的1值重映射到的值（在此值和重映射曲线之间随机化）")]
		public float RemapCurveOneAlt = 1f;

		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		/// if this is true, the x position will be animated
		[Tooltip("如果这是真的，x轴将设置动画")]
		public bool AnimateX;

		[MMFCondition("AnimateX", true)]
		/// the acceleration of the movement
		[Tooltip("运动的加速度X轴曲线")]
		public AnimationCurve AnimatePositionCurveX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f));
		
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		/// if this is true, the y position will be animated
		[Tooltip("如果为真，y轴将设置动画")]
		public bool AnimateY;

		[MMFCondition("AnimateY", true)]
		/// the acceleration of the movement
		[Tooltip("运动的加速度Y轴曲线")]
		public AnimationCurve AnimatePositionCurveY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f));
		
		[MMFEnumCondition("Mode", (int)Modes.AlongCurve)]
		/// if this is true, the z position will be animated
		[Tooltip("如果为真，z轴将设置动画")]
		public bool AnimateZ;

		[MMFCondition("AnimateZ", true)]
		/// the acceleration of the movement
		[Tooltip("运动的加速度Z轴曲线")]
		public AnimationCurve AnimatePositionCurveZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果这是真的，调用该反馈将触发它，即使它正在进行中。如果为假，它将阻止任何新播放，直到当前播放结束")] 
		public bool AllowAdditivePlays = false;
        
		[MMFInspectorGroup("Positions", true, 64)]
		/// if this is true, the initial position won't be added to init and destination
		[Tooltip("如果这是真的，则初始位置不会添加到init和destination")]
		public bool RelativePosition = true;
		/// if this is true, initial and destination positions will be recomputed on every play
		[Tooltip("如果这是真的，将在每次播放时重新计算初始和目标位置")]
		public bool DeterminePositionsOnPlay = false;

		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.AlongCurve)]
        /// the initial position
        [Tooltip("初始位置")]
        public Vector3 InitialPosition = Vector3.zero;

		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.ToDestination)]
        /// the destination position
        [Tooltip("目的地位置")]
        public Vector3 DestinationPosition = Vector3.one;

		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.AlongCurve)]
        /// the initial transform - if set, takes precedence over the Vector3 above
        [Tooltip("初始Transform如果设置，它的Position属性将代替上边的Vevtor3变量：InitialPosition（初始位置）")]
        public Transform InitialPositionTransform;

		[MMFEnumCondition("Mode", (int)Modes.AtoB, (int)Modes.ToDestination)]
		/// the destination transform - if set, takes precedence over the Vector3 above
		[Tooltip("目的地Transform如果设置，它的Position属性将代替上边的Vevtor3变量：DestinationPosition（目的地位置）")]
		public Transform DestinationPositionTransform;

		/// <summary>
		/// 此反馈的持续时间就是其动画的持续时间
		/// the duration of this feedback is the duration of its animation
		/// </summary>
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimatePositionDuration); } set { AnimatePositionDuration = value; } }

		protected Vector3 _newPosition;
		protected Vector3 _currentPosition;
		protected RectTransform _rectTransform;
		protected Vector3 _initialPosition;
		protected Vector3 _destinationPosition;
		protected Coroutine _coroutine;
		protected Vector3 _workInitialPosition;
		protected float _remapCurveZero;
		protected float _remapCurveOne;

		/// <summary>
		/// 在初始化上，我们设置初始和目标位置
		/// On init, we set our initial and destination positions (transform will take precedence over vector3s)
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active)
			{
				if (AnimatePositionTarget == null)
				{
					Debug.LogWarning("The animate position target for " + this + " is null, you have to define it in the inspector");
					return;
				}

				if (Space == Spaces.RectTransform)
				{
					_rectTransform = AnimatePositionTarget.GetComponent<RectTransform>();
				}

				if (!DeterminePositionsOnPlay)
				{
					DeterminePositions();    
				}
			}
		}

		protected virtual void DeterminePositions()
		{
			if (DeterminePositionsOnPlay && RelativePosition && (InitialPosition != Vector3.zero))
			{
				return;
			}
            
			if (InitialPositionTransform != null)
			{
				_workInitialPosition = GetPosition(InitialPositionTransform);
			}
			else
			{
				_workInitialPosition = RelativePosition ? GetPosition(AnimatePositionTarget.transform) + InitialPosition : GetPosition(AnimatePositionTarget.transform);
			}
			if (Mode != Modes.ToDestination)
			{
				if (DestinationPositionTransform != null)
				{
					DestinationPosition = GetPosition(DestinationPositionTransform);
				}
				else
				{
					DestinationPosition = RelativePosition ? GetPosition(AnimatePositionTarget.transform) + DestinationPosition : DestinationPosition;
				}
			}  
		}

		/// <summary>
		/// 在播放中，我们将对象从A移动到B
		/// On Play, we move our object from A to B
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimatePositionTarget == null))
			{
				return;
			}
            
			if (Active || Owner.AutoPlayOnEnable)
			{
				if (DeterminePositionsOnPlay && NormalPlayDirection)
				{
					DeterminePositions();
				}
                
				switch (Mode)
				{
					case Modes.ToDestination:
						_initialPosition = GetPosition(AnimatePositionTarget.transform);
						_destinationPosition = RelativePosition ? _initialPosition + DestinationPosition : DestinationPosition;
						if (DestinationPositionTransform != null)
						{
							_destinationPosition = GetPosition(DestinationPositionTransform);
						}
						_coroutine = Owner.StartCoroutine(MoveFromTo(AnimatePositionTarget, _initialPosition, _destinationPosition, FeedbackDuration, AnimatePositionCurve));
						break;
					case Modes.AtoB:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						_coroutine = Owner.StartCoroutine(MoveFromTo(AnimatePositionTarget, _workInitialPosition, DestinationPosition, FeedbackDuration, AnimatePositionCurve));
						break;
					case Modes.AlongCurve:
						if (!AllowAdditivePlays && (_coroutine != null))
						{
							return;
						}
						float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;

						_remapCurveZero = RandomizeRemap ? Random.Range(RemapCurveZero, RemapCurveZeroAlt) : RemapCurveZero;
						_remapCurveOne = RandomizeRemap ? Random.Range(RemapCurveOne, RemapCurveOneAlt) : RemapCurveOne;
						
						_coroutine = Owner.StartCoroutine(MoveAlongCurve(AnimatePositionTarget, _workInitialPosition, FeedbackDuration, intensityMultiplier));
						break;
				}                    
			}
		}

		/// <summary>
		/// 沿曲线移动对象
		/// Moves the object along a curve
		/// </summary>
		/// <param name="movingObject"></param>
		/// <param name="pointA"></param>
		/// <param name="pointB"></param>
		/// <param name="duration"></param>
		/// <param name="curve"></param>
		/// <returns></returns>
		protected virtual IEnumerator MoveAlongCurve(GameObject movingObject, Vector3 initialPosition, float duration, float intensityMultiplier)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : duration;
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);

				ComputeNewCurvePosition(movingObject, initialPosition, percent, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			ComputeNewCurvePosition(movingObject, initialPosition, FinalNormalizedTime, intensityMultiplier);
			_coroutine = null;
			IsPlaying = false;
			yield break;
		}

		/// <summary>
		/// 计算位置曲线并计算新位置
		/// Evaluates the position curves and computes the new position
		/// </summary>
		/// <param name="movingObject"></param>
		/// <param name="initialPosition"></param>
		/// <param name="percent"></param>
		protected virtual void ComputeNewCurvePosition(GameObject movingObject, Vector3 initialPosition, float percent, float intensityMultiplier)
		{
			float newValueX = AnimatePositionCurveX.Evaluate(percent);
			float newValueY = AnimatePositionCurveY.Evaluate(percent);
			float newValueZ = AnimatePositionCurveZ.Evaluate(percent);

			newValueX = MMFeedbacksHelpers.Remap(newValueX, 0f, 1f, _remapCurveZero * intensityMultiplier, _remapCurveOne * intensityMultiplier);
			newValueY = MMFeedbacksHelpers.Remap(newValueY, 0f, 1f, _remapCurveZero * intensityMultiplier, _remapCurveOne * intensityMultiplier);
			newValueZ = MMFeedbacksHelpers.Remap(newValueZ, 0f, 1f, _remapCurveZero * intensityMultiplier, _remapCurveOne * intensityMultiplier);

			_newPosition = initialPosition;
			_currentPosition = GetPosition(movingObject.transform);

			if (RelativePosition)
			{
				_newPosition.x = AnimateX ? initialPosition.x + newValueX : _currentPosition.x;
				_newPosition.y = AnimateY ? initialPosition.y + newValueY : _currentPosition.y;
				_newPosition.z = AnimateZ ? initialPosition.z + newValueZ : _currentPosition.z;
			}
			else
			{
				_newPosition.x = AnimateX ? newValueX : _currentPosition.x;
				_newPosition.y = AnimateY ? newValueY : _currentPosition.y;
				_newPosition.z = AnimateZ ? newValueZ : _currentPosition.z;
			}

			if (Space == Spaces.Self)
			{
				_newPosition.x = AnimateX ? newValueX : 0f;
				_newPosition.y = AnimateY ? newValueY : 0f;
				_newPosition.z = AnimateZ ? newValueZ : 0f;
			}
			
			SetPosition(movingObject.transform, _newPosition);
		}

		/// <summary>
		/// 在给定时间内将对象从点A移动到点B
		/// Moves an object from point A to point B in a given time
		/// </summary>
		/// <param name="movingObject">Moving object.</param>
		/// <param name="pointA">Point a.</param>
		/// <param name="pointB">Point b.</param>
		/// <param name="duration">Time.</param>
		protected virtual IEnumerator MoveFromTo(GameObject movingObject, Vector3 pointA, Vector3 pointB, float duration, AnimationCurve curve = null)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : duration;
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);
				_newPosition = Vector3.LerpUnclamped(pointA, pointB, curve.Evaluate(percent));

				SetPosition(movingObject.transform, _newPosition);
				
				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

			// set final position
			if (NormalPlayDirection)
			{
				SetPosition(movingObject.transform, pointB);    
			}
			else
			{
				SetPosition(movingObject.transform, pointA);
			}
			_coroutine = null;
			IsPlaying = false;
			yield break;
		}

		/// <summary>
		/// 获取世界、本地或锚点位置
		/// Gets the world, local or anchored position
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual Vector3 GetPosition(Transform target)
		{
			switch (Space)
			{
				case Spaces.World:
					return target.position;
				case Spaces.Local:
					return target.localPosition;
				case Spaces.RectTransform:
					return target.gameObject.GetComponent<RectTransform>().anchoredPosition;
				case Spaces.Self:
					return target.position;
			}
			return Vector3.zero;
		}

		/// <summary>
		/// 设置目标的位置、localposition或anchoredposition
		/// Sets the position, localposition or anchoredposition of the target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="newPosition"></param>
		protected virtual void SetPosition(Transform target, Vector3 newPosition)
		{
			switch (Space)
			{
				case Spaces.World:
					target.position = newPosition;
					break;
				case Spaces.Local:
					target.localPosition = newPosition;
					break;
				case Spaces.RectTransform:
					_rectTransform.anchoredPosition = newPosition;
					break;
				case Spaces.Self:
					target.position = _workInitialPosition;
					if ((Mode == Modes.AtoB) || (Mode == Modes.ToDestination))
					{
						newPosition -= _workInitialPosition;
					}
					target.Translate(newPosition, target);
					break;
			}
		}

		/// <summary>
		/// 停止，如果移动处于活动状态，则中断移动
		/// On stop, we interrupt movement if it was active
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
			IsPlaying = false;
			Owner.StopCoroutine(_coroutine);
			_coroutine = null;
		}

		/// <summary>
		/// On disable we reset our coroutine
		/// </summary>
		public override void OnDisable()
		{
			_coroutine = null;
		}
	}
}