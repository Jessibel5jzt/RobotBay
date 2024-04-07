using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace FFTAI
{
	public class ZipHelper
	{
        #region 压缩

        /// <summary>
        /// 递归压缩文件夹的内部方法
        /// </summary>
        /// <param name="folderToZip">要压缩的文件夹路径</param>
        /// <param name="zipStream">压缩输出流</param>
        /// <param name="parentFolderName">此文件夹的上级文件夹</param>
        /// <returns></returns>
        private static bool ZipDirectory(string folderToZip, ZipOutputStream zipStream, string parentFolderName)
        {
            bool result = true;
            string[] folders, files;
            ZipEntry ent = null;
            FileStream fs = null;
            Crc32 crc = new Crc32();

            try
            {
                ent = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/"));
                zipStream.PutNextEntry(ent);
                zipStream.Flush();

                files = Directory.GetFiles(folderToZip);
                foreach (string file in files)
                {
                    fs = File.OpenRead(file);

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    ent = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/" + Path.GetFileName(file)));
                    ent.DateTime = DateTime.Now;
                    ent.Size = fs.Length;

                    fs.Close();

                    crc.Reset();
                    crc.Update(buffer);

                    ent.Crc = crc.Value;
                    zipStream.PutNextEntry(ent);
                    zipStream.Write(buffer, 0, buffer.Length);
                }

            }
            catch
            {
                result = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
                if (ent != null)
                {
                    ent = null;
                }
                GC.Collect();
                GC.Collect(1);
            }

            folders = Directory.GetDirectories(folderToZip);
            foreach (string folder in folders)
                if (!ZipDirectory(folder, zipStream, folderToZip))
                    return false;

            return result;
        }

        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="folderToZip">要压缩的文件夹路径</param>
        /// <param name="zipedFile">压缩文件完整路径</param>
        /// <param name="password">密码</param>
        /// <returns>是否压缩成功</returns>
        public static bool ZipDirectory(string folderToZip, string zipedFile, string password)
        {
            bool result = false;
            if (!Directory.Exists(folderToZip))
                return result;

            ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipedFile));
            zipStream.SetLevel(6);
            if (!string.IsNullOrEmpty(password)) zipStream.Password = password;

            result = ZipDirectory(folderToZip, zipStream, "");

            zipStream.Finish();
            zipStream.Close();

            return result;
        }

        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="folderToZip">要压缩的文件夹路径</param>
        /// <param name="zipedFile">压缩文件完整路径</param>
        /// <returns>是否压缩成功</returns>
        public static bool ZipDirectory(string folderToZip, string zipedFile)
        {
            bool result = ZipDirectory(folderToZip, zipedFile, null);
            return result;
        }

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="fileToZip">要压缩的文件全名</param>
        /// <param name="zipedFile">压缩后的文件名</param>
        /// <param name="password">密码</param>
        /// <returns>压缩结果</returns>
        public static bool ZipFile(string fileToZip, string zipedFile, string password)
        {
            bool result = true;
            ZipOutputStream zipStream = null;
            FileStream fs = null;
            ZipEntry ent = null;

            if (!File.Exists(fileToZip))
                return false;

            try
            {
                fs = File.OpenRead(fileToZip);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();

                fs = File.Create(zipedFile);
                zipStream = new ZipOutputStream(fs);
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                ent = new ZipEntry(Path.GetFileName(fileToZip));
                zipStream.PutNextEntry(ent);
                zipStream.SetLevel(6);

                zipStream.Write(buffer, 0, buffer.Length);

            }
            catch
            {
                result = false;
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Finish();
                    zipStream.Close();
                }
                if (ent != null)
                {
                    ent = null;
                }
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            GC.Collect();
            GC.Collect(1);

            return result;
        }

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="fileToZip">要压缩的文件全名</param>
        /// <param name="zipedFile">压缩后的文件名</param>
        /// <returns>压缩结果</returns>
        public static bool ZipFile(string fileToZip, string zipedFile)
        {
            bool result = ZipFile(fileToZip, zipedFile, null);
            return result;
        }

        /// <summary>
        /// 压缩文件或文件夹
        /// </summary>
        /// <param name="fileToZip">要压缩的路径</param>
        /// <param name="zipedFile">压缩后的文件名</param>
        /// <param name="password">密码</param>
        /// <returns>压缩结果</returns>
        public static bool Zip(string fileToZip, string zipedFile, string password)
        {
            bool result = false;
            if (Directory.Exists(fileToZip))
                result = ZipDirectory(fileToZip, zipedFile, password);
            else if (File.Exists(fileToZip))
                result = ZipFile(fileToZip, zipedFile, password);

            return result;
        }

        /// <summary>
        /// 压缩文件或文件夹
        /// </summary>
        /// <param name="fileToZip">要压缩的路径</param>
        /// <param name="zipedFile">压缩后的文件名</param>
        /// <returns>压缩结果</returns>
        public static bool Zip(string fileToZip, string zipedFile)
        {
            bool result = Zip(fileToZip, zipedFile, null);
            return result;

        }

        #endregion

        #region 解压
        /// <summary>
        /// 功能：解压zip格式的文件。
        /// </summary>
        /// <param name="zipFilePath">压缩文件路径</param>
        /// <param name="unzipDir">解压文件存放路径,为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param>
        /// <param name="err">出错信息</param>
        /// <returns>解压是否成功</returns>
        public static bool UnZipFile(string zipFilePath, string unzipDir)
        {
            if (zipFilePath == string.Empty) return false;

            if (!File.Exists(zipFilePath))
            {
                return false;
            }

            //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹
            if (unzipDir == string.Empty) unzipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            if (!unzipDir.EndsWith("//")) unzipDir += "//";
            if (!Directory.Exists(unzipDir)) Directory.CreateDirectory(unzipDir);

            try
            {
                using (ZipInputStream stream = new ZipInputStream(File.OpenRead(zipFilePath)))
                {
                    ZipEntry theEntry;
                    while ((theEntry = stream.GetNextEntry()) != null)
                    {
                        string directoryName = Path.GetDirectoryName(theEntry.Name);
                        string fileName = Path.GetFileName(theEntry.Name);
                        if (directoryName.Length > 0) Directory.CreateDirectory(unzipDir + directoryName);
                        if (!directoryName.EndsWith("//")) directoryName += "//";
                        if (fileName != String.Empty)
                        {
                            using (FileStream streamWriter = File.Create(unzipDir + theEntry.Name))
                            {
                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = stream.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }//while
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
