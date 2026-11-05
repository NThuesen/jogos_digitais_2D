import sys
from PIL import Image
ids=['suzu','hina','akari','momo','reina','kaede','nene','ichika','yoru']
Q='Assets/_Game/Tests/PlayMode/QaArt/'
o=Image.new('RGB',(3*392,3*327),(110,110,110))
for i,m in enumerate(ids):
    h=Image.open(Q+m+'_head_192.png').convert('RGBA').resize((384,314),Image.LANCZOS)
    bg=Image.new('RGBA',h.size,(140,105,75,255));bg.alpha_composite(h)
    o.paste(bg.convert('RGB'),((i%3)*392+4,(i//3)*327+4))
o.save(sys.argv[1])
