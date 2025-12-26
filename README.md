# TerminalAliases

A Lethal Company mod that lets you define custom terminal command aliases directly from the terminal.

## Features

- **Commands** to add, list, and remove aliases
- Aliases stored in `BepInEx/config/TerminalAliases.cfg`
- Supports arguments: `fl` expands to `flash`, so `fl Alberts` becomes `flash Alberts`
- Case-insensitive matching with case preservation
- **Ctrl+Enter** auto-confirms purchases and actions

## Commands

| Command | Description |
|---------|-------------|
| `alias` | List all defined aliases |
| `alias [name] [command]` | Create or update an alias |
| `removealias [name]` | Remove an alias |

## Usage Examples

```
> alias vm view monitor
Alias 'vm' set to 'view monitor'

> alias fl flash
Alias 'fl' set to 'flash'

> alias
Defined aliases:
  vm = view monitor
  fl = flash

> removealias vm
Alias 'vm' removed.
```

Once defined, typing `fl Alberts` will expand to `flash Alberts`.