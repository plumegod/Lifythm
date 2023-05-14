using System.Collections;
using System.Collections.Generic;
using SonicBloom.Koreo;
using UnityEngine;
using UnityEngine.Serialization;

public class LaneController : MonoBehaviour
{
    #region Fields

    private SpriteRenderer targetVisuals;

    [Tooltip("Koreography 事件在此 Lane 中包含的有效载荷字符串列表。")]
    public List<string> matchedPayloads = new List<string>();

    // 包含此 Lane 中所有事件的列表。这些事件是由 audio game controller 添加的。
    List<KoreographyEvent> laneEvents = new List<KoreographyEvent>();

    // 包含此 Lane 中当前激活（屏幕上显示）的所有 Note Objects 的队列。使用此队列进行输入和
    // 生命周期有效性检查。
    Queue<NoteObject> trackedNotes = new Queue<NoteObject>();

    // 对 audiogamecontroller 的引用。提供对 NoteObject 池和其他参数的访问。
    AudioGameController gameController;

    // 生命周期边界。
    private Transform spawn;
    private Transform hitspawn;
    private Transform despawn;

    // 在此 Lane 中检查下一个事件的出现时间的索引。
    int pendingEventIdx = 0;

    [SerializeField] private Sprite sprite;
    
    [SerializeField] private string laneName;

    public string LaneName
    {
        get => laneName;
        set => laneName = value;
    }

    #endregion

    // 命中点的位置
    public Vector3 TargetPosition
    {
        get
        {
            return new Vector3(hitspawn.position.x, hitspawn.position.y, 0.0f);
        }
    }

    public float DeSpawnX {
        get
        {
            return despawn.position.x;
        }
        set
        {
            var despawnPosition = new Vector2(value, despawn.position.y);
            despawn.position = despawnPosition;
        }
    }

    #region Methods

    public void Initialize(AudioGameController controller)
    {
        gameController = controller;
    }

    // 此方法控制清理，将内部重置为新状态。
    public void Restart()
    {
        pendingEventIdx = 0;

        // 清除已跟踪的音符。
        int numToClear = trackedNotes.Count;
        for (int i = 0; i < numToClear; ++i)
        {
            trackedNotes.Dequeue().OnClear();
        }
    }

    void Start()
    {
        var parent = transform.parent;
        targetVisuals = parent.GetComponent<SpriteRenderer>();
        spawn = parent.Find("Position/Spawn");
        hitspawn = parent.Find("Position/HitNick");
        despawn = parent.Find("Position/End");
    }

    void Update()
    {
        // 清除无效条目。
        /*
        while (trackedNotes.Count > 0 && trackedNotes.Peek().IsNoteMissed())
        {
            trackedNotes.Dequeue();
        }
        */

        // 检查新的生成。
        CheckSpawnNext();
    }
    
    // 使用目标位置和当前音符对象速度来确定音频样本“位置”的生成位置。
    // 此值相对于目标位置（“当前”时间）的音频样本位置。
    int GetSpawnSampleOffset()
    {
        // 给定当前速度，我们的当前位置的样本偏移量是多少？
        float spawnDistToTarget = spawn.position.x - hitspawn.position.x;

        // 在当前速度下，到达该位置需要多长时间？
        double spawnSecsToTarget = (double)spawnDistToTarget / (double)gameController.noteSpeed;

        // 计算到目标的样本数。
        return (int)(spawnSecsToTarget * gameController.SampleRate);
    }

    // 检查是否应该生成下一个音符对象。如果是，则生成音符对象并将其添加到trackedNotes队列中。
    void CheckSpawnNext()
    {
        int samplesToTarget = GetSpawnSampleOffset();

        int currentTime = gameController.DelayedSampleTime;

        // 对所有事件进行生成。
        while (pendingEventIdx < laneEvents.Count &&
               laneEvents[pendingEventIdx].StartSample < currentTime + samplesToTarget)
        {
            KoreographyEvent evt = laneEvents[pendingEventIdx];

            NoteObject newObj = gameController.GetFreshNoteObject();
            newObj.Initialize(evt, sprite, this, gameController,laneName);

            trackedNotes.Enqueue(newObj);

            pendingEventIdx++;
        }
    }

    // 将一个 KoreographyEvent 添加到 Lane 中。KoreographyEvent 包含定义 Note 对象何时出现在屏幕上的时间信息。
    public void AddEventToLane(KoreographyEvent evt)
    {
        laneEvents.Add(evt);
    }

    // 检查传入的字符串值是否与 matchedPayloads List 中配置的任何值匹配。
    public bool DoesMatchPayload(string payload)
    {
        bool bMatched = false;

        for (int i = 0; i < matchedPayloads.Count; ++i)
        {
            if (payload == matchedPayloads[i])
            {
                bMatched = true;
                break;
            }
        }

        return bMatched;
    }

    #endregion
}
