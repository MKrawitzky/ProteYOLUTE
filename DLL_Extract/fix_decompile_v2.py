"""
Fix decompilation artifacts v2 - IDENTIFIER RENAMING ONLY
NO code removal, NO structural changes
Just rename invalid C# identifiers to valid ones
"""
import re
import glob
import os

BASE = r"C:\BDalSystemData\HyStar\LcPlugin\PrivateData\Bruker proteoElute\DLL_Extract\dnspy_baltic\BalticWpfControlLib"

def fix_file(filepath):
    with open(filepath, 'r', encoding='utf-8', errors='replace') as f:
        content = f.read()
    original = content

    # === IDENTIFIER RENAMING ONLY ===

    # 1. ClassName.<>o__NN.<>p__NN -> ClassName._co_NN._cp_NN  (callsite cache access)
    content = re.sub(r'(\w+)\.<>o__(\d+)\.<>p__(\d+)', r'\1._co_\2._cp_\3', content)

    # 2. Standalone <>o__NN (class/struct declarations) -> _co_NN
    content = re.sub(r'<>o__(\d+)', r'_co_\1', content)

    # 3. <>p__NN (field names) -> _cp_NN
    content = re.sub(r'<>p__(\d+)', r'_cp_\1', content)

    # 4. CS$<>8__locals -> _locals
    content = re.sub(r'CS\$<>8__locals(\d*)', r'_locals\1', content)

    # 5. <>4__this -> _cthis
    content = content.replace('<>4__this', '_cthis')

    # 6. <>8__locals -> _closureLocals
    content = re.sub(r'<>8__locals(\d*)', r'_closureLocals\1', content)

    # 7. <>c__DisplayClassNN_N -> _DC_NN_N
    content = re.sub(r'<>c__DisplayClass(\d+)_(\d+)', r'_DC_\1_\2', content)

    # 8. <>c (standalone) -> _CC
    content = re.sub(r'<>c(?=[^_a-zA-Z0-9])', '_CC', content)

    # 9. <MethodName>b__N_N -> _mb_MethodName_N_N (lambda)
    content = re.sub(r'<(\w+)>b__(\d+)_(\d+)', r'_mb_\1_\2_\3', content)

    # 10. <MethodName>g__Name|N -> _mg_MethodName_Name_N (local function)
    content = re.sub(r'<(\w+)>g__(\w+)\|(\d+)', r'_mg_\1_\2_\3', content)

    # 11. <>O (static delegate cache class) -> _SDC
    content = re.sub(r'<>O(?=[^a-zA-Z0-9_])', '_SDC', content)

    # 12. <>9__N_N (cached delegates) -> _cd_N_N
    content = re.sub(r'<>9__(\d+)_(\d+)', r'_cd_\1_\2', content)
    content = re.sub(r'<>9(?=[^_a-zA-Z0-9])', r'_cd9', content)

    # 13. <MethodName>d__N (async state machines) -> _sm_MethodName_N
    content = re.sub(r'<(\w+)>d__(\d+)', r'_sm_\1_\2', content)

    # 14. <>F{00000200} (special delegate type) -> _DF200
    content = content.replace('<>F{00000200}', '_DF200')

    # 15. <0>__MethodName (cached method ref) -> _cm_MethodName
    content = re.sub(r'<0>__(\w+)', r'_cm_\1', content)

    # 16. Local variable: CallSite <>p__ -> CallSite _cpl
    content = content.replace('CallSite <>p__', 'CallSite _cpl')
    # And usage: target(<>p__, -> target(_cpl,
    content = content.replace('(<>p__,', '(_cpl,')
    content = content.replace('(<>p__)', '(_cpl)')

    # 17. this.<fieldName>P -> this._fp_fieldName (primary constructor params)
    content = re.sub(r'this\.<(\w+)>P\.', r'this._fp_\1.', content)
    content = re.sub(r'this\.<(\w+)>P(?=[;\s,)])', r'this._fp_\1', content)

    # 18. Field declaration: <fieldName>P -> _fp_fieldName
    content = re.sub(r'<(\w+)>P(?=[;\s])', r'_fp_\1', content)

    # 19. Any remaining <> with word chars -> _r_
    content = re.sub(r'<>(\w)', r'_r_\1', content)

    if content != original:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

# Process all C# files
total_fixed = 0
for filepath in glob.glob(os.path.join(BASE, "**", "*.cs"), recursive=True):
    if fix_file(filepath):
        total_fixed += 1
        print(f"Fixed: {os.path.relpath(filepath, BASE)}")

print(f"\nTotal files fixed: {total_fixed}")
