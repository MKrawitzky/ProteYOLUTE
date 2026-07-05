"""
Fix decompilation artifacts in dnSpy-decompiled C# code.
Replaces invalid compiler-generated identifiers with valid C# names.
"""
import re
import glob
import os

BASE = r"C:\BDalSystemData\HyStar\LcPlugin\PrivateData\Bruker proteoElute\DLL_Extract\dnspy_baltic\BalticWpfControlLib"

def fix_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    original = content

    # 1. Fix CS$<>8__locals1 -> _locals1
    content = re.sub(r'CS\$<>8__locals(\d+)', r'_locals\1', content)

    # 2. Fix <>4__this -> _this
    content = re.sub(r'<>4__this', '_this', content)

    # 3. Fix <>8__locals -> _locals
    content = re.sub(r'<>8__locals', '_locals', content)

    # 4. Fix ClassName.<>o__NN.<>p__NN -> ClassName._callsite_NN_NN
    # Pattern: SomeClass.<>o__39.<>p__1  ->  SomeClass._cs_39_1
    content = re.sub(r'(\w+)\.<>o__(\d+)\.<>p__(\d+)', r'\1._cs_\2_\3', content)

    # 5. Fix standalone <>o__NN (class declarations) -> _cacheClass_NN
    content = re.sub(r'<>o__(\d+)', r'_cacheClass_\1', content)

    # 6. Fix <>p__NN -> _field_NN
    content = re.sub(r'<>p__(\d+)', r'_field_\1', content)

    # 7. Fix <>c__DisplayClassNN_N -> _closure_NN_N
    content = re.sub(r'<>c__DisplayClass(\d+)_(\d+)', r'_closure_\1_\2', content)

    # 8. Fix <>c -> _closureHelper
    content = re.sub(r'<>c(?=[^_a-zA-Z0-9])', '_closureHelper', content)

    # 9. Fix <SomeMethod>b__N_N -> _SomeMethod_lambda_N_N
    content = re.sub(r'<(\w+)>b__(\d+)_(\d+)', r'_\1_lambda_\2_\3', content)

    # 10. Fix <SomeMethod>g__Action|N -> _SomeMethod_localfn_N
    content = re.sub(r'<(\w+)>g__(\w+)\|(\d+)', r'_\1_local_\2_\3', content)

    # 11. Fix <>O -> _staticDelegates
    content = re.sub(r'<>O(?=[^a-zA-Z0-9_])', '_staticDelegates', content)

    # 12. Fix remaining <MethodName>d__N (async state machines)
    content = re.sub(r'<(\w+)>d__(\d+)', r'_\1_statemachine_\2', content)

    # 13. Fix <>9__N_N (cached delegates)
    content = re.sub(r'<>9__(\d+)_(\d+)', r'_cachedDelegate_\1_\2', content)
    content = re.sub(r'<>9(?=[^_a-zA-Z0-9])', r'_cachedDelegate', content)

    # 14. Fix NullableFlags and other attribute artifacts
    # These usually cause CS1001 in specific patterns

    # 15. Fix $ in front of strings that shouldn't be there (rare decompilation artifact)
    # Only if it's not inside a regex string

    if content != original:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        changes = sum(1 for a, b in zip(original, content) if a != b)
        return True
    return False

# Process all C# files
total_fixed = 0
for filepath in glob.glob(os.path.join(BASE, "**", "*.cs"), recursive=True):
    if fix_file(filepath):
        total_fixed += 1
        print(f"Fixed: {os.path.relpath(filepath, BASE)}")

print(f"\nTotal files fixed: {total_fixed}")
