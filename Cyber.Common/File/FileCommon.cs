
using Microsoft.AspNetCore.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Cyber
{
    public enum PhysicalPath
    {
        /// <summary>
        /// 文件或文件夹地址可以为空
        /// </summary>
        Ignore,
        /// <summary>
        /// 文件或文件夹必须存在
        /// </summary>
        MustExist

    }

   
    /// <summary>
    /// 文件处理类 
    /// 此方法都为绝对地址
    /// </summary>
    public static class FileCommon
    {
        public static string UploadedDir = "Uploaded";
        /// <summary>
        /// 获取项目路径
        /// </summary>
        /// <returns></returns>
        public static string GetPath(bool wwwroot=true)
        {
            IWebHostEnvironment _webHostEnvironment = GetServiceProvider.GetService<IWebHostEnvironment>();
            return wwwroot?_webHostEnvironment.WebRootPath.IsNullOrEmpty() ? _webHostEnvironment.ContentRootPath + Path.DirectorySeparatorChar : _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar : _webHostEnvironment.ContentRootPath;
        }

       
        /// <summary>
        /// 获取物理路径(默认文件必须存在，不存在返回null)
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <param name="wwwroot">优先去找wwwroot文件夹下的路径</param>
        /// <returns></returns>
        public static string GetPhysicalPath(string relativePath,  bool wwwroot = true)
        {
            return GetPhysicalPath(relativePath, PhysicalPath.MustExist, wwwroot);
        }

        /// <summary>
        /// 获取物理路径
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <param name="physicalPath">文件或文件夹是否必须存在</param>
        /// <param name="wwwroot">优先去找wwwroot文件夹下的路径</param>
        /// <returns></returns>
        public static string GetPhysicalPath(string relativePath, PhysicalPath physicalPath, bool wwwroot=true)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }
            if (relativePath.Contains(":\\")|| Path.IsPathRooted(relativePath))
            {
                return relativePath;
            }
            string ContentRootPath = GetAppPath.Path;
            string WebRootPath = System.IO.Path.Combine(ContentRootPath, "wwwroot");
            if (!Directory.Exists(WebRootPath)&& Debugger.IsAttached)
            {
                if (AppGlobalSettings.RunTimeEunm == RunTimeEunm.DebugWindows || AppGlobalSettings.RunTimeEunm == RunTimeEunm.DebugLinux)
                {
                    ContentRootPath = Directory.GetCurrentDirectory();
                    WebRootPath = System.IO.Path.Combine(ContentRootPath, "wwwroot");
                    if (!Directory.Exists(WebRootPath))
                    {
                        wwwroot = false;
                    }

                 }
                else
                {
                    wwwroot = false;
                }
            }

            if (relativePath.StartsWith("~"))
            {
                relativePath = relativePath.Remove(0, 2);
            }
            string YPATH;
            if (wwwroot)
            {
                YPATH = GetRuntimeDirectory(Path.Combine(WebRootPath, relativePath));
            }
            else {
                YPATH = GetRuntimeDirectory(Path.Combine(ContentRootPath, relativePath));
            }
            if (physicalPath == PhysicalPath.MustExist)
            {
                //WebRootPath不为空就要判断一下了
                var PATH = YPATH;
                var PATH2 = YPATH;
                //如果是文件但没有在wwwroot里面，看看是不是在根目录下
                if (!GetExtension(PATH).IsNullOrEmpty() && !IsExistFile(PATH))
                {
                    PATH = GetRuntimeDirectory(Path.Combine(ContentRootPath, relativePath));
                }
                //判断是否有文件
                if (!GetExtension(PATH).IsNullOrEmpty() && IsExistFile(PATH))
                {
                    return PATH;
                }
                //如果不是文件，文件夹是否存在
                if (GetExtension(PATH).IsNullOrEmpty())
                {
                    if (!Directory.Exists(PATH2))
                    {
                        PATH2 = GetRuntimeDirectory(Path.Combine(ContentRootPath, relativePath));
                    }
                    if (Directory.Exists(PATH2))
                    {
                        return PATH2;
                    }
                }
                return null;
            }
            else
            {
                return YPATH;
            }
            


        }

       

        #region 检测指定目录是否存在
        /// <summary>
        /// 检测指定目录是否存在
        /// </summary>
        /// <param name="directoryPath">目录的绝对路径</param>
        /// <returns></returns>
        public static bool IsExistDirectory(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }
        #endregion
        #region 检测指定文件是否存在,如果存在返回true
        /// <summary>
        /// 检测指定文件是否存在,如果存在则返回true。
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>        
        public static bool IsExistFile(string filePath)
        {
            return File.Exists(filePath);
        }
        #endregion
        #region 获取指定目录中的文件列表
        /// <summary>
        /// 获取指定目录中所有文件列表
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>        
        public static string[] GetFileNames(string directoryPath)
        {
            //如果目录不存在，则抛出异常
            if (!IsExistDirectory(directoryPath))
            {
                throw new FileNotFoundException("directoryPath 不存在");
            }
            //获取文件列表
            return Directory.GetFiles(directoryPath);
        }
        #endregion
        #region 获取指定目录中所有子目录列表,若要搜索嵌套的子目录列表,请使用重载方法.
        /// <summary>
        /// 获取指定目录中所有子目录列表,若要搜索嵌套的子目录列表,请使用重载方法.
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>        
        public static string[] GetDirectories(string directoryPath)
        {
            try
            {
                return Directory.GetDirectories(directoryPath);
            }
            catch
            {
                throw;
            }
        }
        #endregion
        #region 获取指定目录及子目录中所有文件列表
        /// <summary>
        /// 获取指定目录及子目录中所有文件列表
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        /// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。
        /// 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param>
        /// <param name="isSearchChild">是否搜索子目录</param>
        public static string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
        {
            //如果目录不存在，则抛出异常
            if (!IsExistDirectory(directoryPath))
            {
                throw new FileNotFoundException("directoryPath 不存在");
            }
            try
            {
                if (isSearchChild)
                {
                    return Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
                }
                else
                {
                    return Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
                }
            }
            catch
            {
                throw;
            }
        }
        #endregion
        #region 检测指定目录是否为空
        /// <summary>
        /// 检测指定目录是否为空
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>        
        public static bool IsEmptyDirectory(string directoryPath)
        {
            try
            {
                //判断是否存在文件
                string[] fileNames = GetFileNames(directoryPath);
                if (fileNames.Length > 0)
                {
                    return false;
                }
                //判断是否存在文件夹
                string[] directoryNames = GetDirectories(directoryPath);
                if (directoryNames.Length > 0)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                //这里记录日志
                //LogHelper.WriteTraceLog(TraceLogLevel.Error, ex.Message);
                return true;
            }
        }
        #endregion
        #region 检测指定目录中是否存在指定的文件
        /// <summary>
        /// 检测指定目录中是否存在指定的文件,若要搜索子目录请使用重载方法.
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        /// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。
        /// 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param>        
        public static bool Contains(string directoryPath, string searchPattern)
        {
            try
            {
                //获取指定的文件列表
                string[] fileNames = GetFileNames(directoryPath, searchPattern, false);
                //判断指定文件是否存在
                if (fileNames.Length == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                //LogHelper.WriteTraceLog(TraceLogLevel.Error, ex.Message);
            }
        }
        /// <summary>
        /// 检测指定目录中是否存在指定的文件
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        /// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。
        /// 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param> 
        /// <param name="isSearchChild">是否搜索子目录</param>
        public static bool Contains(string directoryPath, string searchPattern, bool isSearchChild)
        {
            try
            {
                //获取指定的文件列表
                string[] fileNames = GetFileNames(directoryPath, searchPattern, true);
                //判断指定文件是否存在
                if (fileNames.Length == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                //LogHelper.WriteTraceLog(TraceLogLevel.Error, ex.Message);
            }
        }
        #endregion
        #region 创建目录
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="dir">要创建的目录路径包括目录名</param>
        public static void CreateDir(string dir)
        {
            if (dir.Length == 0) return;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        #endregion
        #region 删除目录
        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="dir">要删除的目录路径和名称</param>
        public static void DeleteDir(string dir,bool SubDirectory=false)
        {
            if (dir.Length == 0) return;
            if (Directory.Exists(dir))
            {
                if (SubDirectory == true)
                {
                    DeleteDirectory(dir);
                }
                else
                {
                    Directory.Delete(dir);
                }
            }
        }
        #endregion
        #region 删除文件
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="file">要删除的文件路径和名称</param>
        public static void DeleteFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
        /// <summary>
        /// 删除绝对文件
        /// </summary>
        /// <param name="file">要删除的文件路径和名称</param>
        public static void DeletePhysicalFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
        #endregion
        #region 创建文件
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="dir">带后缀的文件名</param>
        /// <param name="pagestr">文件内容</param>
        public static void CreateFile(string dir, string pagestr)
        {
            CreateFile(dir, pagestr, System.Text.Encoding.GetEncoding("GB2312"));
        }
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="dir">带后缀的文件名</param>
        /// <param name="pagestr">文件内容</param>
        /// <param name="encoder">编码</param>
        public static void CreateFile(string dir, string pagestr, Encoding encoder)
        {

            CreateFile(dir, pagestr, encoder, FileCommon.GetPhysicalPath(FileCommon.UploadedDir, PhysicalPath.Ignore));
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="dir">带后缀的文件名</param>
        /// <param name="pagestr">文件内容</param>
        /// <param name="encoder">编码</param>
        ///  <param name="allowedRoot">允许上传的安全路径</param>
        public static void CreateFile(string dir, string pagestr, Encoding encoder,string allowedRoot)
        {


            FileSetting _FileSetting = GetServiceProvider.GetService<FileSetting>();

            if (dir == null || _FileSetting == null || (_FileSetting.FileTypes.IsNullOrEmpty() || !_FileSetting.FileTypes.Contains(FileCommon.GetExtension(dir).ToLower())))
            {
                throw new OutPutException("Upload Error");
            }
            else if (!FileCommon.ValidateAbsolutePath(dir, allowedRoot))
            {
                throw new OutPutException("Upload Path Error");
            }

            //验证文件不能超过最大size
            if (_FileSetting.Size != null && _FileSetting.Size > 0)
            {
                // 计算字符串内容的大小（按编码转换为字节数组）
                byte[] contentBytes = encoder.GetBytes(pagestr);
                long contentSizeKB = contentBytes.Length / 1024; // 内容大小转换为KB

                // 如果内容本身的大小就超过了限制，则直接抛出异常
                if (contentSizeKB > _FileSetting.Size)
                {
                    throw new OutPutException(x =>
                    {
                        x.ChineseSimplified = "文件尺寸过大";
                        x.English = "The file size is too large";
                    });
                }

                // 如果文件已存在，还需考虑现有文件大小
                if (IsExistFile(dir))
                {
                    FileInfo fileInfo = new FileInfo(dir);
                    long totalSizeKB = (fileInfo.Length + contentBytes.Length) / 1024; // 总大小转换为KB

                    if (totalSizeKB > _FileSetting.Size)
                    {
                        throw new OutPutException(x =>
                        {
                            x.ChineseSimplified = "文件尺寸过大";
                            x.English = "The file size is too large";
                        });
                    }
                }
            }

            if (dir.IndexOf(Path.DirectorySeparatorChar) > -1)
                CreateDir(dir.Substring(0, dir.LastIndexOf(Path.DirectorySeparatorChar)));
            else
                CreateDir(dir);

            System.IO.StreamWriter sw = new System.IO.StreamWriter(dir, false, encoder);
            sw.Write(pagestr);
            sw.Close();
        }

        #endregion
        #region 移动文件(剪贴--粘贴)
        /// <summary>
        /// 移动文件(剪贴--粘贴)
        /// </summary>
        /// <param name="dir1">要移动的文件的路径及全名(包括后缀)</param>
        /// <param name="dir2">文件移动到新的位置,并指定新的文件名</param>
        public static void MoveFile(string dir1, string dir2)
        {
            if (AppGlobalSettings.RunTimeEunm == RunTimeEunm.ReleaseWindows || AppGlobalSettings.RunTimeEunm == RunTimeEunm.DebugWindows)
            {
                dir1 = dir1.Replace("/", "\\");
                dir2 = dir2.Replace("/", "\\");
            }
            if (File.Exists(dir1))
                File.Move(dir1,dir2);
        }
        #endregion
        #region 复制文件
        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="dir1">要复制的文件的路径已经全名(包括后缀)</param>
        /// <param name="dir2">目标位置,并指定新的文件名</param>
        public static void CopyFile(string dir1, string dir2)
        {
            if (AppGlobalSettings.RunTimeEunm == RunTimeEunm.ReleaseWindows || AppGlobalSettings.RunTimeEunm == RunTimeEunm.DebugWindows)
            {
                dir1 = dir1.Replace("/", "\\");
                dir2 = dir2.Replace("/", "\\");
            }
            if (File.Exists(dir1))
            {
                File.Copy(dir1, dir2, true);
            }
        }
        #endregion
        #region 获取上传目录名
        /// <summary>
        /// 获取上传目录名,会自动生成文件夹
        /// </summary>
        /// <returns></returns>
        public static string GetUploadDir()
        {
            return GetUploadDir(true, true,null);
        }
        /// <summary>
        /// 获取上传目录名
        /// </summary>
        /// <returns></returns>
        public static string GetUploadDir(Action<GetUpload> action)
        {
            GetUpload getUpload = new GetUpload();
            action(getUpload);
            return GetUploadDir(getUpload.Data, getUpload.CreateDirectory, getUpload.Model);
        }

        /// <summary>
        /// 获取上传目录名,会自动生成文件夹
        /// </summary>
        /// <returns></returns>
        private static string GetUploadDir(bool Data, bool CreateDirectory, object model)
        {
            FileSetting _FileSetting = GetServiceProvider.GetService<FileSetting>();
            string FilePath =string.Empty;

            StringBuilder DestPath = new StringBuilder();

            if (string.IsNullOrEmpty(_FileSetting?.Path)) {

                FilePath = UploadedDir+"/" +"{year}{month}{day}";
            }
            else {
                FilePath =UploadedDir + "/" + _FileSetting?.Path;
            }
            if (model != null)
            {
                var typeArr = model.GetType().GetProperties();

                foreach (var pi in typeArr)
                {
                    if (pi.PropertyType == typeof(string) ||
                        pi.PropertyType == typeof(int) ||
                        pi.PropertyType == typeof(double) ||
                        pi.PropertyType == typeof(decimal) ||
                        pi.PropertyType == typeof(bool) ||
                        pi.PropertyType == typeof(byte) ||
                        pi.PropertyType == typeof(char) ||
                        pi.PropertyType == typeof(float) ||
                        pi.PropertyType == typeof(long) ||
                        pi.PropertyType == typeof(sbyte) ||
                        pi.PropertyType == typeof(short) ||
                        pi.PropertyType == typeof(uint) ||
                        pi.PropertyType == typeof(ulong) ||
                        pi.PropertyType == typeof(ushort)
                        )
                    {
                        FilePath = FilePath.Replace("{" + pi.Name + "}", pi.GetValue(model, null)?.ToString());
                    }
                }
            }

            if (!Data)
            {
                FilePath = FilePath.Replace("{year}", "").Replace("{month}", "").Replace("{day}", "").Replace("//", "/");
            }

            if (!string.IsNullOrEmpty(FilePath))
            {
                DestPath.Append(FilePath.Replace("{year}", DateTime.Now.Year.ToString()).Replace("{month}", DateTime.Now.Month.ToString().PadLeft(2, '0')).Replace("{day}", DateTime.Now.Day.ToString().PadLeft(2, '0')));
            }


            if (!DestPath.ToString().IsNullOrEmpty() && !DestPath.ToString().EndsWith("/")) DestPath.Append("/");

            var DestPaths = GetPhysicalPath(DestPath.ToString(),PhysicalPath.Ignore);
            // 建立文件夹
            if (!Directory.Exists(DestPaths) && CreateDirectory)
            {
                try
                {
                    Directory.CreateDirectory(DestPaths);
                }
                catch (Exception e)
                {
                    throw new ApplicationException("建立上传文件夹失败,文件夹地址：【"+ DestPaths + "】！", e);
                }

            }
            return DestPath.ToString();
        }


        public class GetUpload
        {
            /// <summary>
            /// 生成日期文件夹
            /// </summary>
            public bool Data { get; set; } = true;
            /// <summary>
            /// 自动创建文件夹
            /// </summary>
            public bool CreateDirectory { get; set; } = true;

            /// <summary>
            /// 替换匿名实体 {key}
            /// </summary>
            public object Model { get; set; }

        }


        /// <summary>
        /// 上传文件名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetUploadName(string name, bool Temporary = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            else
            {
                string ext = GetExtension(name,true).ToLower();
                string guid = System.Guid.NewGuid().ToString();
                if (Temporary)
                    guid = "Temporary" + guid;
                return guid + ext;
            }
        }

        #endregion
        #region 复制文件夹
        /// <summary>
        /// 复制文件夹(递归)
        /// </summary>
        /// <param name="varFromDirectory">源文件夹路径</param>
        /// <param name="varToDirectory">目标文件夹路径</param>
        public static void CopyFolder(string varFromDirectory, string varToDirectory)
        {
            Directory.CreateDirectory(varToDirectory);
            if (!Directory.Exists(varFromDirectory)) return;
            string[] directories = Directory.GetDirectories(varFromDirectory);
            if (directories.Length > 0)
            {
                foreach (string d in directories)
                { 
                    if (d.IndexOf(Path.DirectorySeparatorChar) > -1)
                        CopyFolder(d, varToDirectory + d.Substring(d.LastIndexOf(Path.DirectorySeparatorChar)));
                      
                }
            }
            string[] files = Directory.GetFiles(varFromDirectory);
            if (files.Length > 0)
            {
                foreach (string s in files)
                {
                    if (s.IndexOf(Path.DirectorySeparatorChar) > -1)
                        File.Copy(s, varToDirectory + s.Substring(s.LastIndexOf(Path.DirectorySeparatorChar)), true);
                }
            }
        }
        #endregion
        #region 检查文件,如果文件不存在则创建
        /// <summary>
        /// 检查文件,如果文件不存在则创建  
        /// </summary>
        /// <param name="FilePath">路径,包括文件名</param>
        public static void ExistsFile(string FilePath)
        {
            //if(!File.Exists(FilePath))    
            //File.Create(FilePath);    
            //以上写法会报错,详细解释请看下文.........   
            if (!File.Exists(FilePath))
            {
                FileStream fs = File.Create(FilePath);
                fs.Close();
            }
        }
        #endregion
        #region 删除指定文件夹对应其他文件夹里的文件
        /// <summary>
        /// 删除指定文件夹对应其他文件夹里的文件
        /// </summary>
        /// <param name="varFromDirectory">指定文件夹路径</param>
        /// <param name="varToDirectory">对应其他文件夹路径</param>
        public static void DeleteFolderFiles(string varFromDirectory, string varToDirectory)
        {
            Directory.CreateDirectory(varToDirectory);
            if (!Directory.Exists(varFromDirectory)) return;
            string[] directories = Directory.GetDirectories(varFromDirectory);
            if (directories.Length > 0)
            {
                foreach (string d in directories)
                {
                    if (d.IndexOf(Path.DirectorySeparatorChar) > -1)
                        DeleteFolderFiles(d, varToDirectory + d.Substring(d.LastIndexOf("\\")));
                }
            }
            string[] files = Directory.GetFiles(varFromDirectory);
            if (files.Length > 0)
            {
                foreach (string s in files)
                {
                    if (s.IndexOf(Path.DirectorySeparatorChar) > -1)
                        File.Delete(varToDirectory + s.Substring(s.LastIndexOf(Path.DirectorySeparatorChar)));
                }
            }
        }
        #endregion
        #region 从文件的绝对路径中获取文件名( 包含扩展名 )
        /// <summary>
        /// 从文件的绝对路径中获取文件名( 包含扩展名 )
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>        
        public static string GetFileName(string filePath)
        {
            //获取文件的名称
            FileInfo fi = new FileInfo(filePath);
            return fi.Name;
        }
        #endregion
       
        #region 创建一个目录
        /// <summary>
        /// 创建一个目录
        /// </summary>
        /// <param name="directoryPath">目录的绝对路径</param>
        public static void CreateDirectory(string directoryPath)
        {
            //如果目录不存在则创建该目录
            if (!IsExistDirectory(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        #endregion
        
        #region 获取文本文件的行数
        /// <summary>
        /// 获取文本文件的行数
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>        
        public static int GetLineCount(string filePath)
        {
            //将文本文件的各行读到一个字符串数组中
            string[] rows = File.ReadAllLines(filePath);
            //返回行数
            return rows.Length;
        }
        #endregion
        #region 获取一个文件的长度
        /// <summary>
        /// 获取一个文件的长度,单位为Byte
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>        
        public static int GetFileSize(string filePath)
        {
            //创建一个文件对象
            FileInfo fi = new FileInfo(filePath);
            //获取文件的大小
            return (int)fi.Length;
        }
        #endregion
        #region 获取指定目录中的子目录列表
        /// <summary>
        /// 获取指定目录及子目录中所有子目录列表
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        /// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。
        /// 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param>
        /// <param name="isSearchChild">是否搜索子目录</param>
        public static string[] GetDirectories(string directoryPath, string searchPattern, bool isSearchChild)
        {
            try
            {
                if (isSearchChild)
                {
                    return Directory.GetDirectories(directoryPath, searchPattern, SearchOption.AllDirectories);
                }
                else
                {
                    return Directory.GetDirectories(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
                }
            }
            catch
            {
                throw;
            }
        }
        #endregion
        #region 向文本文件写入内容
        /// <summary>
        /// 向文本文件中写入内容
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <param name="text">写入的内容</param>
        /// <param name="encoding">编码</param>
        public static void WriteText(string filePath, string text, Encoding encoding)
        {
            //向文件写入内容
            File.WriteAllText(filePath, text, encoding);
        }
        #endregion
        #region 向文本文件的尾部追加内容
        /// <summary>
        /// 向文本文件的尾部追加内容
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <param name="content">写入的内容</param>
        public static void AppendText(string filePath, string content)
        {
            File.AppendAllText(filePath, content);
        }
        #endregion
        #region 将现有文件的内容复制到新文件中
        /// <summary>
        /// 将源文件的内容复制到目标文件中
        /// </summary>
        /// <param name="sourceFilePath">源文件的绝对路径</param>
        /// <param name="destFilePath">目标文件的绝对路径</param>
        public static void Copy(string sourceFilePath, string destFilePath)
        {
            File.Copy(sourceFilePath, destFilePath, true);
        }
        #endregion
        #region 将文件移动到指定目录
        /// <summary>
        /// 将文件移动到指定目录
        /// </summary>
        /// <param name="sourceFilePath">需要移动的源文件的绝对路径</param>
        /// <param name="descDirectoryPath">移动到的目录的绝对路径</param>
        public static void Move(string sourceFilePath, string descDirectoryPath)
        {
            //获取源文件的名称
            string sourceFileName = GetFileName(sourceFilePath);
            if (IsExistDirectory(descDirectoryPath))
            {
                //如果目标中存在同名文件,则删除
                if (IsExistFile(Path.Combine(descDirectoryPath,sourceFileName)))
                {
                    DeleteFile(Path.Combine(descDirectoryPath,sourceFileName));
                }
                //将文件移动到指定目录
                File.Move(sourceFilePath, Path.Combine(descDirectoryPath,sourceFileName));
            }
        }
        #endregion
        #region 从文件的绝对路径中获取文件名( 不包含扩展名 )
        /// <summary>
        /// 从文件的绝对路径中获取文件名( 不包含扩展名 )
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>        
        public static string GetFileNameNoExtension(string filePath)
        {
            //获取文件的名称
            FileInfo fi = new FileInfo(filePath);
            return fi.Name.Split('.')[0];
        }
        #endregion
        #region 从文件的绝对路径中获取扩展名
        /// <summary>
        /// 从文件的绝对路径中获取扩展名
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>       
        /// <param name="spot">是否带.</param>   
        public static string GetExtension(string filePath,bool spot=false)
        {
            string extension= Path.GetExtension(filePath);
            if(extension.StartsWith(".")&& !spot)
                extension = extension.Substring(1, extension.Length-1);
            return extension;
          
        }
        #endregion
        #region 清空指定目录
        /// <summary>
        /// 清空指定目录下所有文件及子目录,但该目录依然保存.
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        public static void ClearDirectory(string directoryPath)
        {
            if (IsExistDirectory(directoryPath))
            {
                //删除目录中所有的文件
                string[] fileNames = GetFileNames(directoryPath);
                for (int i = 0; i < fileNames.Length; i++)
                {
                    DeleteFile(fileNames[i]);
                }
                //删除目录中所有的子目录
                string[] directoryNames = GetDirectories(directoryPath);
                for (int i = 0; i < directoryNames.Length; i++)
                {
                    DeleteDirectory(directoryNames[i]);
                }
            }
        }
        #endregion
        #region 清空文件内容
        /// <summary>
        /// 清空文件内容
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static void ClearFile(string filePath)
        {
            //删除文件
            File.Delete(filePath);

            if (!FileCommon.IsExistFile(filePath))
            {
                FileCommon.CreateDirectory(filePath.Substring(0, filePath.LastIndexOf(Path.DirectorySeparatorChar)));
                //创建一个FileInfo对象
                FileInfo file = new FileInfo(filePath);
                //创建文件
                FileStream fs = file.Create();
                //关闭文件流
                fs.Close();
            }
        }
     
        #endregion
        #region 删除指定目录
        /// <summary>
        /// 删除指定目录及其所有子目录
        /// </summary>
        /// <param name="FilePath">指定目录的绝对路径</param>
        public static void DeleteDirectory(string FilePath)
        {
            if (Directory.Exists(FilePath))
            {
                try
                {
                    //删除这个目录下的所有子目录
                    if (Directory.GetDirectories(FilePath).Length > 0)
                    {
                        foreach (string fl in Directory.GetDirectories(FilePath))
                        {
                            Directory.Delete(fl, true);
                        }
                    }
                }
                catch { }
                try
                {
                    //删除这个目录下的所有文件
                    if (Directory.GetFiles(FilePath).Length > 0)
                    {
                        foreach (string f in Directory.GetFiles(FilePath))
                        {
                            System.IO.File.Delete(f);
                        }
                        //如果目录没有文件了 删除目录
                        if(!Path.GetExtension(FilePath).IsNullOrEmpty())
                            FilePath = FilePath.Substring(0, FilePath.LastIndexOf(Path.DirectorySeparatorChar));
                        DirectoryInfo dicInfo = new DirectoryInfo(FilePath);
                        FileInfo[] files = dicInfo.GetFiles();
                        if (files.IsNullOrEmpty())
                            Directory.Delete(FilePath, true);
                    }
                }
                catch { }
                try
                {
                    Directory.Delete(FilePath, true);
                }
                catch { }
            }
            if (File.Exists(FilePath))
            {
                try
                {
                    System.IO.File.Delete(FilePath);
                    if (!Path.GetExtension(FilePath).IsNullOrEmpty())
                        FilePath = FilePath.Substring(0, FilePath.LastIndexOf(Path.DirectorySeparatorChar));
                    DirectoryInfo dicInfo = new DirectoryInfo(FilePath);
                    FileInfo[] files = dicInfo.GetFiles();
                    if (files.IsNullOrEmpty())
                        Directory.Delete(FilePath, true);
                }
                catch { }
            }
        }
        #endregion

        #region 创建一个文件
        /// <summary>
        /// 创建一个文件。
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static void CreateFile(string filePath)
        {
            try
            {
                FileSetting _FileSetting = GetServiceProvider.GetService<FileSetting>();

                if (filePath == null || _FileSetting == null || (_FileSetting.FileTypes.IsNullOrEmpty() || !_FileSetting.FileTypes.Contains(FileCommon.GetExtension(filePath).ToLower())))
                {
                    throw new OutPutException("Upload Error");
                }
                else if (!FileCommon.ValidateAbsolutePath(filePath, FileCommon.GetPhysicalPath(FileCommon.UploadedDir)))
                {
                    throw new OutPutException("Upload Path Error");
                }

            
                //如果文件不存在则创建该文件
                if (!FileCommon.IsExistFile(filePath))
                {
                    FileCommon.CreateDirectory(filePath.Substring(0, filePath.LastIndexOf(Path.DirectorySeparatorChar)));
                    //创建一个FileInfo对象
                    FileInfo file = new FileInfo(filePath);
                    //创建文件
                    FileStream fs = file.Create();
                    //关闭文件流
                    fs.Close();
                }
            }
            catch
            {
                //LogHelper.WriteTraceLog(TraceLogLevel.Error, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 创建一个文件,并将字节流写入文件。
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <param name="buffer">二进制流数据</param>
        public static void CreateFile(string filePath, byte[] buffer)
        {
            CreateFile(filePath, buffer, FileCommon.GetPhysicalPath(FileCommon.UploadedDir));
        }


        /// <summary>
        /// 创建一个文件,并将字节流写入文件。
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <param name="buffer">二进制流数据</param>
        ///  <param name="allowedRoot">允许上传的安全路径</param>
        public static void CreateFile(string filePath, byte[] buffer, string allowedRoot)
        {
            try
            {
                FileSetting _FileSetting = GetServiceProvider.GetService<FileSetting>();

                if (filePath == null || _FileSetting == null || (_FileSetting.FileTypes.IsNullOrEmpty() || !_FileSetting.FileTypes.Contains(FileCommon.GetExtension(filePath).ToLower())))
                {
                    throw new OutPutException("Upload Error");
                }
                else if (!FileCommon.ValidateAbsolutePath(filePath, allowedRoot))
                {
                    throw new OutPutException("Upload Path Error");
                }

                //验证文件不能超过最大size
                if (_FileSetting.Size != null && _FileSetting.Size > 0)
                {
                    long bufferSizeKB = buffer.Length / 1024; // 缓冲区大小转换为KB

                    // 如果缓冲区本身的大小就超过了限制，则直接抛出异常
                    if (bufferSizeKB > _FileSetting.Size)
                    {
                        throw new OutPutException(x =>
                        {
                            x.ChineseSimplified = "文件尺寸过大";
                            x.English = "The file size is too large";
                        });
                    }
                }

                //如果文件不存在则创建该文件
                if (!IsExistFile(filePath))
                {
                    CreateDirectory(filePath.Substring(0, filePath.LastIndexOf(Path.DirectorySeparatorChar)));
                    //创建一个FileInfo对象
                    FileInfo file = new FileInfo(filePath);
                    //创建文件
                    FileStream fs = file.Create();
                    //写入二进制流
                    fs.Write(buffer, 0, buffer.Length);
                    //关闭文件流
                    fs.Close();

                    fs.Dispose();
                }
            }
            catch
            {
                //LogHelper.WriteTraceLog(TraceLogLevel.Error, ex.Message);
                throw;
            }
        }
        /// <summary>
        /// 创建一个文件,并将流写入文件。
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <param name="Stream">流数据</param>
        public static void CreateFile(string filePath, Stream stream)
        {
            CreateFile(filePath, stream, FileCommon.GetPhysicalPath(FileCommon.UploadedDir));
        }

        /// <summary>
        /// 创建一个文件,并将流写入文件。
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <param name="Stream">流数据</param>
        ///  <param name="allowedRoot">允许上传的安全路径</param>
        public static void CreateFile(string filePath, Stream stream, string allowedRoot)
        {
            try
            {
                FileSetting _FileSetting = GetServiceProvider.GetService<FileSetting>();

                if (filePath == null || _FileSetting == null || (_FileSetting.FileTypes.IsNullOrEmpty() || !_FileSetting.FileTypes.Contains(FileCommon.GetExtension(filePath).ToLower())))
                {
                    throw new OutPutException("Upload Error");
                }
                else if (!FileCommon.ValidateAbsolutePath(filePath, allowedRoot))
                {
                    throw new OutPutException("Upload Path Error");
                }

                //验证文件不能超过最大size
                if (_FileSetting.Size != null && _FileSetting.Size > 0 && stream.Length > 0)
                {
                    long streamSizeKB = stream.Length / 1024; // 流的大小转换为KB

                    // 如果流本身的大小就超过了限制，则直接抛出异常
                    if (streamSizeKB > _FileSetting.Size)
                    {
                        throw new OutPutException(x =>
                        {
                            x.ChineseSimplified = "文件尺寸过大";
                            x.English = "The file size is too large";
                        });
                    }

                    // 如果文件已存在，还需考虑现有文件大小
                    if (FileCommon.IsExistFile(filePath))
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        long totalSizeKB = (fileInfo.Length + stream.Length) / 1024; // 总大小转换为KB

                        if (totalSizeKB > _FileSetting.Size)
                        {
                            throw new OutPutException(x =>
                            {
                                x.ChineseSimplified = "文件尺寸过大";
                                x.English = "The file size is too large";
                            });
                        }
                    }
                }

                CreateDirectory(filePath.Substring(0, filePath.LastIndexOf(Path.DirectorySeparatorChar)));

                FileStream addFile = new FileStream(filePath, FileMode.Append, FileAccess.Write);

                BinaryWriter AddWriter = new BinaryWriter(addFile);

                BinaryReader TempReader = new BinaryReader(stream);

                //将上传的分片追加到临时文件末尾
                AddWriter.Write(TempReader.ReadBytes((int)stream.Length));
                //关闭BinaryReader文件阅读器
                TempReader.Close();
                stream.Close();
                AddWriter.Close();
                addFile.Close();

                TempReader.Dispose();
                stream.Dispose();
                AddWriter.Dispose();
                addFile.Dispose();
            }
            catch
            {
                //LogHelper.WriteTraceLog(TraceLogLevel.Error, ex.Message);
                throw;
            }
        }
        #endregion
        /// <summary>
        /// 获取流
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static Stream GetStreamByFile(string FilePath)
        {
            if (File.Exists(FilePath))
                return new MemoryStream(File.ReadAllBytes(FilePath));
            else
                return null;
        }

        public static byte[] GetByteByFile(string FilePath)
        {
            if (File.Exists(FilePath))
                return File.ReadAllBytes(FilePath);
            else
                return null;
        }

        public static string GetStringByFile(string FilePath)
        {
            if (File.Exists(FilePath))
                return File.ReadAllText(FilePath);
            else
                return null;
        }


        public static string GetRuntimeDirectory(string path)
        {
            if (AppGlobalSettings.RunTimeEunm==RunTimeEunm.DebugLinux|| AppGlobalSettings.RunTimeEunm == RunTimeEunm.ReleaseLinux)
                return GetLinuxDirectory(path);
            
            if (AppGlobalSettings.RunTimeEunm == RunTimeEunm.DebugWindows || AppGlobalSettings.RunTimeEunm == RunTimeEunm.ReleaseWindows)
                return GetWindowDirectory(path);
            return path;
        }
     
        public static bool SafeGetFilePath(string rootDir, string _path)
        {
            string basePath = Path.GetFullPath(rootDir);
            if (!basePath.EndsWith(Path.DirectorySeparatorChar))
                basePath += Path.DirectorySeparatorChar;

            if (Path.IsPathRooted(_path))
                return false;

            // 先組合再正規化（防 ../）
            string combined = Path.Combine(basePath, _path);
            string fullPath = Path.GetFullPath(combined);

            if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                return false;

            // 關鍵修正：逐個路徑段檢查檔名合法性
            string relativePart = fullPath.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar);
            string[] parts = relativePart.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    return false;

                // 檢查這個段是否為合法檔名/資料夾名
                foreach (char invalid in Path.GetInvalidFileNameChars())
                {
                    if (part.Contains(invalid))
                    {
                        return false;
                    }
                }

                // 可選：額外禁止某些危險名稱
                if (part.Equals(".", StringComparison.Ordinal) ||
                    part.Equals("..", StringComparison.Ordinal) ||
                    part.Trim().Length == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 验证一个绝对路径是否安全：
        /// 1. 必须是绝对路径
        /// 2. 必须位于指定的允许根目录之下（包含根目录本身）
        /// 3. 不允许包含非法文件名字符（每个路径段）
        /// 4. 不允许出现 . 或 .. 作为路径段
        /// </summary>
        /// <param name="absolutePath">用户提供的绝对路径（如 C:\uploads\2025\jan\cat.jpg）</param>
        /// <param name="allowedRoot">允许的根目录（绝对路径，如 C:\uploads）</param>
        /// <returns>规范化后的安全绝对路径</returns>
        public static bool ValidateAbsolutePath(string absolutePath, string allowedRoot)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
                return false;

            if (string.IsNullOrWhiteSpace(allowedRoot))
                return false;

            // 1. 确保根目录是规范化后的绝对路径，并以分隔符结尾
            string rootPath = Path.GetFullPath(allowedRoot);
            if (!rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                rootPath += Path.DirectorySeparatorChar;

            // 2. 确保输入是绝对路径，并进行规范化（处理 ..\、/./ 等）
            if (!Path.IsPathRooted(absolutePath))
                return false;

            string lowerInput = absolutePath.ToLowerInvariant();
            if (lowerInput.Contains("..") ||
                lowerInput.Contains("../") ||
                lowerInput.Contains(@"\..") ||
                lowerInput.Contains("/.."))
            {
                return false;
            }

            string fullPath = Path.GetFullPath(absolutePath);

            // 3. 检查是否在允许的根目录之下（防目录穿越）
            if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
                return false;

            // 4. 拆解相对部分，逐段检查合法性
            string relativePart = fullPath.Substring(rootPath.Length);
            string[] parts = relativePart
                .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                        StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    return false;

                // 检查非法字符（每个段都不能包含 Path.GetInvalidFileNameChars 中的字符）
                foreach (char invalid in Path.GetInvalidFileNameChars())
                {
                    if (part.Contains(invalid))
                        return false;
                }

                // 禁止单独的 . 和 ..
                if (part == "." || part == "..")
                    return false;
            }

            return true;
        }

        public static string DeleteRepeatSlash(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path.StartsWith(@"\\"))
            {
                return @"\\" + Regex.Replace(path.Substring(2), @"\\{2,}", @"\");
            }

            return Regex.Replace(path, @"\\{2,}", @"\");
        }

        private static string GetLinuxDirectory(string path)
        {
            string pathTemp = Path.Combine(path);
            return pathTemp.Replace("\\", "/");
        }
        private static string GetWindowDirectory(string path)
        {
            string pathTemp = Path.Combine(path);
            return pathTemp.Replace("/", "\\");
        }
    }
}
