using System;
using System.Collections.Generic;
using AudioSystem._Scripts.Data;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem._Scripts
{
    public static class AudioSettingManager
    {
        #region 音频设置

        //重载音频设置
        public static void ReloadAudioData(string audioName,EasyAudioType typeData)
        {
            //从字典寻找音频
            EasyAudioType type = AudioInstanceManager.Instance.AudioDict[audioName];
            
            //todo:这段初始化失败，原因不明
            
            //初始化音频设置
            type.sourceObject.name = typeData.audioAddressName;
            type.sourceObject.GetComponent<AudioSource>().volume = typeData.volume;
            type.sourceObject.GetComponent<AudioSource>().pitch = typeData.pitch;
            type.sourceObject.GetComponent<AudioSource>().loop = typeData.loop;
            type.sourceObject.GetComponent<AudioSource>().spatialBlend = typeData.spatialBlend;

            type.sourceObject.GetComponent<AudioSource>().clip = typeData.clip;

            if (type.group != null)
            {
                type.sourceObject.GetComponent<AudioSource>().outputAudioMixerGroup = type.group;
            }
        }

        //音频设置委托
        private static Dictionary<AudioSettingType, Action<EasyAudioType, object>> audioSettingMap = new Dictionary<AudioSettingType, Action<EasyAudioType, object>>()
        {
            { AudioSettingType.Volume, (type, value) => type.volume = (float)value },
            { AudioSettingType.Pitch, (type, value) => type.pitch = (float)value },
            { AudioSettingType.SpatialBlend, (type, value) => type.spatialBlend = (float)value },
            { AudioSettingType.Loop, (type, value) => type.loop = (bool)value },
            { AudioSettingType.Clip, (type, value) => type.clip = (AudioClip)value },
            { AudioSettingType.Group, (type, value) => type.group = (AudioMixerGroup)value },
        };
        
        //设置单个音频选项
        public static void AudioSetting<T>(EasyAudioType type, AudioSettingType settingType, T value)
        {
            var audioSetting = audioSettingMap[settingType];
            audioSetting(type, value);
        }
        
        //查找字典中audioAddressName是否重名，是则重命名，并再次检查是否重名
        public static string CheckAudioName(string audioName)
        {
            int i = 0;
            var newAudioName = audioName;

            while (AudioInstanceManager.Instance.AudioDict.ContainsKey(newAudioName)) // 修改循环条件
            {
                i++;
                newAudioName = audioName + "_" + i;
            }

            return newAudioName;
        }

        #endregion
    }
}