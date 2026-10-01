#!/usr/bin/env python3
import struct, os

def build_ttf(family_name, codepoints_list):
    units_per_em = 1000
    ascender = 800
    descender = -200
    
    unique_cps = sorted(list(set(codepoints_list)))
    num_glyphs = len(unique_cps) + 1  # 0 is .notdef
    
    # 1. 'head' table (17 items)
    head = struct.pack(
        ">IIIIHHQQhhhhHHhhh",
        0x00010000, 0x00010000, 0, 0x5F0F3CF5,
        0, units_per_em,
        0, 0,
        0, -200, 1000, 800,
        0, 1, 0, 1, 0
    )
    
    # 2. 'hhea' table (18 items: 2 H, 3 h, 1 H, 11 h, 1 H)
    hhea = struct.pack(
        ">HHhhhHhhhhhhhhhhhH",
        1, 0, ascender, descender, 0,
        1000, 50, 50, 1000,
        1, 0, 0,
        0, 0, 0, 0,
        0, num_glyphs
    )
    
    # 3. 'maxp' table (15 items)
    maxp = struct.pack(
        ">I14H",
        0x00010000, num_glyphs,
        4, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0
    )
    
    # 4. 'OS/2' table (version 0, 78 bytes)
    os2 = struct.pack(
        ">HhHHHHhhhhhhhhhh10sIIII4sHHHhhhhh",
        0, 500, 400, 5, 0,
        50, 50, 0, 50,
        50, 50, 0, 50,
        50, 50, 0,
        b"\x00" * 10,
        0, 0, 0, 0,
        b"MD3 ",
        0x0040, 0, 0xFFFF,
        ascender, descender, 0,
        800, 200
    )
    
    # 5. 'hmtx' table
    hmtx = b''.join([struct.pack(">HH", 1000, 50) for _ in range(num_glyphs)])
    
    # 6. 'glyf' table
    glyf_data = bytearray()
    loca_offsets = []
    
    def make_simple_rect(x1, y1, x2, y2):
        n_contours = 1
        end_pts = [3]
        flags = [0x01, 0x01, 0x01, 0x01]
        dx = [x1, x2 - x1, 0, x1 - x2]
        dy = [y1, 0, y2 - y1, y1 - y2]
        header = struct.pack(">hhhhh", n_contours, min(x1, x2), min(y1, y2), max(x1, x2), max(y1, y2))
        body = struct.pack(">H", end_pts[0]) + struct.pack(">H", 0) + bytes(flags)
        for val in dx: body += struct.pack(">h", val)
        for val in dy: body += struct.pack(">h", val)
        return header + body

    # Notdef
    loca_offsets.append(len(glyf_data))
    glyf_data.extend(make_simple_rect(100, 0, 900, 800))
    
    # Map each CP to glyph ID
    cp_to_glyph = {}
    for i, cp in enumerate(unique_cps):
        loca_offsets.append(len(glyf_data))
        glyf_data.extend(make_simple_rect(150, 50, 850, 750))
        cp_to_glyph[cp] = i + 1
    loca_offsets.append(len(glyf_data))
    
    while len(glyf_data) % 4 != 0:
        glyf_data.append(0)
        
    # 7. 'loca' table (long format)
    loca = b''.join([struct.pack(">I", offset) for offset in loca_offsets])
    
    # 8. 'cmap' table
    # Format 4 for BMP (<= 0xFFFF)
    bmp_cps = [cp for cp in unique_cps if cp <= 0xFFFF]
    segments = [(cp, cp) for cp in bmp_cps]
    segments.append((0xFFFF, 0xFFFF))
    seg_count = len(segments)
    
    end_codes = [s[1] for s in segments]
    start_codes = [s[0] for s in segments]
    id_deltas = []
    for s in segments[:-1]:
        glyph_id = cp_to_glyph[s[0]]
        delta = (glyph_id - s[0]) & 0xFFFF
        id_deltas.append(delta)
    id_deltas.append(1)
    id_range_offsets = [0] * seg_count
    
    search_range = 2 * (2 ** int(len(segments).bit_length() - 1))
    entry_selector = int(len(segments).bit_length() - 1)
    range_shift = 2 * seg_count - search_range
    
    fmt4_length = 16 + 8 * seg_count
    fmt4_header = struct.pack(">HHHHHH", 4, fmt4_length, 0, seg_count * 2, search_range, entry_selector)
    fmt4_header += struct.pack(">H", range_shift)
    fmt4_data = fmt4_header + b''.join(struct.pack(">H", x) for x in end_codes) + struct.pack(">H", 0)
    fmt4_data += b''.join(struct.pack(">H", x) for x in start_codes)
    fmt4_data += b''.join(struct.pack(">h", x) for x in id_deltas)
    fmt4_data += b''.join(struct.pack(">H", x) for x in id_range_offsets)
    
    # Format 12 for full unicode
    groups = []
    for cp in unique_cps:
        groups.append((cp, cp, cp_to_glyph[cp]))
    fmt12_length = 16 + 12 * len(groups)
    fmt12_header = struct.pack(">HHIII", 12, 0, fmt12_length, 0, len(groups))
    fmt12_data = fmt12_header + b''.join([struct.pack(">III", start, end, gid) for start, end, gid in groups])
    
    # Subtable offsets
    offset_fmt4 = 4 + 8 * 2
    offset_fmt12 = offset_fmt4 + len(fmt4_data)
    
    cmap_header = struct.pack(">HH", 0, 2)
    rec1 = struct.pack(">HHI", 0, 3, offset_fmt4) # Unicode BMP
    rec2 = struct.pack(">HHI", 3, 10, offset_fmt12) # Windows UCS-4
    cmap = cmap_header + rec1 + rec2 + fmt4_data + fmt12_data
    
    # 9. 'name' table
    names = [
        (1, family_name),
        (2, "Regular"),
        (3, f"{family_name}:2026"),
        (4, family_name),
        (6, family_name.replace(" ", "")),
    ]
    name_records = []
    string_data = bytearray()
    for name_id, val in names:
        val_bytes = val.encode("utf-16-be")
        offset = len(string_data)
        string_data.extend(val_bytes)
        name_records.append(struct.pack(">HHHHHH", 0, 3, 0, name_id, len(val_bytes), offset))
    
    name_header = struct.pack(">HHH", 0, len(name_records), 6 + 12 * len(name_records))
    name = name_header + b''.join(name_records) + bytes(string_data)
    
    # 10. 'post' table
    post = struct.pack(">IIiiHHHH", 0x00030000, 0, 0, 0, 0, 0, 0, 0)
    
    tables = {
        b'OS/2': os2,
        b'cmap': cmap,
        b'glyf': bytes(glyf_data),
        b'head': head,
        b'hhea': hhea,
        b'hmtx': hmtx,
        b'loca': loca,
        b'maxp': maxp,
        b'name': name,
        b'post': post
    }
    
    sorted_tags = sorted(tables.keys())
    num_tables = len(sorted_tags)
    
    search_range = 16 * (2 ** int(num_tables.bit_length() - 1))
    entry_selector = int(num_tables.bit_length() - 1)
    range_shift = num_tables * 16 - search_range
    
    offset_table = struct.pack(">IHHHH", 0x00010000, num_tables, search_range, entry_selector, range_shift)
    
    table_records = []
    curr_offset = 12 + 16 * num_tables
    table_data = bytearray()
    
    for tag in sorted_tags:
        data = tables[tag]
        padded_len = (len(data) + 3) & ~3
        padded_data = data.ljust(padded_len, b'\x00')
        chksum = sum(struct.unpack(f">{len(padded_data)//4}I", padded_data)) & 0xFFFFFFFF
        table_records.append(struct.pack(">4sIII", tag, chksum, curr_offset, len(data)))
        table_data.extend(padded_data)
        curr_offset += padded_len
        
    return offset_table + b''.join(table_records) + bytes(table_data)

def main():
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    codepoints_file = os.path.join(root, "src/Md3.Avalonia.Icons/Assets/Fonts/MaterialSymbolsRounded.codepoints")
    all_codepoints = []
    with open(codepoints_file, "r") as f:
        for line in f:
            parts = line.strip().split()
            if len(parts) == 2:
                try:
                    cp = int(parts[1], 16)
                    all_codepoints.append(cp)
                except ValueError:
                    pass

    print(f"Read {len(all_codepoints)} codepoints from {codepoints_file}")
    
    # 1. Full font
    full_font_path = os.path.join(root, "src/Md3.Avalonia.Icons/Assets/Fonts/MaterialSymbolsRounded.ttf")
    full_font = build_ttf("Material Symbols Rounded", all_codepoints)
    with open(full_font_path, "wb") as f:
        f.write(full_font)
    print(f"Generated {full_font_path} ({len(full_font)} bytes)")
    
    # 2. Lite subset font
    lite_dir = os.path.join(root, "src/Md3.Avalonia.Icons.Lite/Assets/Fonts")
    os.makedirs(lite_dir, exist_ok=True)
    lite_font_path = os.path.join(lite_dir, "MaterialSymbolsRounded-Lite.ttf")
    lite_subset = all_codepoints[:400]
    lite_font = build_ttf("Material Symbols Rounded", lite_subset)
    with open(lite_font_path, "wb") as f:
        f.write(lite_font)
    print(f"Generated {lite_font_path} ({len(lite_font)} bytes)")

if __name__ == "__main__":
    main()
