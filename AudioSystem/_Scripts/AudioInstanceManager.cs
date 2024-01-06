using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioSystem._Scripts.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace AudioSystem._Scripts
{
    public class AudioInstanceManager : MonoBehaviour
    {
        public Dictionary<string, EasyAudioType> AudioDict { get; private set; }
        public static AudioInstanceManager Instance { get; private set; }

        private void Awake()
        {
            if(Instance != null)
                return;
            
            Instance = this;
            
            AudioDict = new Dictionary<string, EasyAudioType>();
        }

        //检查是否播放完毕，是则清理资源
        public async Task RemoveClipResource()
        {
            await Task.Delay(1000);
            foreach (var type in AudioDict)
            {
                if(type.Value.sourceObject == null)
                    return;
                if (type.Value.sourceObject.GetComponent<AudioSource>().clip != null)
                {
                    if (CheckIsPlayEnd(type.Key) && type.Value.autoDelete)
                    {
                        await RemoveClipAsync(type.Key);
                    }
                }
            }
        }

        //创建实例，添加对象和初始化设置，返回实例名
        public string CreateAudioInstance(EasyAudioType type, Transform pos = null)
        {
            //检查重命名
            var audioName = AudioSettingManager.CheckAudioName(type.audioAddressName);
            
            //创建实例对象
            if (pos == null)
                pos = transform;
            type.sourceObject = AudioSourcePool.Instance.GetAudioSourceGameObject(pos);
            
            
            //添加字典
            AudioDict.Add(audioName,type);
            
            //初始化音频设置
            AudioSettingManager.ReloadAudioData(audioName,type);
            
            return audioName;
        }

        //删除实例
        public bool RemoveAudioInstance(string audioName)
        {
            //todo:删除实例失败，未返回对象池
            var type = AudioDict[audioName];
            if (type == null)
            {
                Debug.LogWarning($"未找到声音资源 {audioName}");
                return false;
            }
            
            //对象池回收物品
            AudioSourcePool.Instance.ReturnAudioSource(type.sourceObject);
            
            //从字典中删除
            AudioDict.Remove(audioName);
            
            return true;
        }

        #region 音频状态

        //播放触发事件委托
        private Dictionary<AudioState, Action<EasyAudioType>> audioPlayEventMap = new Dictionary<AudioState, Action<EasyAudioType>>()
        {
            { AudioState.Play, type => type.sourceObject.GetComponent<AudioSource>().Play() },
            { AudioState.Pause, type => type.sourceObject.GetComponent<AudioSource>().Pause() },
            { AudioState.Stop, type => type.sourceObject.GetComponent<AudioSource>().Stop() },
            { AudioState.UnPause, type => type.sourceObject.GetComponent<AudioSource>().UnPause() },
            { AudioState.Mute, type => type.sourceObject.GetComponent<AudioSource>().mute = true },
            { AudioState.UnMute, type => type.sourceObject.GetComponent<AudioSource>().mute = false }
        };

        //播放延迟触发事件委托
        private Dictionary<AudioState, Action<EasyAudioType, float>> audioPlayDelayEventMap = new Dictionary<AudioState, Action<EasyAudioType, float>>()
        {
            { AudioState.PlayDelayed, (type, value) => type.sourceObject.GetComponent<AudioSource>().PlayDelayed(value) },
            { AudioState.PlayScheduled, (type, value) => type.sourceObject.GetComponent<AudioSource>().PlayScheduled(value) }
        };

        //音频处理，不带延迟
        public async Task AudioEvent(string audioName, AudioState audioState,Action action)
        {
            //根据audioName查找字典AudioInstanceManager中的AudioType
            EasyAudioType type = AudioDict[audioName];
            if (type == null)
            {
                Debug.LogWarning($"未找到声音资源 {audioName}");
                return;
            }

            if (audioState == AudioState.PlayDelayed || audioState == AudioState.PlayScheduled)
            {
                Debug.LogWarning($"音频状态 {audioState} 无法使用此方法");
                return;
            }
            
            //检查是否是播放音频
            await CheckIsPlay(audioName,audioState);
        
            audioPlayEventMap[audioState](type);
            
            action?.Invoke();
        }
        
        //音频处理，带延迟
        public async Task AudioEvent(string audioName, AudioState audioState, float delay, Action action)
        {
            EasyAudioType type = AudioDict[audioName];
            if (type == null)
            {
                Debug.LogWarning($"未找到声音资源 {audioName}");
                return;
            }

            if (audioState == AudioState.PlayDelayed || audioState == AudioState.PlayScheduled)
            {
                await CheckIsPlay(audioName,audioState);
                
                audioPlayDelayEventMap[audioState](type, delay);
                
                action?.Invoke();
            }
        }

        #endregion

        //检查是否要播放音频
        private async Task CheckIsPlay(string audioName,AudioState audioState)
        {
            if (audioState == AudioState.Play || audioState == AudioState.PlayDelayed || audioState == AudioState.PlayScheduled)
            {
                var isPlay = await CheckClipAsync(AudioDict, audioName);
                if(!isPlay)
                {
                    Debug.LogWarning($"字典 AudioDict {audioName} 未找到音频，请先添加字典"); 
                }
            }
                
        }

        //检查音频是否加载
        private async Task<bool> CheckClipAsync(Dictionary<string, EasyAudioType> AudioDict,string audioName)
        {
            if (!AudioDict.TryGetValue(audioName, out EasyAudioType type))
            {
                Debug.LogWarning($"{audioName} 未加载，请先加载音频到字典"); 
                return false;
            }

            // 如果clip为空，检查是否有资源，如果有资源，加载资源并触发 callback 事件，如果没有资源则根据名字在addressable中加载资源
            if (type.clip == null)
            {
                //寻找资源
                var clip = await LoadAudioClipAsync(type.audioAddressName);
                var i = 0;
                
                while (clip == null && i < 3)
                {
                    clip = await LoadAudioClipAsync(type.audioAddressName);
                    i++;
                }
                
                var source = type.sourceObject;
                source.GetComponent<AudioSource>().clip = clip;
                type.source = source.GetComponent<AudioSource>();
            }
            else
            {
                var source = type.sourceObject;
                type.source = source.GetComponent<AudioSource>();
            }

           return true;
        }

        //加载资源逻辑
        private async Task<AudioClip> LoadAudioClipAsync(string audioAddressName)
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(audioAddressName);
            await handle.Task;
            return handle.Result;
        }

        private Task<bool> RemoveClipAsync(string audioName)
        {
            var type = AudioInstanceManager.Instance.AudioDict[audioName];
            if(type.sourceObject.GetComponent<AudioSource>().clip != null)
            {
                type.sourceObject.GetComponent<AudioSource>().clip = null;
                return Task.FromResult(true);
            }
            else
            {
                Debug.LogWarning($"{audioName} 音频资源为空");
                return Task.FromResult(false);
            }
        }

        #region 播放完毕事件
        
        //检测是否播放完毕
        public bool CheckAudioClipFinished(EasyAudioType type)
        {
            if(type.sourceObject == null)
                return true;
            var audioSource = type.sourceObject.GetComponent<AudioSource>();
            if (audioSource.clip == null)
            {
                return true;
            }
            return audioSource.time >= audioSource.clip.length;
        }

        //检查是否播放结束
        public bool CheckIsPlayEnd(string audioName)
        {
            if (AudioDict.TryGetValue(audioName, out EasyAudioType audioType))
            {
                return CheckAudioClipFinished(audioType);
            }
            else
            {
                Debug.LogWarning($"{audioName} 未加载，请先加载音频到字典");
                return false;
            }

        }

        #endregion
    }
}