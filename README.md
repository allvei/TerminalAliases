# TerminalAliases

A Lethal Company mod that lets you define custom terminal command aliases using a simple multiline configuration.

## Features

- Define aliases in a simple `alias=command` format, one per line
- Supports arguments: `fl=flash` allows `fl Phil` to expand to `flash Phil`
- Works with modded commands: `buy=purchase` makes `hotbarplus buy` work as `hotbarplus purchase`
- Case-insensitive matching with case preservation
- Hot-reload: changes take effect immediately when config is saved

## Configuration

Edit the config file at `BepInEx/config/TerminalAliases.cfg`:

```ini
[Aliases]
Definitions = vm=view monitor
sh=switch
sc=scan
st=store
m=moons
fl=flash
buy=purchase
```

## Examples

| Input | Expands To |
|-------|------------|
| `vm` | `view monitor` |
| `fl Phil` | `flash Phil` |
| `hotbarplus buy` | `hotbarplus purchase` |
| `SC` | `SCAN` |

## Installation

1. Install BepInEx 5.x
2. Place `TerminalAliases.dll` in `BepInEx/plugins/`
3. Run the game once to generate the config file
4. Edit the config to add your aliases
