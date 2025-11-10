# Tooling & Standards Checklist

## EditorConfig
- `.editorconfig` (root) defines UTF-8, LF endings, trailing whitespace trim, final newline.
- C# defaults: 4-space indent, brace preference, explicit types for built-ins, consistent `var` usage when type apparent.
- MSBuild files use 2-space indent.

## Roslyn Analyzers
- `Directory.Build.props` enables .NET analyzers with `latest-recommended` level for every generated project (Unity will import this when regenerating `.csproj` files).
- Treat warnings as guidance for now; severity escalations will be phased in alongside subsystem refactors.

## Next Steps
1. Evaluate adding StyleCop or Unity-specific analyzers once initial SOLID cleanup reduces noise.
2. Hook analyzer execution into any CI/build automation (`dotnet build /warnAsError` once codebase is compliant).
3. Document IDE setup (Rider/VS/VS Code) so team members share formatting defaults.
