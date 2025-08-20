<div align="center">

![sergio](https://github.com/eastcitysoftware/sergio/blob/assets/sergio.png?raw=true)

[![build](https://github.com/eastcitysoftware/sergio/actions/workflows/build.yml/badge.svg)](https://github.com/eastcitysoftware/sergio/actions/workflows/build.yml)
![License](https://img.shields.io/github/license/eastcitysoftware/sergio)

Static web server with hot reload.
</div>

---

Meet Sergio, the no-frills static web server that’s here to make your life easier. Need to spin up a local server faster than you can say "localhost"? Sergio’s got you covered. No bloated configs, no unnecessary drama—just pure, unadulterated serving power. Whether you’re testing, debugging, or showing off your latest masterpiece, Sergio serves it hot and fresh every time.

Because life's too short for 404s.

---

## Usage

```shell
Usage of .\sergio.exe:

Description:
  sergio, static web server with hot reload

Usage:
  sergio <input> [options]

Arguments:
  <input>  The absolute path to a website directory or sergio.json file

Options:
  -?, -h, --help            Show help and usage information
  --version                 Show version information
  port, -p                  The port to listen on [default: 8080]
  disable-compression, -ec  Disable response compression
  verbose                   Enable verbose logging
```

## Examples

### Run a single website

```shell
sergio C:\path\to\website
```

Or, to configure:

```shell
sergio C:\path\to\website --port 3000 --disable-compression --verbose
```

### Run multiple websites

```shell
sergio C:\path\to\sergio.json
```

Or, to configure:

```shell
sergio C:\path\to\sergio.json --port 3000 --disable-compression --verbose
```

### Example `sergio.json`

```json
[
    {
      "domain": "site1.local",
      "root": "C:\\path\\to\\website1",
      "cacheExpirationSeconds": 3600
    },
    {
      "domain": "site2.local",
      "root": "C:\\path\\to\\website2",
      "cacheExpirationSeconds": 60
    }
]

```
