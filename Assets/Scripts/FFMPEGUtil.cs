using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = UnityEngine.Debug;
using MediaInfoLib;

public class VideoInfo 
{
    private const string SP = "\t----hhw----\t";
    
    public string Name { get; private set; }
    public int Bitrate { get; set; }
    public long Duration { get; private set; }
    public string Path { get; private set; }
    public int Width { get; private set; }
    public int Heigth { get; private set; }

    public VideoInfo()
    {
    }
    
    public VideoInfo(string path, string name, MediaInfo mi)
    {
        Path = path;
        Name = name;
        int.TryParse(mi.Get(StreamKind.Video, 0, "Width"), out int i);
        Width = i;
        int.TryParse(mi.Get(StreamKind.Video, 0, "Height"), out int j);
        Heigth = j;
        long.TryParse(mi.Get(StreamKind.Video, 0, "Duration"), out long k);
        Duration = k;
        int.TryParse(mi.Get(StreamKind.Video, 0, "BitRate"), out int m);
        Bitrate = m;
    }

    public string ToTableStr()
    {
        return $"{Name}{SP}{Bitrate}{SP}{Duration}{SP}{Path}{SP}{Width}{SP}{Heigth}";
    }

    public static VideoInfo ParseTableStr(string str)
    {
        var ss = str.Split(SP);
        return new VideoInfo()
        {
            Name = ss[0],
            Bitrate = int.Parse(ss[1]),
            Duration = long.Parse(ss[2]),
            Path = ss[3],
            Width = int.Parse(ss[4]),
            Heigth = int.Parse(ss[5]),
        };
    }
}

public class MP4
{
    private const string SP = "\t----hhw----\t";
    
    public string Name;
    public string Pre;
    public string Sub;
    public string Path;
    public string RealName;

    public bool IsMatch = false;
    
    public MP4()
    {
    }
    
    public MP4(string str, string path = null)
    {
        Path = path;

        if (!string.IsNullOrEmpty(path))
        {
            int index = path.LastIndexOf('\\');
            RealName = path.Substring(index + 1, path.Length - index - 1);
        }

        List<Regex> list = new List<Regex>()
        {
            new Regex("[a-zA-Z0-9]{2,}-[0-9]{2,}"),
            new Regex("[a-zA-Z0-9]{2,}_[0-9]{2,}"),
            new Regex("[a-zA-Z0-9]{2,} [0-9]{2,}"),
            new Regex("[0-9]{6,}"),
            new Regex("[a-zA-Z]{2,}[0-9]{2,}"),
        };

        List<char> sps = new List<char>()
        {
            '-',
            '_',
            ' ',
        };

        for (int i = 0; i < list.Count; i++)
        {
            var match = list[i].Match(str);
        
            if (match.Success)
            {
                if (i < sps.Count)
                {
                    Name = match.Value.ToLower();
                    var ss = Name.Split(sps[i]);
                    Pre = ss[0];
                    Sub = ss[1];

                    if (int.TryParse(Pre, out i))
                    {
                        Name = Pre;
                        Sub = Pre;
                    }
                }
                else
                {
                    if (i == list.Count - 1)
                    {
                        var reg1 = new Regex("[a-zA-Z]{2,}");
                        var reg2 = new Regex("[0-9]{2,}");
                        Name = match.Value.ToLower();
                        Pre = reg1.Match(Name).Value;
                        Sub = reg2.Match(Name).Value;
                    }
                    else
                    {
                        Name = match.Value;
                        Pre = Name;
                        Sub = Name;
                    }
                }
                IsMatch = true;
                break;
            }
        }

        if (!IsMatch)
        {
            Name = str;
            Pre = "";
            Sub = "";
            Debug.LogError(Name);
        }
    }
    
    public string ToTableStr()
    {
        return $"{Name}{SP}{Pre}{SP}{Sub}{SP}{Path}";
    }

    public static MP4 ParseTableStr(string str)
    {
        var ss = str.Split(SP);
        var mp4 = new MP4()
        {
            Name = ss[0],
            Pre = ss[1],
            Sub = ss[2],
            Path = ss[3],
        };
        mp4.IsMatch = !string.IsNullOrEmpty(mp4.Pre);
        
        if (!string.IsNullOrEmpty(mp4.Path))
        {
            int index = mp4.Path.LastIndexOf('\\');
            mp4.RealName = mp4.Path.Substring(index + 1, mp4.Path.Length - index - 1);
        }
        
        return mp4;
    }
}

public enum TestType
{
    Pass,
    NotMatch,
    SameName,
    SamePre,
    SameSub,
}

public class FFMPEGUtil : MonoBehaviour
{
    public static FFMPEGUtil Instance;

    private const long MIN_LEN = 50L * 1024L * 1024L;
    private const long MAX_BIT = 2500000;
    private const long MAX_BIT_L = 1150000;

    public Dictionary<string, VideoInfo> VideoInfos = new Dictionary<string, VideoInfo>();

    public Dictionary<string, Dictionary<string, VideoInfo>> SameNameVideoInfos =
        new Dictionary<string, Dictionary<string, VideoInfo>>();

    private static VideoInfo GetInfo(string path, string name)
    {
        var mi = new MediaInfo();
        mi.Open($"{path}\\{name}");
        var videoInfo = new VideoInfo(path, name, mi);
        mi.Close();
        return videoInfo;
    }

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ReadHistory();
    }

    private void TryAddVideo(VideoInfo videoInfo)
    {
        var path = $"{videoInfo.Path}\\{videoInfo.Name}";

        if (!File.Exists(path))
        {
            return;
        }

        if (VideoInfos.ContainsKey(videoInfo.Name))
        {
            if (SameNameVideoInfos.ContainsKey(videoInfo.Name))
            {
                if (!SameNameVideoInfos[videoInfo.Name].ContainsKey(videoInfo.Path))
                {
                    SameNameVideoInfos[videoInfo.Name].Add(videoInfo.Path, videoInfo);
                }
                else
                {
                    Debug.LogError($"同路径文件：{path}");
                }
            }
            else
            {
                if (!videoInfo.Path.Equals(VideoInfos[videoInfo.Name].Path))
                {
                    Dictionary<string, VideoInfo> a = new Dictionary<string, VideoInfo>();
                    a.Add(videoInfo.Path, videoInfo);
                    a.Add(VideoInfos[videoInfo.Name].Path, VideoInfos[videoInfo.Name]);
                    SameNameVideoInfos.Add(videoInfo.Name, a);
                }
                else
                {
                    Debug.LogError($"同路径文件：{path}");
                }
            }
        }
        else
        {
            VideoInfos.Add(videoInfo.Name, videoInfo);
        }
    }

    private bool IsVideo(string str)
    {
        return str.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".mov", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".avi", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".wmv", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".rmvb", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".flv", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".3gp", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".mpg", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".mts", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".vob", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".m4v", StringComparison.OrdinalIgnoreCase);
    }

    public static void OpenFolder(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath)) return;

        folderPath = folderPath.Replace("/", "\\");
        Process process = new Process();
        ProcessStartInfo psi = new ProcessStartInfo("Explorer.exe");
        psi.Arguments = folderPath;
        process.StartInfo = psi;

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            process?.Close();
        }
    }

    public static void OpenFolderAndSelectedFile(string filePathAndName)
    {
        if (string.IsNullOrEmpty(filePathAndName)) return;

        filePathAndName = filePathAndName.Replace("/", "\\");
        Process process = new Process();
        ProcessStartInfo psi = new ProcessStartInfo("Explorer.exe");
        psi.Arguments = "/e,/select," + filePathAndName;
        process.StartInfo = psi;

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            process?.Close();
        }
    }

    public void Load(string path)
    {
        var reader = File.OpenText($"Assets/Resources/{path}.txt");

        while (!reader.EndOfStream)
        {
            var info = VideoInfo.ParseTableStr(reader.ReadLine());
            TryAddVideo(info);
        }

        reader.Close();
        reader.Dispose();
        UpdateUI();
        Debug.LogError($"加载成功：{path}");
    }

    private void UpdateUI()
    {
        var list = VideoInfos.Values.ToList();
        Sort(list);
        UI.Instance.ShowVideo(list);
    }

    public void Scan(string path)
    {
        StartCoroutine(ScanPath(path));
    }

    public void Sort(List<VideoInfo> videos)
    {
        videos.Sort((a, b) =>
        {
            if (b.Bitrate < 1000 || b.Duration < 1000 || b.Width < 100)
            {
                return 1;
            }

            if (b.Width < 1920 && b.Bitrate > MAX_BIT_L)
            {
                return 1;
            }

            int c = b.Bitrate.CompareTo(a.Bitrate);

            if (c == 0)
            {
                return b.Duration.CompareTo(a.Duration);
            }

            return c;
        });
    }

    private IEnumerator ScanPath(string path)
    {
        var drive = new DirectoryInfo($"{path}:\\");
        Queue<DirectoryInfo> allDir = new Queue<DirectoryInfo>();
        List<FileInfo> allFile = new List<FileInfo>();

        foreach (var file in drive.GetDirectories())
        {
            if (!file.FullName.Contains("$")
                && !file.FullName.Contains("System")
                && !file.FullName.Contains("gradle")
                && !file.FullName.Contains("My proj")
                && !file.FullName.Contains("proj")
                && !file.FullName.Contains("SDK")
                && !file.FullName.Contains("SteamLibrary")
                && !file.FullName.Contains("miHoYo")
                && !file.FullName.Contains("found.000"))
            {
                allDir.Enqueue(file);
            }
        }

        foreach (var file in drive.GetFiles())
        {
            if (file.Length > MIN_LEN && IsVideo(file.FullName))
            {
                allFile.Add(file);
            }
        }

        yield return null;
        int total = 0;
        int count = 100;

        while (allDir.Count > 0)
        {
            var dir = allDir.Dequeue();

            foreach (var file in dir.GetDirectories())
            {
                allDir.Enqueue(file);
                total++;
            }

            foreach (var file in dir.GetFiles())
            {
                if (file.Length > MIN_LEN && IsVideo(file.FullName))
                {
                    allFile.Add(file);
                }

                total++;
            }

            Debug.Log($"扫描中：{dir.FullName}");

            if (total > count)
            {
                count += 100;
                Debug.LogError($"扫描中：{dir.FullName}");
                yield return null;
            }
        }

        yield return null;
        List<VideoInfo> videos = new List<VideoInfo>();

        foreach (var file in allFile)
        {
            var info = GetInfo(file.Directory.FullName, file.Name);
            videos.Add(info);
            TryAddVideo(info);
            Debug.Log($"扫描中：{file.Name}");
            yield return null;
        }

        Sort(videos);

        StreamWriter sw;
        FileInfo fi = new FileInfo($"Assets/Resources/{path}.txt");
        sw = fi.CreateText();

        foreach (var video in videos)
        {
            sw.WriteLine(video.ToTableStr());
        }

        sw.Close();
        sw.Dispose();
        Debug.LogError($"扫描成功：{path}");
        yield return null;
        UpdateUI();
    }

    public void TestDir(string path)
    {
        StartCoroutine(TestDirCo(path));
    }

    private IEnumerator TestDirCo(string path)
    {
        var drive = new DirectoryInfo($"{path}");
        Queue<DirectoryInfo> allDir = new Queue<DirectoryInfo>();
        List<FileInfo> allFile = new List<FileInfo>();

        foreach (var file in drive.GetDirectories())
        {
            if (!file.FullName.Contains("$")
                && !file.FullName.Contains("System")
                && !file.FullName.Contains("gradle")
                && !file.FullName.Contains("My proj")
                && !file.FullName.Contains("proj")
                && !file.FullName.Contains("SDK")
                && !file.FullName.Contains("SteamLibrary")
                && !file.FullName.Contains("miHoYo")
                && !file.FullName.Contains("found.000"))
            {
                allDir.Enqueue(file);
            }
        }

        foreach (var file in drive.GetFiles())
        {
            if (IsVideo(file.FullName))
            {
                allFile.Add(file);
            }
        }

        yield return null;
        int total = 0;
        int count = 100;

        while (allDir.Count > 0)
        {
            var dir = allDir.Dequeue();

            foreach (var file in dir.GetDirectories())
            {
                allDir.Enqueue(file);
                total++;
            }

            foreach (var file in dir.GetFiles())
            {
                if (IsVideo(file.FullName))
                {
                    allFile.Add(file);
                }

                total++;
            }

            Debug.Log($"扫描中：{dir.FullName}");

            if (total > count)
            {
                count += 100;
                Debug.Log($"扫描中：{dir.FullName}");
                yield return null;
            }
        }

        yield return null;

        foreach (var file in allFile)
        {
            var type = TestLink(file.FullName, out MP4 mp4);
            
            switch (type)
            {
                case TestType.SameName:
                    Debug.LogError($"同名文件：{file.FullName}");
                    break;
                case TestType.SameSub:
                    Debug.LogWarning($"同ID文件：{file.FullName}");
                    break;
            }
        }
        
        yield return null;
    }

    public string OutPutPath;
    public string MovePath;
    public bool UseH265 = true;

    public string Encode(VideoInfo info)
    {
        string name = info.Name;
        name = name.Substring(0, name.LastIndexOf('.'));
        string path = $"{OutPutPath}\\{name}.mp4";

        if (File.Exists(path))
        {
            Debug.LogError($"{path}已存在，请删除！");
            return "";
        }

        string scale = info.Width > 1920 ? ",scale=1920:1080" : "";
        string order =
            $"ffmpeg -i \"{info.Path}\\{info.Name}\" -vf fps=30{scale} -c:v libx264 -preset medium -c:a aac \"{OutPutPath}\\{name}.mp4\"";
        order = order.Replace("\\", "/");
        return order;
    }

    public string Encode2(VideoInfo info)
    {
        string name = info.Name;
        name = name.Substring(0, name.LastIndexOf('.'));
        string path = $"{OutPutPath}\\{name}.mp4";

        if (File.Exists(path))
        {
            Debug.LogError($"{path}已存在，请删除！");
            return "";
        }

        string scale = info.Width > 1920 ? ",scale=1920:1080" : "";
        string order =
            $"ffmpeg -i \"{info.Path}\\{info.Name}\" -vf fps=30{scale} -c:v libx265 -preset medium -c:a aac \"{OutPutPath}\\{name}.mp4\"";
        order = order.Replace("\\", "/");
        return order;
    }

    public string Move2(VideoInfo info)
    {
        string order = $"mv \"{info.Path}\\{info.Name}\" \"{MovePath}\\\"";
        order = order.Replace("\\", "/");
        return order;
    }

    public string Del(VideoInfo info)
    {
        string order = $"rm \"{info.Path}\\{info.Name}\"";
        order = order.Replace("\\", "/");
        return order;
    }

    public void GenSH(int max)
    {
        var list = VideoInfos.Values.ToList();
        Sort(list);

        max = Mathf.Min(max, list.Count);

        StreamWriter sw;
        FileInfo fi = new FileInfo($"Assets/Resources/encode.sh");
        sw = fi.CreateText();

        for (int i = 0; i < max; i++)
        {
            if ((list[i].Bitrate < 1000 || list[i].Duration < 1000 || list[i].Width < 100)
                || (list[i].Width < 1920 && list[i].Bitrate > MAX_BIT_L)
                || (list[i].Width >= 1920 && list[i].Bitrate > MAX_BIT))
            {
                var str = UseH265 ? Encode2(list[i]) : Encode(list[i]);

                if (!string.IsNullOrEmpty(str))
                {
                    sw.WriteLine(str);
                }
            }
        }

        sw.Close();
        sw.Dispose();
        Debug.LogError("sh生成成功");
    }

    public void GenMoveSH(int max)
    {
        var list = VideoInfos.Values.ToList();
        Sort(list);

        max = Mathf.Min(max, list.Count);

        StreamWriter sw;
        FileInfo fi = new FileInfo($"Assets/Resources/move.sh");
        sw = fi.CreateText();

        for (int i = 0; i < max; i++)
        {
            if ((list[i].Bitrate < 1000 || list[i].Duration < 1000 || list[i].Width < 100)
                || (list[i].Width < 1920 && list[i].Bitrate > MAX_BIT_L)
                || (list[i].Width >= 1920 && list[i].Bitrate > MAX_BIT))
            {
                sw.WriteLine(Move2(list[i]));
            }
        }

        sw.Close();
        sw.Dispose();
        Debug.LogError("sh生成成功");
    }

    public IEnumerator GenDelSH()
    {
        StreamWriter sw;
        FileInfo fi = new FileInfo($"Assets/Resources/del.sh");
        sw = fi.CreateText();

        DirectoryInfo directoryInfo = new DirectoryInfo(OutPutPath);

        foreach (var file in directoryInfo.GetFiles())
        {
            if (VideoInfos.ContainsKey(file.Name))
            {
                var path = $"{VideoInfos[file.Name].Path}\\{VideoInfos[file.Name].Name}";

                if (File.Exists(path))
                {
                    var videoInfo = GetInfo(directoryInfo.FullName, file.Name);

                    if (videoInfo != null)
                    {
                        var d = videoInfo.Duration - VideoInfos[file.Name].Duration;

                        if (d > 1000 || d < -1000)
                        {
                            Debug.LogError($"时长不对：{directoryInfo.FullName}\\{file.Name}");
                        }
                        else
                        {
                            sw.WriteLine(Del(VideoInfos[file.Name]));
                        }
                    }
                    else
                    {
                        Debug.LogError($"无法解析：{directoryInfo.FullName}\\{file.Name}");
                    }

                    yield return null;
                }
            }
        }

        sw.Close();
        sw.Dispose();
        Debug.LogError("sh生成成功");
    }

    public List<MP4> Mp4s = new List<MP4>();
    public Dictionary<string, List<MP4>> Total = new Dictionary<string, List<MP4>>();
    public Dictionary<string, List<MP4>> Pre = new Dictionary<string, List<MP4>>();
    public Dictionary<string, List<MP4>> Sub = new Dictionary<string, List<MP4>>();
    public Dictionary<string, List<MP4>> Non = new Dictionary<string, List<MP4>>();

    public void ClearMp4s()
    {
        Mp4s.Clear();
        Total.Clear();
        Pre.Clear();
        Sub.Clear();
        Non.Clear();
    }

    private void AddMp4ToDict(MP4 mp4, string key, Dictionary<string, List<MP4>> dict)
    {
        if (dict.TryGetValue(key, out var list))
        {
            list.Add(mp4);
        }
        else
        {
            dict.Add(key, new List<MP4> {mp4});
        }
    }

    public void AddMp4(MP4 mp4)
    {
        Mp4s.Add(mp4);
        AddMp4ToDict(mp4, mp4.Name, Total);

        if (mp4.IsMatch)
        {
            AddMp4ToDict(mp4, mp4.Pre, Pre);
            AddMp4ToDict(mp4, mp4.Sub, Sub);
        }
        else
        {
            AddMp4ToDict(mp4, mp4.Name, Non);
        }
    }

    public void ReadTotal()
    {
        ClearMp4s();

        var txt = Resources.Load<TextAsset>("total").text;
        txt = txt.Replace("\r", "");
        var ss = txt.Split("\n");

        foreach (var s in ss)
        {
            if (!string.IsNullOrEmpty(s))
            {
                var mp4 = MP4.ParseTableStr(s);
                AddMp4(mp4);
            }
        }

        return;

        var reader = File.OpenText("Assets/Resources/total.txt");

        while (!reader.EndOfStream)
        {
            var mp4 = MP4.ParseTableStr(reader.ReadLine());
            AddMp4(mp4);
        }

        reader.Close();
        reader.Dispose();
    }

    public void GenTotal()
    {
        ClearMp4s();

        for (int i = 0; i < 26; i++)
        {
            char a = (char) ('A' + i);
            AddToTotal(a.ToString());
        }

        ExportTotal();
    }

    public void AddToTotal(string tb)
    {
        var path = $"Assets/Resources/{tb}.txt";

        if (!File.Exists(path))
        {
            return;
        }

        var reader = File.OpenText(path);

        while (!reader.EndOfStream)
        {
            var video = VideoInfo.ParseTableStr(reader.ReadLine());
            var mp4 = new MP4(video.Name, $"{video.Path}\\{video.Name}");
            AddMp4(mp4);
        }

        reader.Close();
        reader.Dispose();
    }

    public void ExportTotal()
    {
        StreamWriter sw;
        FileInfo fi = new FileInfo("Assets/Resources/total.txt");
        sw = fi.CreateText();

        foreach (var mp4 in Mp4s)
        {
            sw.WriteLine(mp4.ToTableStr());
        }

        sw.Close();
        sw.Dispose();
        Debug.LogError("导出成功！");
    }

    public TestType TestLink(string link, out MP4 mp4)
    {
        mp4 = new MP4(link);

        if (mp4.IsMatch)
        {
            if (Total.ContainsKey(mp4.Name))
            {
                return TestType.SameName;
            }
            
            if (Sub.ContainsKey(mp4.Sub))
            {
                foreach (var t in Sub[mp4.Sub])
                {
                    if (t.Pre.Equals(mp4.Pre))
                    {
                        return TestType.SameName;
                    }
                }
                
                if (int.TryParse(mp4.Sub, out int i))
                {
                    if (mp4.Sub.Length >= 5)
                    {
                        return TestType.SameSub;
                    }
                }
            }
            
            if (Pre.ContainsKey(mp4.Pre))
            {
                return TestType.SamePre;
            }

            return TestType.Pass;
        }

        return TestType.NotMatch;
    }

    private Dictionary<string, int> History = new Dictionary<string, int>();

    private void ReadHistory()
    {
        var path = "Assets/Resources/history.txt";
        
        if (!File.Exists(path))
        {
            var sw = File.CreateText(path);
            sw.Close();
            sw.Dispose();
        }
        
        var reader = File.OpenText(path);

        while (!reader.EndOfStream)
        {
            var link = reader.ReadLine();
            
            if (string.IsNullOrEmpty(link))
            {
                continue;
            }
            
            link = link.Trim();
            History.Add(link, 1);
        }
        
        reader.Close();
        reader.Dispose();
    }

public void TestFile(string tb)
    {
        var reader = File.OpenText($"Assets/Resources/{tb}.txt");
        
        var history = new FileInfo("Assets/Resources/history.txt").AppendText();
        var sw = new FileInfo($"Assets/Resources/{tb}_p.txt").CreateText();
        var sw_n = new FileInfo($"Assets/Resources/{tb}_n.txt").CreateText();
        var sw_w = new FileInfo($"Assets/Resources/{tb}_w.txt").CreateText();

        Dictionary<string, List<MP4>> cache = new Dictionary<string, List<MP4>>();

        while (!reader.EndOfStream)
        {
            var link = reader.ReadLine();

            if (string.IsNullOrEmpty(link))
            {
                continue;
            }

            link = link.Trim();

            if (History.ContainsKey(link))
            {
                continue;
            }
            
            History.Add(link, 1);
            history.WriteLine(link);
            var type = TestLink(link, out MP4 mp4);

            if (cache.TryGetValue(mp4.Sub, out var list))
            {
                bool flag = true;
                
                foreach (var v in list)
                {
                    if (v.Pre.Equals(mp4.Pre))
                    {
                        sw_w.WriteLine(link);
                        Debug.LogError($"可能重复的链接：{link}");
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    list.Add(mp4);
                }
                else
                {
                    continue;
                }
            }
            else
            {
                cache.Add(mp4.Sub, new List<MP4>(){mp4});
            }
            
            switch (type)
            {
                case TestType.Pass:
                    sw.WriteLine(link);
                    break;
                case TestType.NotMatch:
                    sw_n.WriteLine(link);
                    break;
                case TestType.SameName:
                    sw_w.WriteLine(link);
                    break;
                case TestType.SamePre:
                    sw.WriteLine(link);
                    break;
                case TestType.SameSub:
                    sw_w.WriteLine(link);
                    break;
            }
        }

        reader.Close();
        reader.Dispose();
        history.Close();
        history.Dispose();
        sw.Close();
        sw.Dispose();
        sw_n.Close();
        sw_n.Dispose();
        sw_w.Close();
        sw_w.Dispose();
        Debug.LogError("校验成功！");
    }

    public List<List<MP4>> GetTotalList()
    {
        List<List<MP4>> list = new List<List<MP4>>();

        foreach (var l in Total.Values)
        {
            list.Add(l);
        }

        list.Sort((a, b) => b.Count.CompareTo(a.Count));
        return list;
    }
    
    public List<List<MP4>> GetSameNameList()
    {
        List<List<MP4>> list = new List<List<MP4>>();

        foreach (var l in Total.Values)
        {
            if (l.Count > 1)
            {
                list.Add(l);
            }
        }

        list.Sort((a, b) => b.Count.CompareTo(a.Count));
        return list;
    }
    
    public List<List<MP4>> GetPreList()
    {
        List<List<MP4>> list = new List<List<MP4>>();

        foreach (var l in Pre.Values)
        {
            if (l.Count > 1)
            {
                list.Add(l);
            }
        }

        list.Sort((a, b) => b.Count.CompareTo(a.Count));
        return list;
    }
    
    public List<List<MP4>> GetSubList()
    {
        List<List<MP4>> list = new List<List<MP4>>();

        foreach (var l in Sub.Values)
        {
            if (l.Count > 1)
            {
                list.Add(l);
            }
        }

        list.Sort((a, b) =>
        {
            int x = b[0].Sub.Length.CompareTo(a[0].Sub.Length);
            
            if (a[0].Sub.Length > 7 || b[0].Sub.Length > 7)
            {
                return -x;
            }

            if (x == 0)
            {
                return b.Count.CompareTo(a.Count);
            }
            
            return x;
        });
        return list;
    }
    
    public List<List<MP4>> GetNonList()
    {
        List<List<MP4>> list = new List<List<MP4>>();

        foreach (var l in Non.Values)
        {
            list.Add(l);
        }

        list.Sort((a, b) => b.Count.CompareTo(a.Count));
        return list;
    }

    public List<MP4> FindMp4(string pre, string sub)
    {
        var a = !string.IsNullOrEmpty(pre);
        var b = !string.IsNullOrEmpty(sub);

        if (a && b)
        {
            if (Pre.TryGetValue(pre, out var list))
            {
                List<MP4> res = new List<MP4>();

                foreach (var mp4 in list)
                {
                    if (mp4.Sub.Equals(sub))
                    {
                        res.Add(mp4);
                    }
                }

                return res;
            }
        }
        else if (a)
        {
            if (Pre.TryGetValue(pre, out var list))
            {
                return list;
            }
        }
        else if (b)
        {
            if (Sub.TryGetValue(sub, out var list))
            {
                return list;
            }
        }

        return null;
    }

    public List<List<MP4>> FindAllMp4(string str)
    {
        List<List<MP4>> list = new List<List<MP4>>();
        str = str.ToLower();

        foreach (var kvp in Total)
        {
            if (kvp.Key.Contains(str))
            {
                list.Add(kvp.Value);
            }
        }

        list.Sort((a, b) => b.Count.CompareTo(a.Count));
        return list;
    }
    
    public string GenFolder(string folder)
    {
        string order = $"mkdir \"{MovePath}\\{folder}\"";
        order = order.Replace("\\", "/");
        return order;
    }
    
    public string MoveMp4(MP4 mp4, string folder)
    {
        string order = $"mv \"{mp4.Path}\" \"{MovePath}\\{folder}\"";
        order = order.Replace("\\", "/");
        return order;
    }
    
    public void GenMoveMp4SH(List<MP4> list)
    {
        StreamWriter sw;
        FileInfo fi = new FileInfo($"Assets/Resources/move_mp4.sh");
        sw = fi.CreateText();
        var folder = list[0].Pre;
        sw.WriteLine(GenFolder(folder));

        foreach (var mp4 in list)
        {
            sw.WriteLine(MoveMp4(mp4, folder));
        }
        
        sw.Close();
        sw.Dispose();
        Debug.LogError("sh生成成功");
    }

    public void GenMoveMp4SH(List<List<MP4>> list)
    {
        StreamWriter sw;
        FileInfo fi = new FileInfo($"Assets/Resources/move_mp4.sh");
        sw = fi.CreateText();

        foreach (var mp4s in list)
        {
            var folder = mp4s[0].Pre;
            sw.WriteLine(GenFolder(folder));

            foreach (var mp4 in mp4s)
            {
                sw.WriteLine(MoveMp4(mp4, folder));
            }
        }
        
        sw.Close();
        sw.Dispose();
        Debug.LogError("sh生成成功");
    }
    
    private const long MIN_IMG_LEN = 4L * 1024L * 1024L;
    
    private bool IsImage(string str)
    {
        return str.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".tga", StringComparison.OrdinalIgnoreCase)
               || str.EndsWith(".tif", StringComparison.OrdinalIgnoreCase);
    }

    public void ScanImage(string path)
    {
        StartCoroutine(ImgScanPath(path));
    }
    
    private IEnumerator ImgScanPath(string path)
    {
        var drive = new DirectoryInfo($"{path}");
        Queue<DirectoryInfo> allDir = new Queue<DirectoryInfo>();
        List<FileInfo> allFile = new List<FileInfo>();

        foreach (var file in drive.GetDirectories())
        {
            if (!file.FullName.Contains("$")
                && !file.FullName.Contains("System")
                && !file.FullName.Contains("gradle")
                && !file.FullName.Contains("My proj")
                && !file.FullName.Contains("proj")
                && !file.FullName.Contains("SDK")
                && !file.FullName.Contains("SteamLibrary")
                && !file.FullName.Contains("miHoYo")
                && !file.FullName.Contains("found.000"))
            {
                allDir.Enqueue(file);
            }
        }

        foreach (var file in drive.GetFiles())
        {
            if (file.Length > MIN_IMG_LEN && IsImage(file.FullName))
            {
                allFile.Add(file);
            }
        }

        yield return null;
        int total = 0;
        int count = 100;

        while (allDir.Count > 0)
        {
            var dir = allDir.Dequeue();

            foreach (var file in dir.GetDirectories())
            {
                allDir.Enqueue(file);
                total++;
            }

            foreach (var file in dir.GetFiles())
            {
                if (file.Length > MIN_IMG_LEN && IsImage(file.FullName))
                {
                    allFile.Add(file);
                }

                total++;
            }

            Debug.Log($"扫描中：{dir.FullName}");

            if (total > count)
            {
                count += 100;
                Debug.LogError($"扫描中：{dir.FullName}");
                yield return null;
            }
        }

        yield return null;

        StreamWriter sw;
        FileInfo fi = new FileInfo($"Assets/Resources/img.sh");
        sw = fi.CreateText();

        foreach (var file in allFile)
        {
            var name = file.Name.Substring(0, file.Name.LastIndexOf('.'));
            sw.WriteLine($"ffmpeg -i \"{file.FullName}\" -q 1 \"{file.Directory.FullName}\\a_{name}.jpg\"".Replace("\\", "/"));
        }

        sw.Close();
        sw.Dispose();
        Debug.LogError($"图片扫描成功：{path}");
        yield return null;
    }
}
