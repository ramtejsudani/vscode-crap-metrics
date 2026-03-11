# Contributing

## Development Setup

1. Clone the repo
2. Build server: `cd server/CrapMetricsServer && dotnet build`
3. Install client deps: `cd client && npm install && npm run compile`
4. Press `F5` in VS Code to run in debug mode

## Branching

- `main` — stable, protected. PRs only.
- `develop` — integration branch
- Feature branches: `feature/your-feature`
- Bug fixes: `fix/your-bug`

## Commit Messages

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add support for property analyzers
fix: codelens not refreshing on save
chore: bump OmniSharp to 0.19.10
docs: update README with coverage instructions
```

This drives automatic versioning via semantic-release.

## Versioning

Versions are managed automatically by `semantic-release` in CI:
- `fix:` commits → patch bump (0.0.x)
- `feat:` commits → minor bump (0.x.0)  
- `BREAKING CHANGE:` in commit body → major bump (x.0.0)

**Do not manually edit the version in `package.json`.**
