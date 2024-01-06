using System.Collections.Generic;
using UnityEngine;

namespace AudioSystem._Scripts
{
    public class AudioSourcePool : MonoBehaviour
    {
        private readonly Queue<GameObject> _audioSourceQueue = new Queue<GameObject>();
        [SerializeField]private int _poolSize = 10;
        
        public static AudioSourcePool Instance;

        private void Awake()
        {
            if(Instance != null)
                return;
            
            Instance = this;

            Init();
        }

        //初始化对象池
        private void Init()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                CreateNewAudioSource(transform);
            }
        }

        //从对象池获取对象并设置位置
        public GameObject GetAudioSourceGameObject(Transform pos)
        {
            GameObject audioSourceObject = null;
            if (_audioSourceQueue.Count > 0)
            {
                audioSourceObject = _audioSourceQueue.Dequeue();
                if (audioSourceObject == null)
                {
                    return GetAudioSourceGameObject(pos);
                }
            }
            else
            {
                CreateNewAudioSource(pos);
                audioSourceObject = _audioSourceQueue.Dequeue();
            }

            audioSourceObject.transform.position = pos.position;
            return audioSourceObject;
        }

        //回收音频对象
        public void ReturnAudioSource(GameObject audioSourceObject)
        {
            if (audioSourceObject == null || !_audioSourceQueue.Contains(audioSourceObject))
            {
                Destroy(audioSourceObject);
                return;
            }

            audioSourceObject.GetComponent<AudioSource>().Stop();
            _audioSourceQueue.Enqueue(audioSourceObject);
        }

        //创建一个音频对象
        private GameObject CreateNewAudioSource(Transform pos)
        {
            //创建一个物体，添加音频组件
            var sourceObject = new GameObject("AudioSource");
            sourceObject.transform.SetParent(transform);
            sourceObject.transform.position = pos.position;
            sourceObject.AddComponent<AudioSource>();

            //将音频对象添加到对象池中
            _audioSourceQueue.Enqueue(sourceObject);

            //返回创建的音频对象
            return sourceObject;
        }
        
        private void OnDisable()
        {
            ClearAudioSources();
        }

        private void OnDestroy()
        {
            ClearAudioSources();
        }

        private void ClearAudioSources()
        {
            foreach (var audioSourceObject in _audioSourceQueue)
            {
                Destroy(audioSourceObject);
            }

            _audioSourceQueue.Clear();
        }
    }
}