

## Codely Structured Memories

### User

### Feedback

### Project
- [2026-09-05 14:51:50] [project] AesirInspector naming trap: since 0.9.0 the Odin assemblies/dirs are renamed to `OdinInspector` but the NAMESPACES still use `OdinIntegration` — Runtime namespace `Runestone.AesirInspector.OdinIntegration` vs assembly `Runestone.AesirInspector.OdinInspector`; Editor namespace `Runestone.AesirInspector.OdinIntegration.Editor` vs assembly `Runestone.AesirInspector.Editor.OdinInspector`. **Why:** the rename covered asmdefs/dirs only; renaming one but not the other breaks compilation (bit me when fixing Samples~ on 2026-09-05). **How to apply:** in .asmdef `references` use assembly names (`...OdinInspector`), in C# `using`/`namespace` use `...OdinIntegration`; verify with grep before assuming they match.
### Reference

