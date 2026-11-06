const fs=require('fs');
(async()=>{
 const targetUrl=process.argv[3]||'http://127.0.0.1:8765/';
 const tabs=await (await fetch('http://127.0.0.1:9223/json/list')).json();
 const ws=new WebSocket(tabs.find(t=>t.type==='page').webSocketDebuggerUrl);
 await new Promise(r=>ws.onopen=r); let id=0; const waiting=new Map(); const logs=[];
 ws.onmessage=e=>{const m=JSON.parse(e.data); if(m.id){const p=waiting.get(m.id);waiting.delete(m.id);m.error?p.reject(m.error):p.resolve(m.result)}else if(m.method==='Runtime.consoleAPICalled'||m.method==='Runtime.exceptionThrown')logs.push({method:m.method,type:m.params.type,text:(m.params.args||[]).map(x=>String(x.value??x.description??'')).join(' ').slice(0,3000),exception:m.params.exceptionDetails?.text})};
 const call=(method,params={})=>new Promise((resolve,reject)=>{const i=++id;waiting.set(i,{resolve,reject});ws.send(JSON.stringify({id:i,method,params}))});
 const ev=async expression=>(await call('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true})).result.value;
 await call('Runtime.enable'); await call('Page.enable');
 await call('Emulation.setDeviceMetricsOverride',{width:1280,height:720,deviceScaleFactor:1,mobile:false});
 await call('Page.addScriptToEvaluateOnNewDocument',{source:`window.perfTextures=[];const compress=WebGL2RenderingContext.prototype.compressedTexImage2D;WebGL2RenderingContext.prototype.compressedTexImage2D=function(...a){window.perfTextures.push({level:a[1],format:a[2],width:a[3],height:a[4],bytes:a[6]?.byteLength??0});return compress.apply(this,a)};const storage=WebGL2RenderingContext.prototype.texStorage2D;WebGL2RenderingContext.prototype.texStorage2D=function(...a){window.perfTextures.push({storage:true,levels:a[1],format:a[2],width:a[3],height:a[4]});return storage.apply(this,a)};window.perfStart=performance.now();window.perfReady=null;new MutationObserver(()=>{const bar=document.querySelector('#unity-loading-bar');if(bar?.style.display==='none'&&!window.perfReady)window.perfReady=performance.now()}).observe(document,{attributes:true,subtree:true});`});
 await call('Network.enable'); await call('Network.setCacheDisabled',{cacheDisabled:true});
 await call('Storage.clearDataForOrigin',{origin:new URL(targetUrl).origin,storageTypes:'all'});
 await call('Page.navigate',{url:targetUrl});
 for(let i=0;i<45;i++){await new Promise(r=>setTimeout(r,1000)); if(await ev('window.perfReady'))break;}
 await new Promise(r=>setTimeout(r,2000));
 await call('Runtime.discardConsoleEntries'); await call('HeapProfiler.collectGarbage');
 const ready=await ev('({ready:window.perfReady,start:window.perfStart,heap:performance.memory?.usedJSHeapSize})');console.log(ready);if(!ready?.ready)throw new Error('Unity did not signal readiness within 45 seconds');
 await ev(`document.body.style.margin='0';document.body.style.overflow='hidden';document.querySelector('#unity-container').style.cssText='position:absolute;left:0;top:0;transform:none';document.querySelector('canvas').focus()`);
 await call('Page.bringToFront');
 
 
 await ev(`document.dispatchEvent(new KeyboardEvent('keydown',{code:'F8',key:'F8',keyCode:119,which:119,bubbles:true}))`);
 await new Promise(r=>setTimeout(r,300));
 await ev(`document.dispatchEvent(new KeyboardEvent('keyup',{code:'F8',key:'F8',keyCode:119,which:119,bubbles:true}))`);
 await new Promise(r=>setTimeout(r,4000));
 const label=process.argv[2]||'baseline';
 for(const [w,h] of [[1280,720],[960,600]]){
 await call('Emulation.setDeviceMetricsOverride',{width:w,height:h,deviceScaleFactor:1,mobile:false});
 await ev(`document.querySelector('canvas').style.width='${w}px';document.querySelector('canvas').style.height='${h}px';document.querySelector('canvas').width=${w};document.querySelector('canvas').height=${h}`);
 await new Promise(r=>setTimeout(r,1500));
 const shot=await call('Page.captureScreenshot',{format:'png'});fs.writeFileSync(`docs/performance/${label}-${w}x${h}.png`,Buffer.from(shot.data,'base64'));
 await new Promise(r=>setTimeout(r,400)); const motion=await call('Page.captureScreenshot',{format:'png'});fs.writeFileSync(`docs/performance/${label}-${w}x${h}-walk.png`,Buffer.from(motion.data,'base64')); 
 }
 await call('HeapProfiler.collectGarbage');
 const sample=await ev(`new Promise(resolve=>{let previous=performance.now(),times=[];function step(now){times.push(now-previous);previous=now;if(times.length<300)requestAnimationFrame(step);else resolve({frames:times.length,mean:times.reduce((a,b)=>a+b,0)/times.length,max:Math.max(...times),p95:times.sort((a,b)=>a-b)[Math.floor(times.length*.95)],jsHeap:performance.memory.usedJSHeapSize})}requestAnimationFrame(step)})`);
 const gl=await ev(`(()=>{const g=document.querySelector('canvas').getContext('webgl2');return g?{extensions:g.getSupportedExtensions(),renderer:g.getParameter(g.RENDERER)}:null})()`);
 fs.writeFileSync(`docs/performance/${label}-browser.json`,JSON.stringify({ready,sample,gl,compressedUploads:await ev("window.perfTextures"),logs},null,2));console.log(sample,logs.length);ws.close();process.exit(0);
})().catch(e=>{console.error(e);process.exit(1)});
