# 2D Sandbox System Specification

This document summarizes the technical specifications and features of the 2D sandbox system implemented in Unity.

## Core Features

### 1. 2D Tilemap システム
- **Ground Tilemap**: 地形（土ブロック）用。衝突判定（TilemapCollider2D）付き。
- **Background Tilemap**: 背景（壁ブロック）用。
- **WorldGenerator**:
    - Perlin Noise を使用した地形生成。
    - 幅100、高さ50のマスの範囲で地形を生成。
    - `Setup/Run Initial Setup` メニューから即座に再生成可能。

### 2. PlayerController
- **物理挙動**: `Rigidbody2D` を使用。重力スケールを調整し、テラリアのような少しフワッとしたジャンプを実現。
- **操作**: A/D または矢印キーで左右移動、Space キーでジャンプ。
- **接地判定**: 足元の `GroundCheck` オブジェクトによる円形判定。

### 3. BlockInteraction
- **左クリック**: プレイヤーの周囲5ブロック以内の「Ground」タイルを破壊。
- **右クリック**: プレイヤーの周囲5ブロック以内の空地に「DirtTile」を設置。

## Technical Details
- **Scripts**:
  - `WorldGenerator.cs`: Handles startup terrain generation.
  - `PlayerController.cs`: Handles input and physics for the player.
  - `BlockInteraction.cs`: Handles mouse interaction with the Tilemap.
- **Prefabs**:
  - `Player`: Basic character setup with required components.
