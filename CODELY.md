

## Codely Structured Memories

### User

### Feedback

### Project
- [2026-09-05 15:17:19] [project] AesirInspector assembly scheme (since 0.14.0, 2026-09-05): exactly two assemblies — Runtime `Runestone.AesirInspector` (namespace Runestone.AesirInspector) and Editor `Runestone.AesirInspector.Editor` (namespace Runestone.AesirInspector.Editor); Odin Inspector is a hard dependency (no ODIN_INSPECTOR defineConstraints, no #if guards, Sirenix APIs used directly). **Why:** 0.14.0 merged the former Unity/OdinInspector split assemblies and unified the old `Runestone.AesirInspector.OdinIntegration(.Editor)` namespaces into the base namespaces. **How to apply:** new code uses only these two namespaces/asmdefs; pre-0.14 code needs the migration table in CHANGELOG 0.14.0.

### Reference

