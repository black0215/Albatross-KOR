import sys
import locale

# Set UTF-8 encoding for Windows cmd
if sys.platform == 'win32':
    sys.stdout.reconfigure(encoding='utf-8')

def compare_binary(file1, file2, context=16, max_diffs=50):
    """Compare two binary files byte-by-byte and show differences."""
    
    with open(file1, 'rb') as f1, open(file2, 'rb') as f2:
        data1 = f1.read()
        data2 = f2.read()
    
    print(f"=" * 80)
    print(f"Binary Comparison: {file1} vs {file2}")
    print(f"=" * 80)
    print(f"File 1 Size: {len(data1):,} bytes")
    print(f"File 2 Size: {len(data2):,} bytes")
    print(f"Size Difference: {len(data2) - len(data1):+,} bytes")
    print()
    
    if len(data1) != len(data2):
        print(f"WARNING: Files have different sizes!")
        print()
    
    max_len = max(len(data1), len(data2))
    min_len = min(len(data1), len(data2))
    
    differences = []
    
    # Find all differences
    for i in range(max_len):
        b1 = data1[i] if i < len(data1) else None
        b2 = data2[i] if i < len(data2) else None
        
        if b1 != b2:
            differences.append(i)
    
    print(f"Total Differences: {len(differences):,} bytes ({len(differences) * 100.0 / max_len:.2f}%)")
    print()
    
    if len(differences) == 0:
        print("✓ Files are IDENTICAL!")
        return
    
    # Show first N differences with context
    shown = 0
    last_shown = -context * 2
    
    for diff_offset in differences:
        if shown >= max_diffs:
            remaining = len(differences) - shown
            print(f"\n... ({remaining:,} more differences not shown)")
            break
        
        # Skip if too close to previous
        if diff_offset - last_shown < context and shown > 0:
            continue
        
        shown += 1
        last_shown = diff_offset
        
        start = max(0, diff_offset - context)
        end = min(max_len, diff_offset + context)
        
        print(f"\n[Difference #{shown} at offset 0x{diff_offset:08X} ({diff_offset:,})]")
        
        # Show hex dump
        hex1 = data1[start:end].hex(' ', 1) if diff_offset < len(data1) else "(EOF)"
        hex2 = data2[start:end].hex(' ', 1) if diff_offset < len(data2) else "(EOF)"
        
        print(f"  File1: {hex1}")
        print(f"  File2: {hex2}")
        
        # Show ASCII representation
        def to_ascii(data, offset, length):
            result = ""
            for i in range(length):
                idx = offset + i
                if idx >= len(data):
                    result += "."
                else:
                    b = data[idx]
                    result += chr(b) if 32 <= b < 127 else "."
            return result
        
        ascii1 = to_ascii(data1, start, end - start)
        ascii2 = to_ascii(data2, start, end - start)
        
        print(f"  ASCII1: {ascii1}")
        print(f"  ASCII2: {ascii2}")
    
    print(f"\n" + "=" * 80)
    print(f"Summary: {len(differences):,} differences found")
    print("=" * 80)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python compare_cfg_binary.py <file1> <file2>")
        sys.exit(1)
    
    compare_binary(sys.argv[1], sys.argv[2])
