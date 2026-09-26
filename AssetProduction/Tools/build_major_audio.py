"""Original synthesized arcade music; no external samples or compositions."""
import math, random, wave, struct
from pathlib import Path
out=Path.cwd()/'Assets/Resources/Runner/Audio';out.mkdir(parents=True,exist_ok=True)
rate=22050; beat=.6; duration=beat*32; samples=[0.0]*int(rate*duration)
rng=random.Random(73)
def add(start,length,voice):
    first=int(start*rate)
    for i in range(int(length*rate)):
        at=(first+i)%len(samples);samples[at]+=voice(i/rate)
def freq(note):return 440*2**((note-69)/12)
progression=[48,53,55,50,48,53,55,55]
for bar,root in enumerate(progression):
    base=bar*4*beat
    for step in range(8):
        t=base+step*beat/2
        add(t,.055,lambda x:(rng.random()*2-1)*math.exp(-x*80)*.065)
        if step%2==0:
            add(t,.24,lambda x:math.sin(2*math.pi*(48*x+3.4*(1-math.exp(-x*24))))*math.exp(-x*19)*.24)
        if step in (2,6):
            add(t,.12,lambda x:(rng.random()*2-1)*math.exp(-x*28)*.10)
        n=root+(0,7,12,7,0,7,14,12)[step];f=freq(n)
        add(t,.23,lambda x,f=f:(2/math.pi*math.asin(math.sin(2*math.pi*f*x)))*min(1,x*100)*math.exp(-x*14)*.14)
        if step in (1,3,5,7):
            melody=freq(root+24+(0,4,7,12)[step//2])
            add(t,.29,lambda x,f=melody:(math.sin(2*math.pi*f*x)+.2*math.sin(4*math.pi*f*x))*math.exp(-x*13)*min(1,x*160)*.08)
peak=max(abs(v) for v in samples)
with wave.open(str(out/'music-v002.wav'),'wb') as wav:
    wav.setnchannels(1);wav.setsampwidth(2);wav.setframerate(rate)
    wav.writeframes(b''.join(struct.pack('<h',int(v/max(1,peak)*30000)) for v in samples))
print('Original music loop:',duration,'seconds; peak',round(peak,3))
