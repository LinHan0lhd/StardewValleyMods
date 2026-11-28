#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Netcode;
using StardewValley.Buildings;

namespace AutoServerPro.Core;

/// <summary>
/// 只改变发送内容 不删除服务器上的真实小屋
/// </summary>
internal static class CabinCollectionPatches
{
    private static readonly FieldInfo ElementsField = AccessToolsField(typeof(NetCollection<Building>), "elements");
    private static readonly FieldInfo ChangesField;
    private static readonly FieldInfo ReassignsField;
    private static readonly FieldInfo ChangeKeyField;
    private static readonly FieldInfo ChangeFieldField;
    private static readonly FieldInfo ChangeRemovalField;
    private static readonly FieldInfo ChangeReassignedField;
    private static readonly FieldInfo IncomingChangesField;
    private static readonly FieldInfo IncomingKeyField;
    private static readonly FieldInfo IncomingFieldField;
    private static readonly FieldInfo IncomingRemovalField;

    static CabinCollectionPatches()
    {
        Type elementsType = typeof(NetGuidDictionary<Building, NetRef<Building>>);
        ChangesField = AccessToolsField(elementsType, "outgoingChanges");
        ReassignsField = AccessToolsField(elementsType, "dictReassigns");
        Type changeType = ChangesField.FieldType.GetGenericArguments()[0];
        ChangeKeyField = AccessToolsField(changeType, "Key");
        ChangeFieldField = AccessToolsField(changeType, "Field");
        ChangeRemovalField = AccessToolsField(changeType, "Removal");
        ChangeReassignedField = AccessToolsField(changeType, "Reassigned");

        IncomingChangesField = AccessToolsField(elementsType, "incomingChanges");
        Type incomingType = IncomingChangesField.FieldType.GetGenericArguments()[0];
        IncomingKeyField = AccessToolsField(incomingType, "Key");
        IncomingFieldField = AccessToolsField(incomingType, "Field");
        IncomingRemovalField = AccessToolsField(incomingType, "Removal");
    }

    internal static bool RemoveIncomingCabins(NetCollection<Building> collection, HashSet<Guid> cabinKeys)
    {
        bool removedAny = false;
        var elements = GetElements(collection);
        var incoming = (IList)IncomingChangesField.GetValue(elements);
        foreach (object change in incoming.Cast<object>().ToList())
        {
            bool removal = (bool)IncomingRemovalField.GetValue(change);
            Guid key = (Guid)IncomingKeyField.GetValue(change);
            Building building = removal ? null : ((NetRef<Building>)IncomingFieldField.GetValue(change)).Value;
            if ((removal && cabinKeys.Contains(key)) || (!removal && building?.buildingType.Value == "Cabin"))
            {
                incoming.Remove(change);
                removedAny = true;
            }
        }
        return removedAny;
    }

    public static bool WriteFullPrefix(NetCollection<Building> __instance, BinaryWriter writer)
    {
        if (!CabinSyncManager.FilterActive) return true;

        var elements = GetElements(__instance);
        var fields = elements.FieldDict;
        var reassigns = GetReassigns(elements);
        var visible = fields.Where(pair => CabinSyncManager.IncludeInSync(pair.Value.Value)).ToList();

        writer.Write(visible.Count);
        foreach (var pair in visible)
        {
            writer.WriteGuid(pair.Key);
            reassigns[pair.Key].Write(writer);
            pair.Value.NetFields.WriteFull(writer);
        }
        return false;
    }

    public static bool WritePrefix(NetCollection<Building> __instance, BinaryWriter writer)
    {
        if (!CabinSyncManager.FilterActive) return true;

        var elements = GetElements(__instance);
        var reassigns = GetReassigns(elements);
        var changes = ((IEnumerable)ChangesField.GetValue(elements)).Cast<object>()
            .Where(change => CabinSyncManager.IncludeInSync(GetBuilding(change))).ToList();

        writer.Write7BitEncoded((uint)changes.Count);
        foreach (object change in changes)
        {
            bool removal = (bool)ChangeRemovalField.GetValue(change);
            writer.Write(removal ? (byte)1 : (byte)0);
            writer.WriteGuid((Guid)ChangeKeyField.GetValue(change));
            ((NetVersion)ChangeReassignedField.GetValue(change)).Write(writer);
            if (!removal)
                GetField(change).NetFields.WriteFull(writer);
        }

        var updates = new List<UpdateEntry>();
        foreach (var pair in elements.FieldDict)
        {
            if (pair.Value.NetFields.Dirty && CabinSyncManager.IncludeInSync(pair.Value.Value))
                updates.Add(new UpdateEntry(pair.Key, pair.Value, reassigns[pair.Key]));
        }
        foreach (object change in changes)
        {
            var field = GetField(change);
            if ((bool)ChangeRemovalField.GetValue(change) && field.NetFields.Dirty)
            {
                updates.Add(new UpdateEntry(
                    (Guid)ChangeKeyField.GetValue(change),
                    field,
                    (NetVersion)ChangeReassignedField.GetValue(change)));
            }
        }

        writer.Write7BitEncoded((uint)updates.Count);
        foreach (var update in updates)
        {
            writer.WriteGuid(update.Key);
            update.Reassigned.Write(writer);
            writer.WriteSkippable(() => update.Field.NetFields.Write(writer));
        }
        return false;
    }

    private static NetGuidDictionary<Building, NetRef<Building>> GetElements(NetCollection<Building> collection)
    {
        return (NetGuidDictionary<Building, NetRef<Building>>)ElementsField.GetValue(collection);
    }

    private static Dictionary<Guid, NetVersion> GetReassigns(object elements)
    {
        return (Dictionary<Guid, NetVersion>)ReassignsField.GetValue(elements);
    }

    private static NetRef<Building> GetField(object change)
    {
        return (NetRef<Building>)ChangeFieldField.GetValue(change);
    }

    private static Building GetBuilding(object change)
    {
        return GetField(change).Value;
    }

    private static FieldInfo AccessToolsField(Type type, string name)
    {
        for (Type current = type; current != null; current = current.BaseType)
        {
            var field = current.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null) return field;
        }
        throw new MissingFieldException(type.FullName, name);
    }

    private sealed class UpdateEntry
    {
        public readonly Guid Key;
        public readonly NetRef<Building> Field;
        public readonly NetVersion Reassigned;

        public UpdateEntry(Guid key, NetRef<Building> field, NetVersion reassigned)
        {
            Key = key;
            Field = field;
            Reassigned = reassigned;
        }
    }
}
