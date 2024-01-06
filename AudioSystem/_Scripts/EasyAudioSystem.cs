using System;
using System.Threading.Tasks;
using AudioSystem._Scripts.Data;
using UnityEngine;
using static AudioSystem._Scripts.AudioInstanceManager;

namespace AudioSystem._Scripts
{
    public class EasyAudioSystem : MonoBehaviour
    {
        public static EasyAudioSystem Instance { get; private set; }

        private void Awake()
        {
            if(Instance != null)
                return;
            
            Instance = this;
        }

        private async void Update()
        {
            await AudioInstanceManager.Instance.RemoveClipResource();
        }

        public string CreateAudioSource(EasyAudioType easyAudioType,Transform pos = null)
        {
            var audioName = AudioInstanceManager.Instance.CreateAudioInstance(easyAudioType, pos);
            
            return audioName;
        }

        public void RemoveAudioSource(string audioName)
        {
            AudioInstanceManager.Instance.RemoveAudioInstance(audioName);
        }

        public void UpdateAudioSetting<T>(string audioName, AudioSettingType settingType, T value)
        {
            var audioType = AudioInstanceManager.Instance.AudioDict[audioName];
            if (audioType == null)
            {
                Debug.LogWarning($"{audioName} 音频不存在");
                return;
            }
            
            AudioSettingManager.AudioSetting(audioType, settingType, value);
        }

        public async Task ChargeAudioSource(string audioName,AudioState audioState,Action action = null)
        {
            await AudioInstanceManager.Instance.AudioEvent(audioName, audioState, action);
        }

        public async Task ChargeAudioSource(string audioName, AudioState audioState, float delay, Action action = null)
        {
            await AudioInstanceManager.Instance.AudioEvent(audioName, audioState, delay, action);
        }
        
        public async Task FastAudioSource(EasyAudioType easyAudioType,Transform pos = null,Action action = null)
        {
            var audioName = CreateAudioSource(easyAudioType, pos);
            await ChargeAudioSource(audioName, AudioState.Play, action);

            await CheckPlayEndDeleteToFast(audioName);
        }
        
        public Task<bool> CheckPlayEnd(string audioName)
        {
            return Task.FromResult(AudioInstanceManager.Instance.CheckIsPlayEnd(audioName));
        }
        
        private async Task CheckPlayEndDeleteToFast(string audioName)
        {
            while (!AudioInstanceManager.Instance.CheckIsPlayEnd(audioName))
            {
                await Task.Delay(100);
            }
            
            RemoveAudioSource(audioName);
        }
    }
}