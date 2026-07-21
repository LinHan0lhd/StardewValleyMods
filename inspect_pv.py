#!/usr/bin/env python3
"""从 StardewValley.dll 里找 xTile.ObjectModel.PropertyValue 的字段名。"""
import re

DLL = "/workspace/StardewValley/Stardew Valley.dll"

with open(DLL, "rb") as f:
    data = f.read()

# .NET 元数据里字段名是 UTF-8 字符串，存储在 #Strings 堆
# PropertyValue 类的字段名通常是 _type/_value 或 type/value 或 m_type/m_value
# 先找 PropertyValue 类定义的特征

# 找所有 "PropertyValue" 出现位置
print("=== PropertyValue 出现位置 ===")
positions = []
idx = 0
while True:
    idx = data.find(b'PropertyValue', idx)
    if idx < 0:
        break
    positions.append(idx)
    idx += 1
print(f"共 {len(positions)} 处")

# 找可能的字段名（.NET 元数据 #Strings 堆里的字段名通常是短字符串）
# 找所有像是字段名的字符串
field_candidates = set()
for m in re.finditer(rb'[\x01-\x7e]{2,40}', data):
    s = m.group()
    # 过滤：字段名通常只含字母数字下划线
    try:
        decoded = s.decode('ascii')
    except:
        continue
    if not decoded.replace('_', '').isalnum():
        continue
    if decoded.lower() in ('type', 'value', 'key', 'data', 'inner', 'raw',
                           '_type', '_value', '_key', '_data',
                           'm_type', 'm_value', 'm_key', 'm_data',
                           'propertytype', 'propertyvalue',
                           'isbool', 'isint', 'isfloat', 'isstring',
                           'boolvalue', 'intvalue', 'floatvalue', 'stringvalue',
                           '_boolvalue', '_intvalue', '_floatvalue', '_stringvalue',
                           'kind', 'discriminator', 'tag', 'case'):
        field_candidates.add(decoded)

print("\n=== 候选字段名 ===")
for s in sorted(field_candidates):
    print(" ", s)

# 找 TryGetValue 方法的字符串特征
print("\n=== TryGetValue 相关字符串 ===")
for m in re.finditer(rb'TryGetValue[A-Za-z`0-9]*', data):
    s = m.group().decode('ascii', errors='replace')
    print(" ", s)

# 找 implicit operator 特征（隐式转换）
print("\n=== implicit operator 相关 ===")
for m in re.finditer(rb'op_Implicit[^\x00]*', data):
    s = m.group()[:80].decode('ascii', errors='replace')
    print(" ", s)
