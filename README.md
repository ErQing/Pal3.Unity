<p align="center">
  <img width="128" align="center" src="Assets/Resources/UI/game-icon.png">
</p>
<h1 align="center">
  Pal3.Unity
</h1>
<p align="center">
  仙剑奇侠传三（以及外传）C#/Unity实现
</p>
<p align="center">
  <a style="text-decoration:none">
    <img src="https://img.shields.io/badge/unity-2021.2.13f1-blue?style=flat-square" alt="Unity Version" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/badge/platform-Linux%20%7C%20Win%20%7C%20Mac%20%7C%20iOS%20%7C%20Android-orange?style=flat-square" alt="Platform" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/badge/license-GPL--3.0-green?style=flat-square" alt="License" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/repo-size/jasonstein/pal3.unity?style=flat-square" alt="Size" />
  </a>
</p>

# 简介
这是一个使用C#/Unity重新实现的仙剑奇侠传三以及仙剑奇侠传三外传的开源项目。仙剑奇侠传三以及仙剑奇侠传三外传属于原上海软星作品，版权属于软星科技以及大宇公司所有，本项目不包含任何仙剑奇侠传三以及仙剑奇侠传三外传的游戏本体数据。本项目的实现方式为运行时读取仙剑奇侠传三原始游戏数据实现，具体实现方法请见源代码。本项目遵循GPL-3.0协议，但仅限于此项目的代码，任何与仙剑奇侠传三或者外传有关的图片，音视频，游戏数据均不在此范围，任何未经版权方许可的情况下使用仙剑奇侠传三或者外传游戏数据进行商业行为都是违法的。

## 为什么要做这个？
学了几周Unity，想找项目练手，一开始自己尝试做了一下新仙剑的第一关，大概了解了RPG游戏的制作流程，后因机缘巧合发现了zby大佬的[PAL3patch](https://github.com/zhangboyang/PAL3patch)项目以及dontpanic92大佬的[OpenPAL3](https://github.com/dontpanic92/OpenPAL3)项目，所以给技术上实现仙三复刻带来了可能。至于为什么选择C#/Unity？选择Unity的原因有两点，第一是Unity对全平台全端的打包做的很好很方便，第二个是Unity提供Mac上arm64原生IDE支持（2021.2.X），我大概有一半时间是在Mac上的，所以比较在意这点。其实本项目的实现几乎没有用到Unity引擎的大部分功能，特别是编辑器功能，因为几乎是100%纯代码实现的，所以其实理论上不需要花太大的代价就可以把本项目移植到其他支持C#脚本的引擎中。

## 如何运行项目
使用Unity2021.2.X打开当前项目文件夹即可，具体当前项目所需要的Unity版本请查看上面的Badge。为什么要选择Unity2021？主要是因为目前Unity官方只提供了2021的Apple Silicon版本，再一个是2021开始Unity可以选择.NET Standard 2.1作为API接口。另外Unity2021LTS版本今年很快也会公布，届时我会把项目锁在2021LTS上。

因为项目本身不含有仙剑奇侠传三或仙剑奇侠传三外传的游戏数据，所以你需要持有一份仙剑三或外传的游戏原始文件（Steam或者方块游戏获得皆可，注意：暂时仅支持简体版游戏）。
- 第一次打开Unity项目之后，先双击选择Scenes\Game作为当前场景，然后点播放键即可。如果选择Scenes\ResourceViewer，则会打开游戏资源查看器。
- 第一次打开的时候会自动弹出文件夹选择窗口，请选择当前电脑上仙剑奇侠传三（或者外传）的安装文件夹即可。

## 如何在手持设备上运行
- 在打包后的运行时，所有平台都默认使用Application.persistentDataPath目录读取仙剑三文件，具体这个目录在哪里，根据平台决定，请阅读Unity文档：[Application.persistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html)
- 第一次安装游戏到手持设备后，请先运行一次，然后将手持设备连接电脑，拷贝整个仙剑奇侠三（或者外传）原游戏目录到游戏App目录下即可，如果是仙剑三，文件夹的名必须为PAL3，如果是仙剑三外传，文件夹名必须为PAL3A。
- iOS设备请使用iTunes将文件夹共享给游戏应用。

## 如何切换仙剑奇侠传三/仙剑奇侠传三外传
版本的切换是靠Define symbol实现的，PAL3对应仙剑奇侠传三，PAL3A对应仙剑奇侠传三外传。我在Unity编辑器菜单栏里面加了一个快捷按钮可以一键切换（菜单栏->Pal3->Switch Variant->PAL3A），切换之后对应的游戏文件夹也自动的从PAL3转换成PAL3A。
注意：当前仙剑三外传的完成度还处于非常早期，游戏剧情只能推进一小段。仙剑三的完成度很高，主线剧情已经全部完成。

## 截图
![PAL3 Mac](Screenshots/PAL3_Mac.png?raw=true)
![PAL3 iOS](Screenshots/PAL3_iOS.png?raw=true)

## iOS演示视频
https://www.bilibili.com/video/BV1Fu411R7jM

## 按键以及操作
- 鼠标键盘：鼠标左键点击操作人物行走方向（键盘方向键也可以），AD控制镜头旋转，空格与周边附近的物品或者NPC交互。M键打开大地图，U键打开剧情选择菜单。
- 手柄：左摇杆控制人物行走，右摇杆控制镜头旋转，A键交互，菜单和选择按钮对应大地图和剧情选择菜单。
- 触屏：仅在有触摸屏的手持设备上才会启用，虚拟摇杆控制行走，交互键互动。

## 项目进度以及路线图
仙剑奇侠传三的剧情部分已经全部完成，游戏可以完整的从头玩到尾体验一遍剧情，也可以使用游戏内提供的剧情选择菜单跳转至预设好的剧情时间点。游戏中的迷宫机关，战斗，物品系统，小游戏以及其他的子系统尚未完成，大部分特效也尚未实现，未来的重心将是优先实现迷宫机关，然后实现特效，最后是战斗系统以及相关联的各种子系统。另外主菜单和大部分界面也需要重新设计和实现（毕竟现在还要做手机端的适配）

## 如何贡献？
因为项目还处于早期实现过程中，很多系统还没有实现，暂时不接受比较大的Pull request，特别是feature类型，如果您有好的想法，意见或者发现了Bug请欢迎提交issue或者加入交流群与我讨论。

另外您还可以参考这个项目解析视频：https://www.bilibili.com/video/BV1Pr4y167sF

## 技术交流以及测试
请加入QQ群：252315306
