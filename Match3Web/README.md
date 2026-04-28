# Match3 Web (Play Without Unity)

This folder gives you a playable browser version so you can test game feel without opening Unity.

## Run locally

Option A (Python):

```bash
cd Match3Web
python3 -m http.server 8080
```

Then open: `http://localhost:8080`

Option B (Node):

```bash
npx serve Match3Web
```

## How to add updated AI-generated graphics

1. Generate tile sprites with your image model ("GPT image" / latest image generation model) at **1024x1024 PNG** with transparent background.
2. Export a consistent set:
   - `tile_red.png`, `tile_blue.png`, `tile_green.png`, `tile_yellow.png`, `tile_purple.png`, `tile_orange.png`
3. Place files in `Match3Web/assets/tiles/`.
4. Replace the `drawCell` function in `main.js` with image rendering (`drawImage`) or keep current glossy procedural style.

## Prompt template for new graphics

"Create a bright, candy-like match-3 tile icon, rounded shape, strong silhouette, glossy highlight, high contrast, transparent PNG, no text, no background, game-ready mobile UI style."

## Fast publish (shareable playable link)

- Drag-drop `Match3Web` folder to Netlify Drop
- Or deploy to Vercel / GitHub Pages

You immediately get a public URL to play and share.
