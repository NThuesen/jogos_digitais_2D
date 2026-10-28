// Rasterises SVG strings to PNG with resvg. Fonts are loaded from FONTS_DIR (see README.md).
import fs from 'node:fs';
import path from 'node:path';

const { Resvg } = await import(process.env.RESVG_PATH ?? '@resvg/resvg-js');
const FONTS = process.env.FONTS_DIR ?? path.join(process.cwd(), 'node_modules', '@expo-google-fonts');

export const FONT_FILES = [
  'fredoka/500Medium/Fredoka_500Medium.ttf', 'fredoka/700Bold/Fredoka_700Bold.ttf',
  'nunito/600SemiBold/Nunito_600SemiBold.ttf', 'nunito/700Bold/Nunito_700Bold.ttf', 'nunito/800ExtraBold/Nunito_800ExtraBold.ttf',
  'patrick-hand/400Regular/PatrickHand_400Regular.ttf',
  'dela-gothic-one/400Regular/DelaGothicOne_400Regular.ttf',
  'rubik/500Medium/Rubik_500Medium.ttf', 'rubik/700Bold/Rubik_700Bold.ttf',
].map((p) => path.join(FONTS, p));

export function toPng(svg, file, scale = 1) {
  const r = new Resvg(svg, {
    fitTo: scale === 1 ? { mode: 'original' } : { mode: 'zoom', value: scale },
    font: { fontFiles: FONT_FILES, loadSystemFonts: false, defaultFontFamily: 'Nunito' },
  });
  const png = r.render().asPng();
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, png);
  return png;
}
