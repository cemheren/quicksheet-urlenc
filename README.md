# quicksheet-urlenc

[QuickSheet](https://github.com/cemheren/QuickSheet) extension for URL encoding and decoding — percent-encoding for query strings, paths, and full URIs.

## Usage

Type the `urlenc:` prefix followed by text in any QuickSheet cell. Results appear in adjacent cells.

**Exact cell text** (copy-paste into QuickSheet):

| Cell text | Output |
|-----------|--------|
| `urlenc: hello world` | Component: `hello%20world` + Full URI: `hello%20world` |
| `urlenc: hello%20world` | Auto-detects encoded input → Decoded: `hello world` |
| `urlenc: https://example.com/path?q=hello world&lang=en` | Component + Full URI encodings |
| `urlenc: café résumé` | Component: `caf%C3%A9%20r%C3%A9sum%C3%A9` |
| `urlenc: hello world,encode` | Explicit encode: `hello%20world` |
| `urlenc: hello%20world,decode` | Explicit decode: `hello world` |
| `urlenc: /api/v1/users/john doe,path` | Path mode (preserves `/`): `/api/v1/users/john%20doe` |

## Modes

- **auto** (default) — detects `%XX` sequences → decode; otherwise → encode (shows both component and full URI forms)
- **encode** / **enc** / **e** — force URL-encode (component-level, encodes everything)
- **decode** / **dec** / **d** — force URL-decode
- **component** / **comp** / **c** — same as encode (encodeURIComponent equivalent)
- **path** / **p** — encode but preserve `/` for path segments

## Install

```
ext: github:cemheren/quicksheet-urlenc
```

## Features

- Auto-detect encode vs decode based on input
- Component encoding (like `encodeURIComponent`)
- Full URI encoding (like `encodeURI` — preserves `:/?#[]@!$&'()*+,;=`)
- Path-safe encoding (preserves `/`)
- UTF-8 aware — handles emoji, accented characters, CJK
- Zero network, zero NuGet — pure .NET 9

## Requirements

- [QuickSheet](https://github.com/cemheren/QuickSheet)
- .NET 9 SDK

## License

MIT
