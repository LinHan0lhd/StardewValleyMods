using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.ContentManagers;
using StardewValley;
using xTile;

namespace CPXnbExporter;
public class ModEntry : Mod
{
    private ModConfig _cfg;
    private ExportOptions _opt;
    private ExportPipeline _pipe;
    private List<CpAssetLoader.CpAssetInfo> _list;
    private int _idx = -1;
    private HashSet<string> _done, _cpSet;
    private enum Phase { Idle, Loading, Wait, Finish }
    private Phase _phase = Phase.Idle;

    public override void Entry(IModHelper h)
    {
        _cfg = h.ReadConfig<ModConfig>();
        _cfg.Validate(Monitor);
        CpAssetLoader.Init(h, Monitor);
        h.Events.GameLoop.ReturnedToTitle += (_,_) => { if(_cfg.AutoExport && _phase==Phase.Idle) Start(new[]{_cfg.AutoPlatform, _cfg.AutoUnpacked?"u":""}); };
        h.Events.GameLoop.UpdateTicked += OnTick;
        h.ConsoleCommands.Add("xnb_export", "导出单个", (_,a) => ExportOne(a));
        h.ConsoleCommands.Add("xnb_export_all", "批量导出", (_,a) => Start(a));
        h.ConsoleCommands.Add("xnb_status", "查看进度", (_,_) => Status());
    }

    void OnTick(object _, UpdateTickedEventArgs e)
    {
        if(_phase==Phase.Idle) return;
        if(_phase==Phase.Wait) { if(_pipe.CheckAllWorkersCompleted()) { _phase=Phase.Finish; Finish(); } return; }
        int n=0;
        while(n<_cfg.PerFrame && _idx+1<_list.Count)
        {
            _idx++;
            if(!Enqueue(_list[_idx])) { _idx--; return; }
            n++;
        }
        if(_idx+1>=_list.Count) { _pipe.CompleteAdding(); _phase=Phase.Wait; }
    }

    void Start(string[] a)
    {
        if(_phase!=Phase.Idle) { Monitor.Log("进行中", LogLevel.Warn); return; }
        _phase=Phase.Loading; _opt=ExportOptions.Parse(a, Path.Combine(Helper.DirectoryPath,"exported"));
        _idx=-1; _done=new(); _pipe=new(_cfg.Workers, _cfg.Queue, Monitor);
        _list=CpAssetLoader.LoadAllCpAssets();
        if(_list.Count==0) { _pipe.CompleteAdding(); _phase=Phase.Wait; return; }
        _cpSet=new(_list.Select(x=>Norm(x.AssetName)), StringComparer.OrdinalIgnoreCase);
        Monitor.Log($"导出 {_list.Count} 个", LogLevel.Info);
    }

    bool Enqueue(CpAssetLoader.CpAssetInfo a)
    {
        string raw=a.AssetName, norm=Norm(raw), safe=Sanitize(GetName(norm));
        string pb=Path.Combine(_opt.PackedDir,safe), ub=_opt.Unpacked?Path.Combine(_opt.UnpackedDir,safe):null;
        try
        {
            if(a.AssetType==CpAssetLoader.CpAssetType.Texture) { if(!EnqTex(raw,pb,ub))return false; _done.Add(norm); }
            else if(a.AssetType==CpAssetLoader.CpAssetType.Map) { if(!TryEnqMap(raw,norm,pb,ub))return false; }
            else if(a.AssetType==CpAssetLoader.CpAssetType.Data) { if(!EnqData(raw,pb))return false; _done.Add(norm); }
            else // Unknown — try Texture, then Map, then Data
            {
                if(EnqTex(raw,pb,ub)) _done.Add(norm);
                else if(TryEnqMap(raw,norm,pb,ub)) { }
                else if(EnqData(raw,pb)) _done.Add(norm);
                else Monitor.Log($"✗ 无法加载 {raw}", LogLevel.Warn);
            }
            return true;
        }
        catch(Exception ex)
        {
            Monitor.Log($"✗ 加载失败 {raw}: {ex.Message}", LogLevel.Warn);
            try{if(EnqTex(raw,pb,ub)){_done.Add(norm);return true;}}catch{}
            return true;
        }
    }

    bool TryEnqMap(string raw, string norm, string pb, string ub)
    {
        try
        {
            Map m; string act=raw;
            try{m=Helper.GameContent.Load<Map>(raw);}catch{if(!raw.StartsWith("Maps/")){act="Maps/"+raw;m=Helper.GameContent.Load<Map>(act);}else throw;}
            var host=TileSheetMerger.Merge(m,TileSheetMerger.DefaultHost,Helper,Monitor);
            if(host!=null && !_done.Contains(TileSheetMerger.DefaultHost))
            {
                string h=Sanitize(TileSheetMerger.DefaultHost), hp=Path.Combine(_opt.PackedDir,h), hu=_opt.Unpacked?Path.Combine(_opt.UnpackedDir,h):null;
                if(!EnqTex(host,TileSheetMerger.DefaultHost,hp,hu))return false; _done.Add(TileSheetMerger.DefaultHost);
            }
            TBinWriter.MapAssetName=act;
            var item=new ExportWorkItem{Type=WorkItemType.Map,FileName=raw,PackedBasePath=pb,UnpackedBasePath=ub,Platform=_opt.Platform,TbinData=TBinWriter.SerializeTbin(m)};
            if(!_pipe.TryAdd(item))return false; _done.Add(norm);
            foreach(var ts in m.TileSheets)
            {
                string src=ts.ImageSource; if(string.IsNullOrEmpty(src))continue;
                if(TileSheetMerger.IsVirtual(ts))continue;
                string n=Norm(src); if(!_cpSet.Contains(n)||_done.Contains(n))continue;
                string s=Sanitize(n), pp=Path.Combine(_opt.PackedDir,s), up=_opt.Unpacked?Path.Combine(_opt.UnpackedDir,s):null;
                try{if(EnqTex(src,pp,up))_done.Add(n);}catch{}
            }
            return true;
        }
        catch { return false; }
    }

    // 常见内容目录前缀，用于路径不完整时回退尝试
    static readonly string[] _contentPrefixes = { "Maps/", "LooseSprites/", "TileSheets/", "Characters/", "Buildings/", "Portraits/", "Data/" };

    string ResolveAssetPath(string a)
    {
        // 已经有目录前缀的直接返回
        if(a.Contains('/')) return a;
        // 尝试补常见前缀
        foreach(var p in _contentPrefixes)
        {
            try { if(Helper.GameContent.DoesAssetExist<Texture2D>(Helper.GameContent.ParseAssetName(p+a))) return p+a; } catch{}
            try { if(Helper.GameContent.DoesAssetExist<Map>(Helper.GameContent.ParseAssetName(p+a))) return p+a; } catch{}
        }
        return a;
    }

    bool EnqTex(string a,string pb,string ub)
    {
        string actual = a.Contains('/') ? a : ResolveAssetPath(a);
        // 优先用临时 ContentManager 加载，避免 GameContent 缓存导致内存泄漏
        try
        {
            if(IsLoaded(Game1.content,actual))
                return EnqTex(Helper.GameContent.Load<Texture2D>(actual),actual,pb,ub);
            using var cm=Game1.content.CreateTemporary();
            return EnqTex(cm.Load<Texture2D>(actual),actual,pb,ub);
        }
        catch
        {
            if(actual.StartsWith("Mods/",StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if(IsLoaded(Game1.content,actual))
                        return EnqTex(Helper.GameContent.Load<IRawTextureData>(actual),actual,pb,ub);
                    using var cm2=Game1.content.CreateTemporary();
                    return EnqTex(cm2.Load<IRawTextureData>(actual),actual,pb,ub);
                }
                catch (Exception ex) { Monitor.Log($"✗ 加载纹理失败 {a}: {ex.Message}", LogLevel.Warn); return false; }
            }
            Monitor.Log($"⚠ 跳过内置资源 {a}", LogLevel.Trace);
            return false;
        }
    }
    bool EnqTex(Texture2D t,string fn,string pb,string ub)
    {
        var px=new Color[t.Width*t.Height]; t.GetData(px); XnbWriter.NormalizeAlpha(px);
        byte[] png=null;
        if(ub!=null){using var tmp=new Texture2D(t.GraphicsDevice,t.Width,t.Height);tmp.SetData(px);using var ms=new MemoryStream();tmp.SaveAsPng(ms,t.Width,t.Height);png=ms.ToArray();}
        // 临时加载的纹理立即释放
        if(!IsLoaded(Game1.content,fn)) t.Dispose();
        return _pipe.TryAdd(new ExportWorkItem{Type=WorkItemType.Texture,FileName=fn,PackedBasePath=pb,UnpackedBasePath=ub,Platform=_opt.Platform,PixelData=px,PngData=png,Width=t.Width,Height=t.Height});
    }
    bool EnqTex(IRawTextureData r,string fn,string pb,string ub)
    {
        var px=(Color[])r.Data.Clone(); XnbWriter.NormalizeAlpha(px); byte[] png=null;
        if(ub!=null){using var tmp=new Texture2D(Game1.graphics.GraphicsDevice,r.Width,r.Height);tmp.SetData(px);using var ms=new MemoryStream();tmp.SaveAsPng(ms,r.Width,r.Height);png=ms.ToArray();}
        return _pipe.TryAdd(new ExportWorkItem{Type=WorkItemType.Texture,FileName=fn,PackedBasePath=pb,UnpackedBasePath=ub,Platform=_opt.Platform,PixelData=px,PngData=png,Width=r.Width,Height=r.Height});
    }
    bool EnqData(string a,string pb)
    {
        string actual = a.Contains('/') ? a : ResolveAssetPath(a);
        var types=GetLikely(actual)??new(); types.Add(typeof(object));
        object d=null; foreach(var t in types){try{d=Load(actual,t);if(d!=null)break;}catch{}}
        if(d==null){Monitor.Log($"✗ 无法加载数据 {a}",LogLevel.Warn);return true;}
        string ub=_opt.Unpacked?Path.Combine(_opt.UnpackedDir,Sanitize(GetName(Norm(a)))):null;
        return _pipe.TryAdd(new ExportWorkItem{Type=WorkItemType.Data,FileName=a,PackedBasePath=pb,UnpackedBasePath=ub,Platform=_opt.Platform,DataObject=d,DataTypeName=d.GetType().FullName});
    }

    void ExportOne(string[] a)
    {
        if(a.Length==0){Monitor.Log("用法: xnb_export <asset> [mobile|pc] [u] [type]",LogLevel.Info);return;}
        string asset=a[0], last=a.Length>1?a[^1].ToLower():null;
        bool opt=last is "pc"or"w"or"windows"or"mobile"or"a"or"android"or"i"or"ios"or"unpacked"or"u";
        string type=(!opt&&a.Length>1)?a[^1]:null;
        var args=a.Skip(1).ToArray(); if(type!=null)args=args[..^1];
        var types=TryTypes(type); if(types.Length==0){Monitor.Log($"无类型 '{type}'",LogLevel.Error);return;}
        var opt2=ExportOptions.Parse(args,Path.Combine(Helper.DirectoryPath,"exported"));
        var pts=new List<Type>(types); if(pts[0]==typeof(object)){var l=GetLikely(asset);if(l!=null)pts.InsertRange(0,l);}
        object d=null; foreach(var t in pts){try{d=Load(asset,t);if(d!=null)break;}catch{}}
        if(d==null){Monitor.Log($"无法加载 '{asset}'",LogLevel.Error);return;}
        switch(d){case Map m:ExpMap(asset,m,opt2);break;case Texture2D t:ExpTex(asset,t,opt2);break;case IRawTextureData r:ExpRaw(asset,r,opt2);break;default:ExpData(asset,d,opt2);break;}
    }

    void Status()
    {
        if(_phase==Phase.Idle){Monitor.Log("空闲",LogLevel.Info);return;}
        Monitor.Log($"进度: {_idx+1}/{_list?.Count??0}",LogLevel.Info);
        if(_pipe!=null)Monitor.Log($"T:{_pipe.TexSuccess}/{_pipe.TexFail} M:{_pipe.MapSuccess}/{_pipe.MapFail} D:{_pipe.DataSuccess}/{_pipe.DataFail}",LogLevel.Info);
    }

    void Finish()
    {
        _phase=Phase.Idle;
        long ts=_pipe?.TexSuccess??0,tf=_pipe?.TexFail??0,ms=_pipe?.MapSuccess??0,mf=_pipe?.MapFail??0,ds=_pipe?.DataSuccess??0,df=_pipe?.DataFail??0;
        long total=ts+ms+ds, fail=tf+mf+df, skip=_list==null?0:_list.Count-_idx-1;
        _pipe?.Dispose(); _pipe=null;_list=null;_idx=-1;_done=null;_cpSet=null;TBinWriter.MapAssetName=null;
        Monitor.Log($"完成 总计:{total} 成功 贴图:{ts} 地图:{ms} 数据:{ds} 失败:{fail} 跳过:{skip}",LogLevel.Info);
    }

    bool ExpTex(string a,Texture2D o,ExportOptions opt)
    {
        Texture2D c=null; try{c=Clone(o);XnbWriter.ExportTextureSet(Path.Combine(opt.PackedDir,Sanitize(GetName(Norm(a)))),opt.Unpacked?Path.Combine(opt.UnpackedDir,Sanitize(GetName(Norm(a)))):null,c,opt.Platform);Monitor.Log($"✓ {a}",LogLevel.Info);return true;}
        catch(Exception ex){Monitor.Log($"✗ {a}: {ex.Message}",LogLevel.Warn);return false;}finally{c?.Dispose();}
    }
    bool ExpRaw(string a,IRawTextureData r,ExportOptions opt)
    {
        try{using var t=new Texture2D(Game1.graphics.GraphicsDevice,r.Width,r.Height);t.SetData(r.Data);XnbWriter.ExportTextureSet(Path.Combine(opt.PackedDir,Sanitize(GetName(Norm(a)))),opt.Unpacked?Path.Combine(opt.UnpackedDir,Sanitize(GetName(Norm(a)))):null,t,opt.Platform);Monitor.Log($"✓ {a}",LogLevel.Info);return true;}
        catch(Exception ex){Monitor.Log($"✗ {a}: {ex.Message}",LogLevel.Warn);return false;}
    }
    bool ExpMap(string a,Map m,ExportOptions opt)
    {
        try{string n=Norm(a),s=Sanitize(GetName(n)),p=Path.Combine(opt.PackedDir,s+".xnb");Directory.CreateDirectory(Path.GetDirectoryName(p));TBinWriter.MapAssetName=n;using(var fs=new FileStream(p,FileMode.Create,FileAccess.Write))TBinWriter.WriteMapXnb(fs,m,opt.Platform);if(opt.Unpacked){string u=Path.Combine(opt.UnpackedDir,s+".tbin");Directory.CreateDirectory(Path.GetDirectoryName(u));using(var fs=new FileStream(u,FileMode.Create,FileAccess.Write))TBinWriter.WriteMapTbin(fs,m);long fsz=new FileInfo(u).Length;File.WriteAllText(Path.Combine(opt.UnpackedDir,s+".config"),XnbMetadata.MapConfig(opt.Platform,fsz),System.Text.Encoding.UTF8);}Monitor.Log($"✓ {a}",LogLevel.Info);return true;}
        catch(Exception ex){Monitor.Log($"✗ {a}: {ex.Message}",LogLevel.Warn);return false;}
    }
    bool ExpData(string a,object d,ExportOptions opt)
    {
        try{string p=Path.Combine(opt.PackedDir,Sanitize(GetName(Norm(a))));Directory.CreateDirectory(Path.GetDirectoryName(p));if(DataExporter.ExportData(p,d,a)){if(opt.Unpacked){string u=Path.Combine(opt.UnpackedDir,Sanitize(GetName(Norm(a))));Directory.CreateDirectory(Path.GetDirectoryName(u));string src=p+".json";string dst=u+".json";File.Copy(src,dst,true);long fsz=new FileInfo(dst).Length;File.WriteAllText(u+".config",XnbMetadata.DataConfig(d.GetType().FullName,a,fsz),System.Text.Encoding.UTF8);}Monitor.Log($"✓ {a}",LogLevel.Info);return true;}return false;}
        catch(Exception ex){Monitor.Log($"✗ {a}: {ex.Message}",LogLevel.Warn);return false;}
    }

    static string Norm(string a)
    {
        if(string.IsNullOrEmpty(a))return a;
        a=a.Replace('\\','/');
        if(a.StartsWith("SMAPI/",StringComparison.OrdinalIgnoreCase))
        {
            string r=a[6..]; int i=r.IndexOf('/');
            if(i>=0){string m=r[..i],x=r[(i+1)..];if(x.StartsWith("assets/",StringComparison.OrdinalIgnoreCase))x=x[7..];return"Maps/Mods/"+m+"/"+x;}
            return"Maps/Mods/"+r;
        }
        return a;
    }
    static string Sanitize(string a){if(string.IsNullOrEmpty(a))return a;var s=a.Replace('\\','/').Split('/');for(int i=0;i<s.Length;i++)s[i]=string.Join("_",s[i].Split(Path.GetInvalidFileNameChars()));return string.Join(Path.DirectorySeparatorChar.ToString(),s);}
    string GetName(string a)
    {
        string l=LocalizedContentManager.CurrentLanguageString;
        if(string.IsNullOrEmpty(l))return a;
        string x=a+"."+l;
        bool m=a.StartsWith("Maps/",StringComparison.OrdinalIgnoreCase);
        try
        {
            if(m) return Helper.GameContent.DoesAssetExist<Map>(Helper.GameContent.ParseAssetName(x))?x:a;
            return Helper.GameContent.DoesAssetExist<Texture2D>(Helper.GameContent.ParseAssetName(x))?x:a;
        }
        catch{return a;}
    }

    object Load(string a,Type t)=>GetType().GetMethod(nameof(LoadImpl),BindingFlags.NonPublic|BindingFlags.Instance)!.MakeGenericMethod(t).Invoke(this,new object[]{a});
    T LoadImpl<T>(string a){if(IsLoaded(Game1.content,a))return Game1.content.Load<T>(a);using var cm=Game1.content.CreateTemporary();return cm.Load<T>(a);}
    bool IsLoaded(ContentManager cm,string a){var m=cm.GetType().GetMethod("IsLoaded",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);if(m==null)return false;try{return(bool)m.Invoke(cm,new object[]{Helper.GameContent.ParseAssetName(a)});}catch{return false;}}

    Type[] TryTypes(string n)
    {
        if(string.IsNullOrWhiteSpace(n))return new[]{typeof(object)};
        if(n.Equals("image",StringComparison.OrdinalIgnoreCase))return new[]{typeof(Texture2D)};
        if(n.Equals("map",StringComparison.OrdinalIgnoreCase))return new[]{typeof(Map)};
        var t=Type.GetType(n);if(t!=null)return new[]{t};
        HashSet<Type> bn=new HashSet<Type>(),bf=new HashSet<Type>();
        foreach(var asm in AppDomain.CurrentDomain.GetAssemblies()){if(asm.IsDynamic)continue;foreach(var ty in asm.GetExportedTypes()){try{if(string.Equals(ty.FullName,n,StringComparison.OrdinalIgnoreCase))bf.Add(ty);if(string.Equals(ty.Name,n,StringComparison.OrdinalIgnoreCase))bn.Add(ty);}catch{}}}
        var m=bf.Any()?bf:bn;return m.OrderBy(p=>p.FullName,StringComparer.OrdinalIgnoreCase).ToArray();
    }
    List<Type> GetLikely(string a)
    {
        var n=Helper.GameContent.ParseAssetName(a);
        if(n.IsDirectlyUnderPath("Maps"))return new(){typeof(Map),typeof(Texture2D)};
        if(n.IsDirectlyUnderPath("Animals")||n.IsDirectlyUnderPath("Buildings")||n.IsDirectlyUnderPath("Characters")||n.IsDirectlyUnderPath("Portraits")||n.IsDirectlyUnderPath("Minigames")||n.IsDirectlyUnderPath("TerrainFeatures")||n.IsDirectlyUnderPath("TileSheets"))return new(){typeof(Texture2D)};
        if(n.IsDirectlyUnderPath("Characters/Dialogue")||n.IsDirectlyUnderPath("Characters/schedules")||n.IsDirectlyUnderPath("Data/Events")||n.IsDirectlyUnderPath("Data/Festivals"))return new(){typeof(Dictionary<string,string>)};
        if(n.IsDirectlyUnderPath("Data")){string x=n.BaseName["Data/".Length..];if(x.Contains('_'))return null;var m=typeof(DataLoader).GetMethod(x,BindingFlags.Public|BindingFlags.Static|BindingFlags.IgnoreCase);if(m!=null)return new(){m.ReturnType};}
        return null;
    }
    Texture2D Clone(Texture2D s){var d=new Color[s.Width*s.Height];s.GetData(d);var c=new Texture2D(Game1.graphics.GraphicsDevice,s.Width,s.Height);c.SetData(d);return c;}
}
