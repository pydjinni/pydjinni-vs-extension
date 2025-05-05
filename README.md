# PyDjinni Visual Studio Extension

[![Visual Studio Marketplace Downloads](https://img.shields.io/visual-studio-marketplace/d/PyDjinni.PyDjinni)](https://marketplace.visualstudio.com/items?itemName=PyDjinni.PyDjinni)
[![Visual Studio Marketplace Version](https://img.shields.io/visual-studio-marketplace/v/PyDjinni.PyDjinni)](https://marketplace.visualstudio.com/items?itemName=PyDjinni.PyDjinni)
[![Visual Studio Marketplace Rating](https://img.shields.io/visual-studio-marketplace/r/PyDjinni.PyDjinni)](https://marketplace.visualstudio.com/items?itemName=PyDjinni.PyDjinni&ssr=false#review-details)


## Prerequisites

This extension relies on the language server that comes with the PyDjinni Toolchain.
Make sure to install PyDjinni in your active Python environment:

```sh
pip install pydjinni
```

## Features

* 🌈 Syntax highlighting
* 🔎 Syntax error reporting
* 🎯 Jump to definition
* ⚠️ Deprecation warnings
* ℹ️ Type documentation on hover
* 📋 File outline
* 💾 Automatically generate code on save
* 🤖 IntelliSense support for type and language target completion

## Configuration

### Global Language Server Settings

Global PyDjinni Language server settings can be found under: **Tools | Options | PyDjinni | General**

### Project Specific Settings

In order to customize project specific settings, add a `VSWorkspaceSettings.json` to the root of the project:

```jsonc
{
  // Path to the PyDjinni configuration file
  "pydjinni.config": "pydjinni.yaml",
  // Whether to run the PyDjinni generator on file save
  "pydjinni.generateOnSave": false
}
```
