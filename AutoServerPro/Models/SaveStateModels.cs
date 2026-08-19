#nullable disable
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;

namespace AutoServerPro.Models;

[XmlRoot("Items")]
public class ItemsWrapper
{
    [XmlElement("Item")]
    public Item[] Items { get; set; }
}

[XmlRoot("ChunkState")]
public class ChunkState
{
    [XmlElement("X")] public float X;
    [XmlElement("Y")] public float Y;
    [XmlElement("RandomOffset")] public int RandomOffset;
    [XmlElement("XSpriteSheet")] public int XSpriteSheet;
    [XmlElement("YSpriteSheet")] public int YSpriteSheet;
    [XmlElement("Scale")] public float Scale;
    [XmlElement("Alpha")] public float Alpha;
    [XmlElement("Rotation")] public float Rotation;
    [XmlElement("RotationVelocity")] public float RotationVelocity;
    [XmlElement("HitWall")] public bool HitWall;
    [XmlElement("Bob")] public float Bob;
    [XmlElement("Bounces")] public int Bounces;
}

[XmlRoot("DebrisState")]
public class DebrisState
{
    [XmlElement("LocationName")] public string LocationName;
    [XmlElement("ItemId")] public string ItemId;
    [XmlElement("ItemXml")] public string ItemXml;
    [XmlElement("Stack")] public int Stack;
    [XmlElement("Quality")] public int Quality;
    [XmlElement("DebrisType")] public int DebrisType;
    [XmlElement("ChunkType")] public int ChunkType;
    [XmlElement("ChunkFinalYLevel")] public int ChunkFinalYLevel;
    [XmlElement("FloppingFish")] public bool FloppingFish;
    [XmlElement("Scale")] public float Scale;
    [XmlElement("ItemQuality")] public int ItemQuality;
    [XmlElement("ChunksColorR")] public int ChunksColorR;
    [XmlElement("ChunksColorG")] public int ChunksColorG;
    [XmlElement("ChunksColorB")] public int ChunksColorB;
    [XmlElement("ChunksColorA")] public int ChunksColorA;
    [XmlElement("NonSpriteColorR")] public int NonSpriteColorR;
    [XmlElement("NonSpriteColorG")] public int NonSpriteColorG;
    [XmlElement("NonSpriteColorB")] public int NonSpriteColorB;
    [XmlElement("NonSpriteColorA")] public int NonSpriteColorA;
    [XmlElement("SpriteChunkSheetName")] public string SpriteChunkSheetName;
    [XmlElement("SizeOfSourceRectSquares")] public int SizeOfSourceRectSquares;
    [XmlElement("DebrisMessage")] public string DebrisMessage;
    [XmlElement("IsSinking")] public bool IsSinking;
    [XmlElement("ChunksMoveTowardPlayer")] public bool ChunksMoveTowardPlayer;
    [XmlElement("TimeSinceDoneBouncing")] public float TimeSinceDoneBouncing;
    [XmlArray("Chunks")][XmlArrayItem("Chunk")] public List<ChunkState> Chunks = new();
}

[XmlRoot("NpcPositionData")]
public class NpcPositionData
{
    [XmlElement("Name")] public string Name { get; set; }
    [XmlElement("MapName")] public string MapName { get; set; }
    [XmlElement("TileX")] public float TileX { get; set; }
    [XmlElement("TileY")] public float TileY { get; set; }
    [XmlElement("Facing")] public int Facing { get; set; }
}

[XmlRoot("GameStateSnapshot")]
public class GameStateSnapshot
{
    [XmlElement("TimeOfDay")] public int TimeOfDay;
    [XmlArray("DebrisItems")][XmlArrayItem("DebrisItem")] public List<DebrisState> DebrisItems = new();
    [XmlArray("NpcPositions")][XmlArrayItem("NpcPosition")] public List<NpcPositionData> NpcPositions = new();
}
