# PROMPT GLOBAL — copiar ANTES do prompt de asset

**Uso:** cole somente o bloco abaixo no ChatGPT, seguido do bloco `PROMPT DE GERAÇÃO` do asset escolhido. **Anexe antes** as imagens indicadas na ficha daquele prompt; não mandar este arquivo isoladamente sem um alvo.

## PROMPT GLOBAL

```text
PROJECT: Neko Maid Inc. — original cozy anime 2D cat-maid café management game.

TASK: Produce ONE production-oriented, MODULAR 2D art asset (or precisely one requested variant) suitable for assembly as an interactive Unity UI element or an isolated environmental sprite. Do NOT design a complete game screen, a contact sheet, a slide, a marketing mockup or a finished UI screenshot.

ART DIRECTION: Cozy Illustrated Anime Café + Ornamental Visual Novel. Warm, welcoming, expressive illustrated-anime finish with refined colored outlines, soft painterly shading, subtle natural texture, delicate café-related ornamental motifs, visually clean hierarchy and readable silhouette at real game size. Avoid artificial pixelation, pixel-art grids, chunky aliased edges, chibi characters copied from the reference, photorealism, generic flat corporate icons, plastic 3D renders and excessive gold/visual clutter.

REFERENCE POLICY: Use the attached reference images for atmosphere, shape grammar and compatibility only. Never copy their exact layout, text, character designs or decorative motifs pixel for pixel. The old style anchor includes pixel art; DO NOT reproduce its pixelation. When an approved asset reference is attached, match its finish, line weight, ornament scale and lighting without cloning it.

MODULARITY: Isolate the requested item with clean margins; no other unrelated items, scenery, branding or explanatory labels. Keep all functional text, prices, stats, roles, currency amounts, character names, action labels and localization out of the image. Unity overlays and controls these dynamically. Exception ONLY when the asset prompt explicitly requests the permanent static game logo.

TRANSPARENCY: For a frame, icon, portrait, overlay, button or object, prefer a genuine transparent PNG background, not painted checkerboard transparency. If transparency is unavailable, use a uniformly contrasting matte that can be removed cleanly in Blender without losing pale interior fills, thin outlines, glow or shadows. Keep the asset entirely inside the canvas with breathing room; no clipped corners or edge-touching elements. For architectural/background layers, an opaque background is allowed when the prompt explicitly says so.

RESPONSIVENESS: The asset will be rendered in Unity at 960x600 and 1280x720. Favor a clear stable center and legible corners. If the prompt requests a resizable 9-slice asset, keep four symmetrical corners and uniform straight side bands; avoid textured/hard-coded labels in the stretchable center.

OUTPUT: Return the image only, not an explanation, markup, code, sprite sheet of unrelated assets, watermark, fake UI screenshot or decorative presentation page. One variant per render. Preserve consistent viewpoint, lighting and margins with approved sibling pieces.
```
