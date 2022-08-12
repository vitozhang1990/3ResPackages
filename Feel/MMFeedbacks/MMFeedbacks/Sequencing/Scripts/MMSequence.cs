using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// the possible states for sequence notes
	public enum MMSequenceTrackStates { Idle, Down, Up }

	/// <summary>
	/// 序列注释类
	/// 描述序列注释内容的类，基本上是一个时间戳和在该时间戳上播放的ID
	/// A class describing the contents of a sequence note, basically a timestamp and the ID to play at that timestamp
	/// </summary>
	[System.Serializable]
	public class MMSequenceNote
	{
		public float Timestamp;
		public int ID;

		public virtual MMSequenceNote Copy()
		{
			MMSequenceNote newNote = new MMSequenceNote();
			newNote.ID = this.ID;
			newNote.Timestamp = this.Timestamp;
			return newNote;
		}
	}

	/// <summary>
	/// 一个描述序列轨迹属性的类:ID，颜色(用于检查器)，键(用于记录器)，状态(用于记录器)
	/// A class describing the properties of a sequence's track : ID, color (for the inspector), Key (for the recorder), State (for the recorder)
	/// </summary>
	[System.Serializable]
	public class MMSequenceTrack
	{
		public int ID = 0;
		public Color TrackColor;
		public KeyCode Key = KeyCode.Space;
		public bool Active = true;
		[MMFReadOnly]
		public MMSequenceTrackStates State = MMSequenceTrackStates.Idle;
		[HideInInspector]
		public bool Initialized = false;
        
		public virtual void SetDefaults(int index)
		{
			if (!Initialized)
			{
				ID = index;
				TrackColor = MMSequence.RandomSequenceColor();
				Key = KeyCode.Space;
				Active = true;
				State = MMSequenceTrackStates.Idle;
				Initialized = true;
			}            
		}
	}

	/// <summary>
	/// 用于存储序列注释的类
	/// </summary>
	[System.Serializable]
	public class MMSequenceList
	{
		public List<MMSequenceNote> Line;
	}

	/// <summary>
	/// 反馈序列
	/// 按顺序记录和播放反馈事件数据
	/// 反馈序列可以通过反馈从他们的计时部分，由序列器和潜在的其他类播放
	/// This scriptable object holds "sequences", data used to record and play events in sequence
	/// MMSequences can be played by MMFeedbacks from their Timing section, by Sequencers and potentially other classes
	/// </summary>
	[CreateAssetMenu(menuName = "MoreMountains/Sequencer/MMSequence")]
	public class MMSequence : ScriptableObject
	{
		[Header("序列")]
		/// the length (in seconds) of the sequence
		[Tooltip("序列的长度（以秒为单位）")]
		[MMFReadOnly]
		public float Length;
		/// the original sequence (as outputted by the input sequence recorder)
		[Tooltip("原始序列（由输入序列记录器输出）")]
		public MMSequenceList OriginalSequence;
		/// the duration in seconds to apply after the last input
		[Tooltip("在最后一次输入之后应用的持续时间(以秒为单位)")]
		public float EndSilenceDuration = 0f;

		[Header("序列的内容")]
		/// the list of tracks for this sequence
		[Tooltip("此序列的轨迹列表")]
		public List<MMSequenceTrack> SequenceTracks;

		[Header("量化")]
		/// whether this sequence should be used in quantized form or not
		[Tooltip("该序列是否应以量化形式使用")]
		public bool Quantized;
		/// the target BPM for this sequence
		[Tooltip("此序列的目标BPM")]
		public int TargetBPM = 120;
		/// the contents of the quantized sequence
		[Tooltip("量化序列的内容列表")]
		public List<MMSequenceList> QuantizedSequence;
        
		[Space]
		[Header("控制")]
		[MMFInspectorButton("RandomizeTrackColors")]
		[Tooltip("随机跟踪颜色按钮")]
		public bool RandomizeTrackColorsButton;

		/// <summary>
		/// 量子化的节拍?
		/// </summary>
		protected float[] _quantizedBeats; 
		/// <summary>
		/// 序列注释类的删除列表
		/// </summary>
		protected List<MMSequenceNote> _deleteList;

		/// <summary>
		/// 比较和排序两个序列注释
		/// Compares and sorts two sequence notes
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		static int SortByTimestamp(MMSequenceNote p1, MMSequenceNote p2)
		{
			return p1.Timestamp.CompareTo(p2.Timestamp);
		}

		/// <summary>
		/// 根据时间戳对原始序列进行排序
		/// Sorts the original sequence based on timestamps
		/// </summary>
		public virtual void SortOriginalSequence()
		{
			OriginalSequence.Line.Sort(SortByTimestamp);
		}

        /// <summary>
        /// 量化原始序列，填充量化序列列表，按节拍安排事件
        /// Quantizes the original sequence, filling the QuantizedSequence list, arranging events on the beat
        /// </summary>
        public virtual void QuantizeOriginalSequence()
		{
			ComputeLength();
			QuantizeSequenceToBPM(OriginalSequence.Line);
		}

		/// <summary>
		/// 计算序列的长度
		/// Computes the length of the sequence
		/// </summary>
		public virtual void ComputeLength()
		{
			Length = OriginalSequence.Line[OriginalSequence.Line.Count - 1].Timestamp + EndSilenceDuration;
		}

		/// <summary>
		/// 使序列中的每个时间戳与BPM轨迹匹配
		/// Makes every timestamp in the sequence match the BPM track
		/// </summary>
		public virtual void QuantizeSequenceToBPM(List<MMSequenceNote> baseSequence)
		{
			float sequenceLength = Length;
			float beatDuration = 60f / TargetBPM;
			int numberOfBeatsInSequence = (int)(sequenceLength / beatDuration);
			QuantizedSequence = new List<MMSequenceList>();
			_deleteList = new List<MMSequenceNote>();
			_deleteList.Clear();

			// we fill the BPM track with the computed timestamps
			_quantizedBeats = new float[numberOfBeatsInSequence];
			for (int i = 0; i < numberOfBeatsInSequence; i++)
			{
				_quantizedBeats[i] = i * beatDuration;
			}
            
			for (int i = 0; i < SequenceTracks.Count; i++)
			{
				QuantizedSequence.Add(new MMSequenceList());
				QuantizedSequence[i].Line = new List<MMSequenceNote>();
				for (int j = 0; j < numberOfBeatsInSequence; j++)
				{
					MMSequenceNote newNote = new MMSequenceNote();
					newNote.ID = -1;
					newNote.Timestamp = _quantizedBeats[j];
					QuantizedSequence[i].Line.Add(newNote);

					foreach (MMSequenceNote note in baseSequence)
					{
						float newTimestamp = RoundFloatToArray(note.Timestamp, _quantizedBeats);
						if ((newTimestamp == _quantizedBeats[j]) && (note.ID == SequenceTracks[i].ID))
						{
							QuantizedSequence[i].Line[j].ID = note.ID;
						}
					}
				}
			}        
		}

		/// <summary>
		/// 在验证时，我们初始化轨迹的属性
		/// On validate, we initialize our track's properties
		/// </summary>
		protected virtual void OnValidate()
		{
			for (int i = 0; i < SequenceTracks.Count; i++)
			{
				SequenceTracks[i].SetDefaults(i);
			}
		}

		/// <summary>
		/// 随机化轨迹颜色
		/// Randomizes track colors
		/// </summary>
		protected virtual void RandomizeTrackColors()
		{
			foreach(MMSequenceTrack track in SequenceTracks)
			{
				track.TrackColor = RandomSequenceColor();
			}
		}

		/// <summary>
		/// 返回序列轨迹的随机颜色
		/// Returns a random color for the sequence tracks
		/// </summary>
		/// <returns></returns>
		public static Color RandomSequenceColor()
		{
			int random = UnityEngine.Random.Range(0, 32);
			switch (random)
			{
				case 0: return new Color32(240, 248, 255, 255); 
				case 1: return new Color32(127, 255, 212, 255);
				case 2: return new Color32(245, 245, 220, 255);
				case 3: return new Color32(95, 158, 160, 255);
				case 4: return new Color32(255, 127, 80, 255);
				case 5: return new Color32(0, 255, 255, 255);
				case 6: return new Color32(255, 215, 0, 255);
				case 7: return new Color32(255, 0, 255, 255);
				case 8: return new Color32(50, 128, 120, 255);
				case 9: return new Color32(173, 255, 47, 255);
				case 10: return new Color32(255, 105, 180, 255);
				case 11: return new Color32(75, 0, 130, 255);
				case 12: return new Color32(255, 255, 240, 255);
				case 13: return new Color32(124, 252, 0, 255);
				case 14: return new Color32(255, 160, 122, 255);
				case 15: return new Color32(0, 255, 0, 255);
				case 16: return new Color32(245, 255, 250, 255);
				case 17: return new Color32(255, 228, 225, 255);
				case 18: return new Color32(218, 112, 214, 255);
				case 19: return new Color32(255, 192, 203, 255);
				case 20: return new Color32(255, 0, 0, 255);
				case 21: return new Color32(196, 112, 255, 255);
				case 22: return new Color32(250, 128, 114, 255);
				case 23: return new Color32(46, 139, 87, 255);
				case 24: return new Color32(192, 192, 192, 255);
				case 25: return new Color32(135, 206, 235, 255);
				case 26: return new Color32(0, 255, 127, 255);
				case 27: return new Color32(210, 180, 140, 255);
				case 28: return new Color32(0, 128, 128, 255);
				case 29: return new Color32(255, 99, 71, 255);
				case 30: return new Color32(64, 224, 208, 255);
				case 31: return new Color32(255, 255, 0, 255);
				case 32: return new Color32(154, 205, 50, 255);
			}
			return new Color32(240, 248, 255, 255); 
		}

        /// <summary>
        /// 将浮点舍入到数组中最接近的浮点（数组必须排序）
		/// Rounds a float to the closest float in an array (array has to be sorted)
		/// </summary>
		/// <param name="value"></param>
		/// <param name="array"></param>
		/// <returns></returns>
		public static float RoundFloatToArray(float value, float[] array)
		{
			int min = 0;
			if (array[min] >= value) return array[min];

			int max = array.Length - 1;
			if (array[max] <= value) return array[max];

			while (max - min > 1)
			{
				int mid = (max + min) / 2;

				if (array[mid] == value)
				{
					return array[mid];
				}
				else if (array[mid] < value)
				{
					min = mid;
				}
				else
				{
					max = mid;
				}
			}

			if (array[max] - value <= value - array[min])
			{
				return array[max];
			}
			else
			{
				return array[min];
			}
		}
	}
}