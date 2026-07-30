using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace CPXnbExporter
{
    public static class TileSheetMerger
    {
        public const string DefaultHost = "Maps/busPeople";
        public static bool IsVirtual(TileSheet ts){string s=ts?.ImageSource?.Replace('\\','/');return!string.IsNullOrEmpty(s)&&(s.StartsWith("SMAPI/",StringComparison.OrdinalIgnoreCase)||s.Contains("/Mods/",StringComparison.OrdinalIgnoreCase)||s.StartsWith("Mods/",StringComparison.OrdinalIgnoreCase));}
        static int Gcd(int a,int b){while(b!=0){int t=b;b=a%b;a=t;}return a;}
        static int Lcm(int a,int b)=>a/Gcd(a,b)*b;

        public static Texture2D Merge(Map map,string hostName,IModHelper h,IMonitor m)
        {
            if(map==null||h==null)return null;
            var vList=map.TileSheets.Where(IsVirtual).ToList();if(vList.Count==0)return null;
            string hid=hostName.Replace('\\','/');int ls=hid.LastIndexOf('/');if(ls>=0)hid=hid[(ls+1)..];
            var hostTs=map.TileSheets.FirstOrDefault(ts=>ts.ImageSource?.Replace('\\','/').Equals(hostName,StringComparison.OrdinalIgnoreCase)==true||ts.Id.Equals(hid,StringComparison.OrdinalIgnoreCase));
            Texture2D hTex;try{hTex=h.GameContent.Load<Texture2D>(hostName);}catch{return null;}
            // 强制 16×16 瓦片尺寸
            const int hTw=16,hTh=16;
            int hPw=hTex.Width,hPh=hTex.Height;
            if(hostTs==null)
            {
                hostTs=new TileSheet(hid,map,hostName,new Size(hTw,hTh),new Size(hPw/hTw,hPh/hTh));
                map.AddTileSheet(hostTs);
            }
            else
            {
                hostTs.TileWidth=hTw;hostTs.TileHeight=hTh;
                hostTs.SheetWidth=hPw/hTw;hostTs.SheetHeight=hPh/hTh;
            }
            var vData=new List<VData>();var all=new List<(VData vd,int ox,int oy,Color[] px)>();
            foreach(var vs in vList)
            {
                Texture2D vt;try{vt=h.GameContent.Load<Texture2D>(vs.ImageSource);}catch{return null;}
                int pw=vt.Width,ph=vt.Height;
                // 强制 16×16 瓦片
                int tw=hTw,th=hTh;
                // 像素提取用纹理尺寸算列数
                int osw=Math.Max(1,pw/tw),osh=Math.Max(1,ph/th);
                // tile index 反推用 map 上的原始 SheetWidth（SDV 1.6 可能已缩放，取原始值）
                int origSw=vs.SheetWidth>0?vs.SheetWidth:osw;
                var vp=new Color[pw*ph];vt.GetData(vp);
                var vd=new VData{Sheet=vs,Pw=pw,Ph=ph,Tw=tw,Th=th,Osw=osw,Osh=osh,OrigSw=origSw,Id=vs.Id,Map=new()};
                for(int ty=0;ty<osh;ty++)for(int tx=0;tx<osw;tx++)
                {
                    bool has=false;int sy=ty*th,ey=Math.Min(sy+th,ph),sx=tx*tw,ex=Math.Min(sx+tw,pw);
                    for(int py=sy;py<ey&&!has;py++)for(int px=sx;px<ex&&!has;px++)if(vp[py*pw+px].A>0)has=true;
                    if(!has)continue;
                    var tp=new Color[tw*th];
                    for(int py=0;py<th&&sy+py<ph;py++)for(int px=0;px<tw&&sx+px<pw;px++)tp[py*tw+px]=vp[(sy+py)*pw+(sx+px)];
                    all.Add((vd,tx,ty,tp));
                }
                vData.Add(vd);
            }
            if(all.Count==0)return null;
            var uniq=new List<(VData vd,int ox,int oy,Color[] px)>();var hs=new HashSet<int>();
            foreach(var(vd,ox,oy,px)in all){int hash=17;for(int i=0;i<px.Length;i++){var c=px[i];hash=hash*31+c.R;hash=hash*31+c.G;hash=hash*31+c.B;hash=hash*31+c.A;}if(!hs.Contains(hash)){hs.Add(hash);uniq.Add((vd,ox,oy,px));}}
            all=uniq;
            int mw=hPw;int mh=hPh;
            foreach(var vd in vData)
            {
                var tiles=all.Where(t=>t.vd==vd).ToList();int tpr=mw/vd.Tw,rows=(tiles.Count+tpr-1)/tpr,uh=rows*vd.Th;
                int oy=mh,rem=oy%vd.Th;if(rem!=0)oy+=vd.Th-rem;vd.Oy=oy;vd.Uh=uh;vd.N=tiles.Count;
                int col=0,row=0;foreach(var(_,ox,oyy,_)in tiles){vd.Map[(ox,oyy)]=(col,row);col++;if(col>=tpr){col=0;row++;}}
                mh=oy+uh;
            }
            var res=new Color[mw*mh];var hp=new Color[hPw*hPh];hTex.GetData(hp);
            for(int y=0;y<hPh;y++)for(int x=0;x<hPw;x++)res[y*mw+x]=hp[y*hPw+x];
            foreach(var vd in vData)foreach(var(_,ox,oy,px)in all.Where(t=>t.vd==vd)){var(nx,ny)=vd.Map[(ox,oy)];int dx=nx*vd.Tw,dy=vd.Oy+ny*vd.Th;for(int py=0;py<vd.Th;py++)for(int px2=0;px2<vd.Tw;px2++)res[(dy+py)*mw+(dx+px2)]=px[py*vd.Tw+px2];}
            var mt=new Texture2D(hTex.GraphicsDevice,mw,mh);mt.SetData(res);
            // 更新 host TileSheet 尺寸
            hostTs.SheetWidth=mw/hTw;hostTs.SheetHeight=mh/hTh;
            // 先重定向所有 tile 引用到 hostTs
            int tilesPerRow=mw/hTw;
            foreach(var layer in map.Layers)for(int y=0;y<layer.LayerHeight;y++)for(int x=0;x<layer.LayerWidth;x++)
            {
                var tile=layer.Tiles[x,y];
                if(tile is StaticTile st&&vData.Any(v=>v.Sheet==st.TileSheet))
                {
                    var vd=vData.First(v=>v.Sheet==st.TileSheet);int otx=st.TileIndex%vd.OrigSw,oty=st.TileIndex/vd.OrigSw;
                    if(vd.Map.TryGetValue((otx,oty),out var np)){int ni=np.Item2*tilesPerRow+np.Item1;layer.Tiles[x,y]=new StaticTile(layer,hostTs,st.BlendMode,ni);foreach(var p in st.Properties)layer.Tiles[x,y].Properties[p.Key]=p.Value;}
                    else layer.Tiles[x,y]=null;
                }
                else if(tile is AnimatedTile anim)
                {
                    var nf=new List<StaticTile>();
                    foreach(var frame in anim.TileFrames)
                    {
                        var vd=vData.FirstOrDefault(v=>v.Sheet==frame.TileSheet);if(vd==null){nf.Add(frame);continue;}
                        int otx=frame.TileIndex%vd.OrigSw,oty=frame.TileIndex/vd.OrigSw;
                        if(vd.Map.TryGetValue((otx,oty),out var np)){int ni=np.Item2*tilesPerRow+np.Item1;var n=new StaticTile(layer,hostTs,frame.BlendMode,ni);foreach(var p in frame.Properties)n.Properties[p.Key]=p.Value;nf.Add(n);}
                    }
                    if(nf.Count>0)layer.Tiles[x,y]=new AnimatedTile(layer,nf.ToArray(),anim.FrameInterval);
                    else layer.Tiles[x,y]=null;
                }
            }
            // 确认没有 tile 引用虚拟瓦片表后再删除
            for(int i=map.TileSheets.Count-1;i>=0;i--)
            {
                if(!IsVirtual(map.TileSheets[i]))continue;
                bool stillUsed=false;
                foreach(var layer in map.Layers)for(int y=0;y<layer.LayerHeight;y++)for(int x=0;x<layer.LayerWidth;x++)
                {
                    var t=layer.Tiles[x,y];
                    if(t is StaticTile s&&s.TileSheet==map.TileSheets[i]){stillUsed=true;break;}
                    if(t is AnimatedTile a&&a.TileFrames.Any(f=>f.TileSheet==map.TileSheets[i])){stillUsed=true;break;}
                }
                if(!stillUsed)map.RemoveTileSheet(map.TileSheets[i]);
            }
            return mt;
        }

        class VData{public TileSheet Sheet;public int Pw,Ph,Tw,Th,Osw,Osh,OrigSw,Oy,Uh,N;public string Id;public Dictionary<(int,int),(int,int)>Map;}
    }
}
