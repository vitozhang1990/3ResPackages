using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 默认情况下，此反馈不起任何作用，它只是一个注释，您可以将文本存储在其中以供将来参考，也许是为了记住如何设置特定的反馈。可选地，它还可以在播放时将该注释输出到控制台。
	/// This feedback doesn't do anything by default, it's just meant as a comment, you can store text in it for future reference, maybe to remember how you setup a particular MMFeedbacks. Optionnally it can also output that comment to the console on Play.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("默认情况下，此反馈不起任何作用，它只是一个注释，您可以将文本存储在其中以供将来参考，也许是为了记住如何设置特定的反馈。可选地，它还可以在播放时将该注释输出到控制台。")]
	[FeedbackPath("Debug/Comment（注释）")]
	public class MMF_DebugComment : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
		#endif
     
		[MMFInspectorGroup("Comment", true, 61)]
		/// the comment / note associated to this feedback 
		[Tooltip("the comment / note associated to this feedback")]
		[TextArea(10,30)] 
		public string Comment;

		/// if this is true, the comment will be output to the console on Play 
		[Tooltip("if this is true, the comment will be output to the console on Play")]
		public bool LogComment = false;
		/// the color of the message when in DebugLogTime mode
		[Tooltip("the color of the message when in DebugLogTime mode")]
		[MMCondition("LogComment", true)]
		public Color DebugColor = Color.gray;
        
		/// <summary>
		/// On Play we output our message to the console if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || !LogComment)
			{
				return;
			}
            
			Debug.Log(Comment);
		}
	}
}