using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem._Scripts.Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "newAudioType",menuName = "AudioSystem/Audio Data/Audio Type")]
    public class EasyAudioType : ScriptableObject
    {
        [HideInInspector] public AudioSource source;
        [HideInInspector] public GameObject sourceObject;
        
        public string audioAddressName;

        [Header("音频文件")]public AudioClip clip;
        [Header("混音")]public AudioMixerGroup group;

        [Header("音量大小")][Range(0f, 1f)] public float volume = 0.5f;
        [Header("播放速度")][Range(0.1f, 2f)] public float pitch = 1f;
        [Header("3D播放模式")][Range(0f, 1f)] public float spatialBlend = 0f;
        [Header("是否循环")] public bool loop = false;
        [Header("是否自动删除")] public bool autoDelete = true;
    }
}
