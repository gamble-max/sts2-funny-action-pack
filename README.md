# 尖塔整活动作拓展包 (Spire Funny Action Pack) — `v1.0.0`

Native **Spine 4.2** bone-driven funny emote & dance pack with particle VFX for all **6 Slay the Spire 2 playable characters** (`Ironclad`, `Silent`, `Defect`, `Necrobinder + Osty`, `Regent + Minions & Swords`, and `Fool`). Works **standalone** via hotkeys, or pairs with **[Co-op QoL Suite](https://github.com/gamble-max/sts2-coop-qol-suite)** for one-click toolbar buttons in the Chat/Quick tabs and real-time Steam lobby multiplayer sync!

《杀戮尖塔 2》全六大原生角色专属搞笑动作拓展包（原生 Spine 4.2 骨骼驱动 + 轻量粒子特效）。支持**单人独立使用**，搭配 **Co-op QoL Suite（联机功能优化聚合包）** 时可在「弹幕栏 / 快捷栏」一键点击触发并实现 Steam 联机队友实时同步观看！

## 动作列表与快捷键 / Actions & Hotkeys (`6 × 4 = 24` 套定制骨骼动作)

| 动作 / Action | 快捷键 / Hotkey | 效果说明 / Description |
| :--- | :---: | :--- |
| **高抬腿 (`High-Knees`)** | `J` | 开关式持续原地高抬腿 + 脚底扬尘与汗珠特效；亡灵契约师触发时奥斯蒂同步踩点摆手 |
| **奶蛙捧腹大笑 (`Belly Laugh`)** | `K` | 三段式魔性动作：仰天捂肚大笑 → 抱头抖肩 → 仰倒打滚蹬腿，附带爆笑泪花特效 |
| **玉兔公主舞 (`Jade Rabbit Dance`)** | `L` | 优雅 S 型身段摇曳、兰花指/袖摆舞动、单腿轻点与金色花瓣星尘环绕 |
| **挑衅动作 (`Taunt`)** | `U` | 欠揍拍屁股 + 勾手鬼脸挑衅循环，附带怒气挑衅火星 |

> 攻击、施法、受击动画期间会自动平滑退避让位，阵亡或战斗结束自动关闭。

## 安装方法 / Installation

1. 前往本仓库 [Releases](../../releases) 下载最新的 `Sts2ActionPack-v1.0.0.zip`。
2. 解压到《杀戮尖塔 2》安装目录下的 `mods/` 文件夹中，使目录结构为：
   `<Slay the Spire 2>/mods/Sts2ActionPack/Sts2ActionPack.dll` 与 `Sts2ActionPack.json`。
3. （可选）同时安装 `CoopQolSuite` 可获得顶栏动作按钮与联机动作同步。

## 目录结构 / Directory Structure

- `manifest.json` — Mod 清单（`id: "Sts2ActionPack"`, `version: "1.0.0"`, `affects_gameplay: false`）
- `src/Core/` — 核心状态机、Spine 骨骼 delta 缓存、粒子特效、`CoopQolSuite` 软桥接
- `src/Rigs/` — 6 大原生角色 + 奥斯蒂/随从专属 Spine 骨骼驱动
- `build/` — 独立零前置构建脚本（`build.mjs` + `Shared/`）

## 从源码构建 / Build from Source

需要 **Node.js (v18+)** 与 **Visual Studio 2022 / BuildTools**（含 Roslyn `csc.exe`），以及本地《杀戮尖塔 2》游戏目录（可通过环境变量 `STS2_DIR` 指定）：

```powershell
node build/build.mjs
# 构建并直接部署到游戏目录：
node build/build.mjs --deploy
```
