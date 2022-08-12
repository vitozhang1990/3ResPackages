﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈将允许您在播放时更改场景的skybox，将其替换为另一个skybox，可以是特定的skybox或在多个skybox中随机选取的skybox。
	/// This feedback will let you change the scene's skybox on play, replacing it with another one, either a specific one, or one picked at random among multiple skyboxes.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将允许您在播放时更改场景的skybox，将其替换为另一个skybox，可以是特定的skybox或在多个skybox中随机选取的skybox。")]
	[FeedbackPath("Renderer（渲染器）/Skybox（更换天空盒）")]
	public class MMFeedbackSkybox : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		#endif
        
		/// whether skyboxes are selected at random or not
		public enum Modes { Single, Random }

		[Header("Skybox")]
		/// the selected mode 
		public Modes Mode = Modes.Single;
		/// the skybox to assign when in Single mode
		public Material SingleSkybox;
		/// the skyboxes to pick from when in Random mode
		public Material[] RandomSkyboxes;
        
		/// <summary>
		/// On play, we set the scene's skybox to a new one
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (Mode == Modes.Single)
			{
				RenderSettings.skybox = SingleSkybox;
			}
			else if (Mode == Modes.Random)
			{
				RenderSettings.skybox = RandomSkyboxes[Random.Range(0, RandomSkyboxes.Length)];
			}
		}
	}    
}