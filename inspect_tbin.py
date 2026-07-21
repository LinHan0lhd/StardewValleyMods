#!/usr/bin/env python3
"""解析 tbin 文件，提取 tilesheet 的 ImageSource。
对比原版和导出版本的差异。"""
import struct
import sys

def read_string(data, offset):
    """读取 int32 长度 + UTF8 bytes 字符串，返回 (string, new_offset)"""
    length = struct.unpack_from('<i', data, offset)[0]
    offset += 4
    s = data[offset:offset+length].decode('utf-8', errors='replace')
    offset += length
    return s, offset

def read_properties(data, offset):
    """读取属性列表"""
    count = struct.unpack_from('<i', data, offset)[0]
    offset += 4
    props = {}
    for _ in range(count):
        key, offset = read_string(data, offset)
        ptype = data[offset]
        offset += 1
        if ptype == 0:  # bool
            val = data[offset]
            offset += 1
            props[key] = ('bool', val != 0)
        elif ptype == 1:  # int
            val = struct.unpack_from('<i', data, offset)[0]
            offset += 4
            props[key] = ('int', val)
        elif ptype == 2:  # float
            val = struct.unpack_from('<f', data, offset)[0]
            offset += 4
            props[key] = ('float', val)
        elif ptype == 3:  # string
            val, offset = read_string(data, offset)
            props[key] = ('string', val)
        else:
            props[key] = ('unknown', ptype)
            break
    return props, offset

def parse_tbin(path, label):
    print(f"\n========== {label}: {path} ==========")
    with open(path, 'rb') as f:
        data = f.read()

    offset = 0
    # Header "tBIN10"
    magic = data[0:6].decode('ascii', errors='replace')
    offset = 6
    print(f"Header: {magic}")

    # Map Id
    map_id, offset = read_string(data, offset)
    print(f"Map.Id: '{map_id}'")

    # Map Description
    map_desc, offset = read_string(data, offset)
    print(f"Map.Description: '{map_desc}'")

    # Map Properties
    map_props, offset = read_properties(data, offset)
    print(f"Map.Properties count: {len(map_props)}")
    for k, v in list(map_props.items())[:3]:
        print(f"  {k} = {v}")
    if len(map_props) > 3:
        print(f"  ... ({len(map_props)-3} more)")

    # TileSheets
    ts_count = struct.unpack_from('<i', data, offset)[0]
    offset += 4
    print(f"\nTileSheets count: {ts_count}")
    for i in range(ts_count):
        ts_id, offset = read_string(data, offset)
        ts_desc, offset = read_string(data, offset)
        ts_image, offset = read_string(data, offset)
        # SheetSize (x, y)
        sw, sh = struct.unpack_from('<ii', data, offset)
        offset += 8
        # TileSize (x, y)
        tw, th = struct.unpack_from('<ii', data, offset)
        offset += 8
        # Margin (x, y)
        mw, mh = struct.unpack_from('<ii', data, offset)
        offset += 8
        # Spacing (x, y)
        spw, sph = struct.unpack_from('<ii', data, offset)
        offset += 8
        # Properties
        ts_props, offset = read_properties(data, offset)
        print(f"  [{i}] Id='{ts_id}' Image='{ts_image}' SheetSize={sw}x{sh} TileSize={tw}x{th}")
        print(f"      Margin={mw}x{mh} Spacing={spw}x{sph} Props={len(ts_props)}")

if __name__ == '__main__':
    # 原版
    parse_tbin(r'C:\Users\admin\Desktop\1.6 xnb 拆包解包器\unpacked\Maps\ArchaeologyHouse.tbin', '原版(PE)')
    # 我们的导出
    parse_tbin(r'C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\CPXnbExporter\exported\unpacked\Maps\ArchaeologyHouse.tbin', '我们导出')
