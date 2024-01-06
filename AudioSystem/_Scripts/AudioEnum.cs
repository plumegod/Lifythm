namespace AudioSystem._Scripts
{

    //播放触发事件类型
    public enum AudioState
    {
        Play,
        Pause,
        Stop,
        UnPause,
        Mute,
        UnMute,
        PlayDelayed,
        PlayScheduled
    }
    
    //音频设置类型
    public enum AudioSettingType
    {
        Volume,
        Pitch,
        Loop,
        SpatialBlend,
        Clip,
        Group,
    }
}