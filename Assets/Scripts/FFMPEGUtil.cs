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

    public bool IsMatch = false;
    
    public MP4()
    {
    }
    
    public MP4(string str)
    {
        var regex = new Regex("[a-zA-Z0-9]+-[0-9]+");
        var match = regex.Match(str);

        if (match.Success)
        {
            Name = match.Value.ToLower();
            var ss = Name.Split('-');
            Pre = ss[0];
            Sub = ss[1];
            IsMatch = true;
        }
        else
        {
            Name = str;
            Pre = "";
            Sub = "";
        }
    }
    
    public string ToTableStr()
    {
        return $"{Name}{SP}{Pre}{SP}{Sub}";
    }

    public static MP4 ParseTableStr(string str)
    {
        var ss = str.Split(SP);
        var mp4 = new MP4()
        {
            Name = ss[0],
            Pre = ss[1],
            Sub = ss[2],
        };
        mp4.IsMatch = string.IsNullOrEmpty(mp4.Pre);
        return mp4;
    }
}

public class FFMPEGUtil : MonoBehaviour
{
    public static FFMPEGUtil Instance;
    
    private const long MIN_LEN = 50L * 1024L * 1024L;
    private const long MAX_BIT = 5000000;
    private const long MAX_BIT_L = 2500000;

    public Dictionary<string, VideoInfo> VideoInfos = new Dictionary<string, VideoInfo>();
    public Dictionary<string, Dictionary<string, VideoInfo>> SameNameVideoInfos = new Dictionary<string, Dictionary<string, VideoInfo>>();

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
        psi.Arguments = "/e,/select,"+filePathAndName;
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
        string order = $"ffmpeg -i \"{info.Path}\\{info.Name}\" -vf fps=30{scale} -c:v libx264 -preset medium -c:a aac \"{OutPutPath}\\{name}.mp4\"";
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
        string order = $"ffmpeg -i \"{info.Path}\\{info.Name}\" -vf fps=30{scale} -c:v libx265 -preset medium -c:a aac \"{OutPutPath}\\{name}.mp4\"";
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
        
        sw.WriteLine("pause");
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
    
    public void GenDelSH()
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
                    sw.WriteLine(Del(VideoInfos[file.Name]));
                }
            }
        }
        
        sw.Close();
        sw.Dispose();
        Debug.LogError("sh生成成功");
    }
}
