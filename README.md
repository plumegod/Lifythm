# Lifythm
音乐节奏游戏《生音》的关键代码文本，本游戏使用插件 Koreography 

本代码使用了自建音乐资源控件和描边系统，集中体现在引用EasyAudioSystem、Outline类上。

各文件对应功能：
AudioEffectManager - 各个敲击物品控制脚本
AudioGameController - 主要游戏控制脚本，控制游戏关卡开始、播放、校准、引导和重置功能。
LaneController - 控制单条声音节奏线，并检测生成，含对象池
NoteObject - 单个节奏点生成控制器
RhythmScoreManager - 分数控制系统和背景切换功能
