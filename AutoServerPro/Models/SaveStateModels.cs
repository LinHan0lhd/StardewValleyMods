#nullable disable
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;

namespace AutoServerPro.Models
{
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

    [XmlRoot("ObjectState")]
    public class ObjectState
    {
        [XmlElement("TileX")] public float TileX;
        [XmlElement("TileY")] public float TileY;
        [XmlElement("ItemId")] public string ItemId;
        [XmlElement("ItemXml")] public string ItemXml;
    }

    [XmlRoot("FarmerPositionState")]
    public class FarmerPositionState
    {
        [XmlElement("FarmerId")] public long FarmerId;
        [XmlElement("X")] public float X;
        [XmlElement("Y")] public float Y;
    }

    [XmlRoot("MineState")]
    public class MineState
    {
        [XmlElement("MineLevel")] public int MineLevel;
        [XmlElement("ForceLayout")] public int? ForceLayout;
        [XmlArray("Objects")][XmlArrayItem("Object")] public List<ObjectState> Objects = new();
        [XmlArray("FarmerPositions")][XmlArrayItem("FarmerPosition")] public List<FarmerPositionState> FarmerPositions = new();
    }

    [XmlRoot("GameStateSnapshot")]
    public class GameStateSnapshot
    {
        [XmlElement("TimeOfDay")] public int TimeOfDay;
        [XmlArray("DebrisItems")][XmlArrayItem("DebrisItem")] public List<DebrisState> DebrisItems = new();
        [XmlElement("MineState")] public MineState Mine;
    }
}
