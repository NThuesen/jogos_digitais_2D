"""Original Suzu study: independent authored paths, never segmented reference art."""
from pathlib import Path
import cairosvg
ROOT=Path(__file__).resolve().parents[4]
OUT=ROOT/'Assets/Experiments/SuzuModular/Parts'
SRC=Path(__file__).parent
ink='#594742'; mint='#a4caac'; dark='#71978c'; cream='#fff0d8'; brown='#684a43'; rose='#a34858'
parts={}
def p(d,c,stroke=ink,w=.8): return f'<path d="{d}" fill="{c}" stroke="{stroke}" stroke-width="{w}" stroke-linejoin="round" stroke-linecap="round"/>'
def part(n,*paths): parts[n]=''.join(paths)
part('00_tail',p('M39 67 Q17 80 14 62 Q13 52 20 51 Q26 52 23 56 Q18 55 19 62 Q21 71 38 61Z',cream),p('M14 58 Q12 51 20 50 Q27 50 24 56 L20 55 L18 59Z',mint))
for name,x,y,c in [('01_far_leg',44,76,'#d4bcae'),('02_near_leg',54,77,cream)]:
 part(name,p(f'M{x-4} {y} L{x+4} {y} L{x+3} 101 Q{x} 104 {x-3} 101Z',c),p(f'M{x-3} 98 Q{x+1} 96 {x+4} 99 L{x+9} 101 Q{x+11} 105 {x+7} 106 L{x-4} 106 L{x-5} 103Z',brown),p(f'M{x-2} 100 L{x+3} 100',cream,w=1.5))
part('03_far_arm',p('M57 48 Q66 48 66 57 L63 69 Q59 73 57 68 L59 58Z','#d8ad97'))
part('04_bodice',p('M42 43 Q49 39 57 45 L62 61 L39 62Z',brown),p('M46 45 L54 44 L57 60 L45 60Z',cream),p('M44 44 L48 49 L52 44 L56 48 L57 43',cream),p('M39 58 L60 58 L61 63 L39 64Z',brown))
part('05_skirt',p('M40 59 Q48 62 59 59 Q62 70 70 81 Q55 89 31 81 Q37 70 40 59Z',brown),p('M34 78 Q38 83 41 80 Q45 85 48 82 Q52 87 56 83 Q61 86 64 81 L68 80 L69 84 Q50 92 30 83Z',cream),p('M42 64 Q39 73 36 78 M57 64 Q59 73 64 79','none','#99705c'),p('M45 61 L58 62 Q58 71 64 78 Q52 83 40 77Z',cream),p('M48 65 Q51 70 56 68 M45 75 Q53 79 60 77','none','#d6b6a5'))
part('06_near_arm',p('M39 49 Q35 51 36 59 L41 70 Q45 74 48 70 Q49 68 45 66 L43 57Z','#f4c4a7'),p('M40 46 Q33 46 32 53 L35 61 Q39 63 44 59 L46 52Z',brown),p('M34 59 L36 64 L39 62 L42 64 L45 60 L43 58Z',cream),p('M39 49 Q36 51 37 56','none','#956b5d'))
part('07_hair_back',p('M32 25 Q31 11 47 10 Q65 9 69 25 L68 47 L62 51 L57 47 L54 50 L42 48 L34 50 Q27 41 32 25Z',mint),p('M33 26 Q30 43 36 47 L40 46 Q36 34 40 20Z',dark),p('M61 23 Q68 38 63 47 L67 45 Q72 31 65 21Z',dark))
part('08_face',p('M42 25 Q51 19 62 25 L66 35 L70 39 L66 41 Q66 46 58 48 Q47 49 40 42Z','#f8ceb0'),p('M42 38 Q44 45 55 48 L58 47 Q46 44 45 39Z','#eeb496','none'),p('M61 43 Q63 45 65 43','none','#a65f5c'),p('M58 41 L62 41','none','#de8b7c',1.5))
part('09_eye',p('M55 33 Q60 30 64 34 L63 40 Q59 43 56 39Z',cream,'none'),p('M59 33 Q63 33 63 37 Q63 41 59 40 Q57 37 59 33Z','#d99b43','none'),p('M60 33 L60 37','none',ink,1.5),p('M55 34 Q59 30 64 34 L65 33','none',ink,1.3),p('M59 34 L59 35','none',cream,1.3),p('M56 29 Q59 28 62 29','none','#8f7662'))
part('10_fringe',p('M32 29 Q30 14 44 11 Q58 8 66 21 L68 32 L63 31 L61 23 Q61 32 57 33 L55 24 L53 34 L49 32 L48 24 Q47 38 42 42 L45 43 Q40 47 35 41 Q31 37 32 29Z',mint),p('M36 26 Q35 17 46 14 L43 19 L40 28Z','#d7e5bd','none'),p('M48 16 L51 14 L55 15 L54 23 L51 24Z','#c4dfb7','none'),p('M57 17 L60 18 L62 25 L59 24Z','#c4dfb7','none'),p('M39 29 Q35 40 41 42 M46 16 Q44 23 43 29 M55 16 L57 28','none',dark))
part('11_ears',p('M33 18 L33 3 L39 7 L42 6 L42 10 L49 13Z',cream),p('M36 14 L35 7 L44 13Z','#d68d88','none'),p('M56 12 L68 4 L67 20Z',cream),p('M60 13 L66 8 L65 17Z','#d68d88','none'))
part('12_headdress',p('M35 19 L34 15 L37 12 L40 13 L41 9 L45 8 L48 10 L51 8 L55 9 L57 12 L60 12 L63 16 L62 20 Q49 11 35 22Z',cream),p('M39 14 L40 18 M46 11 L47 16 M53 11 L53 16 M59 15 L58 18','none','#d3aa9e'))
part('13_side_lock',p('M33 29 Q25 32 30 42 Q34 47 29 52 Q39 49 37 42 Q32 37 36 33Z',mint),p('M31 36 Q29 39 34 44','none',dark))
part('14_bow',p('M34 29 Q27 25 27 29 L28 34 L34 32 L39 35 L41 29 Q38 27 34 29Z',rose),p('M32 32 L29 40 L32 39 L34 34 L37 40 L39 39 L36 32Z',rose),p('M33 32 Q37 31 38 35 Q38 39 34 38 Q30 37 33 32Z','#e4ac55'),p('M34 36 L36 36','none',ink),p('M33 33 L34 33','none',cream))
part('15_neck_ribbon',p('M53 51 L48 49 L48 53 L53 53 L58 56 L59 52 L54 51Z',rose),p('M53 53 L51 58 L54 57 L55 54Z',rose),p('M53 51 L54 52','none','#e4ac55',1.5))
parts['10_fringe'] += p('M35 31 Q33 39 38 41 M38 29 Q35 37 40 39 M48 19 L47 27 M51 19 L51 26 M59 21 L60 27','none','#c8dfb7',1.1)
parts['05_skirt'] += p('M42 72 L44 74 L43 76 L46 78 L49 77 L51 80 L54 78 L57 80 L60 77','none','#d6b6a5',.7)
parts['04_bodice'] += p('M46 46 L44 49 L46 51 L44 53 L46 55','none','#d6b6a5',.8)
for n,body in parts.items():
 svg=f'<svg xmlns="http://www.w3.org/2000/svg" width="96" height="112" viewBox="0 0 96 112">{body}</svg>'
 (SRC/(n+'.svg')).write_text(svg)
 cairosvg.svg2png(bytestring=svg.encode(),write_to=str(OUT/(n+'.png')),output_width=192,output_height=224)
(SRC/'assembly.svg').write_text('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 96 112">'+''.join(parts.values())+'</svg>')
