using System.Collections.Generic;
using SonicBloom.Koreo;
using SonicBloom.Koreo.Players;
using UnityEngine;

public class AudioGameController : MonoBehaviour
{
    [Tooltip("用于目标生成的音轨事件 ID")]
    [EventID]
    public string eventID;

    [Tooltip("在其中检测输入作为命中的时间范围，包括过早和过晚的时间，单位为毫秒。")]
    [Range(8f, 150f)]
    public float hitWindowRangeInMS = 80;

    [Tooltip("音频播放速度。")]
    [Range(0.1f,2f)]
    public float audioSpeed = 1;

    [Tooltip("音符对象每秒所经过的单位数量。")]
    public float noteSpeed = 1f;

    [Tooltip("用于生成音符的原型（蓝图），可以是预制体。")]
    public NoteObject noteObjectArchetype;

    [Tooltip("代表事件所经过的轨道的轨道控制器对象列表。")]
    public List<LaneController> noteLanes = new List<LaneController>();

    [Tooltip("音频播放开始前提供的时间长度，单位为秒。在编辑器中播放时，此值的更改不会立即生效。")]
    public float leadInTime;

    [Tooltip("用于播放 Koreographed 音频的音频源。请确保在音乐播放器中禁用“自动播放唤醒”。")]
    public AudioSource audioCom;

    [SerializeField] private bool canPlay = false;
    
    public bool CanPlay {get => canPlay; set => canPlay = value;}

    [SerializeField] private bool played = false;
    [SerializeField] private bool isPlaying = false;

    public bool Played => played;
    
    // 音频播放开始前剩余的前导时间。
    float leadInTimeLeft;

    // 音频播放前剩余的时间（处理事件延迟）。
    float timeLeftToPlay;
    
    public float TimeLeftToPlay {get => timeLeftToPlay; set => timeLeftToPlay = value;}

    // 存储在 Koreographer 组件中的 Koreography 的本地缓存。
    Koreography playingKoreo;

    // Koreographer 以样本为单位工作。将用户可见值转换为样本时间。这将简化整个计算。
    int hitWindowRangeInSamples;	// 可行事件可以被命中的样本范围。
	
    // 用于包含音符对象的对象池，以减少不必要的实例化/销毁。
    Stack<NoteObject> noteObjectPool = new Stack<NoteObject>();
    
    // 由Koreography指定的采样率。
    public int SampleRate
    {
        get
        {
            return playingKoreo.SampleRate;
        }
    }

    // 包括任何必要延迟的当前采样时间。
    public int DelayedSampleTime
    {
        get
        {
            // 将Koreographer报告的时间偏移可能的leadInTime量。
            return playingKoreo.GetLatestSampleTime() - (int)(audioCom.pitch * leadInTimeLeft * SampleRate);
        }
    }
    
    void Start()
		{
			InitializeLeadIn();

			// 初始化所有轨道
			for (int i = 0; i < noteLanes.Count; ++i) {
				noteLanes[i].Initialize(this);
			}

			// 初始化事件
			playingKoreo = Koreographer.Instance.GetKoreographyAtIndex(0); // 获取 Koreography 实例
			KoreographyTrack rhythmTrack = playingKoreo.GetTrackByID(eventID); // 通过 ID 获取轨道
			List<KoreographyEvent> rawEvents = rhythmTrack.GetAllEvents(); // 获取所有事件

			for (int i = 0; i < rawEvents.Count; ++i) {
				KoreographyEvent evt = rawEvents[i];
				string payload = evt.GetTextValue(); // 获取事件的负载信息

				// 找到正确的轨道
				for (int j = 0; j < noteLanes.Count; ++j) {
					LaneController lane = noteLanes[j];
					if (lane.DoesMatchPayload(payload)) {
						// 将事件添加到轨道上进行跟踪
						lane.AddEventToLane(evt);

						// 退出轨道搜索循环
						break;
					}
				}
			}
		}

		// 设置引导时间，如果指定的引导时间为零，则立即开始音频播放
		void InitializeLeadIn() {

			// 仅当有引导时间时才初始化引导时间
			if (leadInTime > 0f) {
				// 设置延迟开始播放
				leadInTimeLeft = leadInTime;
				timeLeftToPlay = leadInTime - Koreographer.Instance.EventDelayInSeconds;
			}
			else if(leadInTime <= 0f){
				// 立即播放并处理歌曲中的偏移量。负零和零相同，因此这不是一个问题。
				audioCom.time = -leadInTime;
				audioCom.pitch = audioSpeed;
				audioCom.Play();
				isPlaying = true;
			}
		}

		void Update() {
			// 这应该在 Start() 中完成。这里允许使用 Inspector 修改进行测试。
			UpdateInternalValues();
			
			if (Time.timeScale == 0)
			{
				return;
				audioCom.Stop();
			}
			
			
			
			if(!CanPlay)
				return;
			
			// 倒计时引导时间
			if (leadInTimeLeft > 0f) {
				leadInTimeLeft = Mathf.Max(leadInTimeLeft - Time.unscaledDeltaTime, 0f);
			}

			// 倒计时剩余时间
			if (timeLeftToPlay > 0f) {
				timeLeftToPlay -= Time.unscaledDeltaTime;

				// 检查是否是开始播放的时间
				if (timeLeftToPlay <= 0f) {
					audioCom.time = -timeLeftToPlay;
					audioCom.Play();
					isPlaying = true;

					timeLeftToPlay = 0f;
				}
			}

			if (isPlaying)
			{
				UpdateStop();
			}
		}

		// 更新依赖于公共或 Inspector 驱动的外部字段的任何内部值
		void UpdateInternalValues() {
			hitWindowRangeInSamples = (int)(0.001f * hitWindowRangeInMS * SampleRate);
		}

		// 从对象池中获取一个新的 NoteObject 对象。
		public NoteObject GetFreshNoteObject()
		{
			NoteObject retObj;

			if (noteObjectPool.Count > 0)
			{
				retObj = noteObjectPool.Pop();// 从对象池中弹出一个 NoteObject 对象。
			}
			else
			{
				retObj = Instantiate(noteObjectArchetype);  // 如果对象池为空，则创建一个新的 NoteObject 对象。
			}
			
			retObj.gameObject.SetActive(true);  // 激活该 NoteObject 对象。
			retObj.enabled = true;

			return retObj;  // 返回新的 NoteObject 对象。
		}

		// 将一个 NoteObject 对象返回到对象池中并将其禁用。
		public void ReturnNoteObjectToPool(NoteObject obj)
		{
			if (obj != null)
			{
				obj.enabled = false; // 禁用该 NoteObject 对象。
				obj.gameObject.SetActive(false); // 将该 NoteObject 对象设为非激活状态。

				noteObjectPool.Push(obj);  // 将该 NoteObject 对象返回到对象池中。
			}
		}

		// 重新开始游戏，使所有的音符轨道和任何激活的 NoteObject 对象重置或清除。
		public void Restart()
		{
			// 重置音频。
			audioCom.Stop();
			audioCom.time = 0f;

			// 清空延迟事件更新队列。这实际上是重置 Koreography 并确保尚未发送的延迟事件不会继续发送。
			Koreographer.Instance.FlushDelayQueue(playingKoreo);

			// 重置 Koreography 时间。通常由加载 Koreography 处理。由于我们只是重新开始，需要自己处理。
			playingKoreo.ResetTimings();

			// 重置所有音符轨道以便重新开始追踪。
			for (int i = 0; i < noteLanes.Count; ++i)
			{
				noteLanes[i].Restart();
			}

			// 重新初始化引导时间。
			InitializeLeadIn();
		}

		public void UpdateStop()
		{
			if ((int)audioCom.time >= (int)audioCom.clip.length)
			{
				isPlaying = false;
				played = true;
			}
		}
}
