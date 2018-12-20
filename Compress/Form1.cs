using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        private string fileFullPath = @"C:\Users\Desktop\test\extractusage.zip";

        private string fileExtractFolder = @"C:\Users\Desktop\test\extract\";

        private string fileExtractFileName = @"usage.csv";

        private string returnString = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 圧縮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            COMPRESS0();

            System.IO.File.Delete(@"C:\Users\Desktop\test\test.zip");
        }

        private void COMPRESS0()
        {
            using (var z = ZipFile.Open(@"C:\Users\Desktop\test\test.zip",
                    ZipArchiveMode.Create))
            {
                z.CreateEntryFromFile(
                  @"C:\Users\Desktop\test\x1.csv", "x2.csv", CompressionLevel.Optimal);
            }
            returnString = Convert.ToBase64String(FileToByteArray(@"C:\Users\Desktop\test\test.zip"));
        }

        /// <summary>
        /// タダのファイル圧縮を行う。
        /// </summary>
        private void COMPRESS1()
        {
            //圧縮するファイルのパス
            string compFile = @"C:\Users\Desktop\test\extractusage.csv";
            //作成する圧縮ファイルのパス
            string gzipFile = @"C:\Users\Desktop\test\compress\comp.zip";

            //作成する圧縮ファイルのFileStreamを作成する
            FileStream compFileStrm =
                new FileStream(gzipFile, System.IO.FileMode.Create);
            //圧縮モードのGZipStreamを作成する
            GZipStream gzipStrm =
                new GZipStream(compFileStrm,
                    CompressionMode.Compress);

            //圧縮するファイルを開いて少しずつbufferに読み込む
            FileStream inFileStrm = new FileStream(
                compFile, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[1024];
            while (true)
            {
                //圧縮するファイルからデータを読み込む
                int readSize = inFileStrm.Read(buffer, 0, buffer.Length);
                //最後まで読み込んだ時は、ループを抜ける
                if (readSize == 0)
                    break;
                //データを圧縮して書き込む
                gzipStrm.Write(buffer, 0, readSize);
            }

            //閉じる
            inFileStrm.Close();
            gzipStrm.Close();
        }

        public void Compress()
        {
            string directoryPath = @"C:\Users\\Desktop\test\";
            DirectoryInfo directorySelected = new System.IO.DirectoryInfo(directoryPath);

            foreach (FileInfo file in directorySelected.GetFiles("extractusage.csv"))
            {
                using (FileStream originalFileStream = file.OpenRead())
                {
                    if ((File.GetAttributes(file.FullName) & FileAttributes.Hidden)
                        != FileAttributes.Hidden & file.Extension != ".cmp")
                    {
                        byte[] fileBytes = null;
                        using (FileStream compressedFileStream = File.Create(file.FullName + ".cmp"))
                        {
                            using (DeflateStream compressionStream = new DeflateStream(compressedFileStream, CompressionMode.Compress))
                            {
                                originalFileStream.CopyTo(compressionStream);
                                fileBytes = new byte[compressedFileStream.Length];
                                compressedFileStream.Read(fileBytes, 0, fileBytes.Length);
                            }
                        }

                        FileInfo info = new FileInfo(directoryPath + "\\" + file.Name + ".cmp");
                        Console.WriteLine("Compressed {0} from {1} to {2} bytes.", file.Name, file.Length, info.Length);

                        // ①．CompressedFileStream を Byte 変換してからサーバ側に転送（）
                        string endString = Convert.ToBase64String(fileBytes);

                        // ②．作られた.cmpファイルを対象のフォルダ内から削除する。
                        info.Delete();
                    }
                }
            }
        }

        /// <summary>
        /// 解凍
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            Extract3();
        }

        /// <summary>
        /// ファイル圧縮制御関連
        /// </summary>
        private void ZipCompression()
        {
            Stream data = new MemoryStream(); // The original data
            Stream unzippedEntryStream; // Unzipped data from a file in the archive

            ZipArchive archive = new ZipArchive(data);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    unzippedEntryStream = entry.Open(); // .Open will return a stream
                                                        // Process entry data here
                }
            }
        }

        private void Extract()
        {
            using (ZipArchive a = ZipFile.OpenRead(fileFullPath))
            //Encodingを指定する場合は、次のようにする
            //using (ZipArchive a = ZipFile.Open(@"C:\test\1.zip",
            //    ZipArchiveMode.Read,
            //    System.Text.Encoding.GetEncoding("shift_jis")))
            {
                //書庫内のファイルとディレクトリを列挙する
                foreach (ZipArchiveEntry e in a.Entries)
                {
                    Console.WriteLine("名前       : {0}", e.Name);
                    //ディレクトリ付きのファイル名
                    Console.WriteLine("フルパス   : {0}", e.FullName);
                    Console.WriteLine("サイズ     : {0}", e.Length);
                    Console.WriteLine("圧縮サイズ : {0}", e.CompressedLength);
                    Console.WriteLine("更新日時   : {0}", e.LastWriteTime);
                }
            }
        }

        private void Extract2()
        {
            //ZIP書庫を開く
            using (ZipArchive a = ZipFile.OpenRead(fileFullPath))
            {
                foreach (ZipArchiveEntry e in a.Entries)
                {
                    e.ExtractToFile(fileExtractFolder + fileExtractFileName, true);
                }
            }
        }

        public byte[] FileToByteArray(string fileName)
        {
            byte[] buff = null;
            FileStream fs = new FileStream(fileName,
                                           FileMode.Open,
                                           FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(fileName).Length;
            buff = br.ReadBytes((int)numBytes);

            fs.Close();
            br.Close();
            return buff;
        }

        private void Extract3()
        {
            Stream st = new MemoryStream(FileToByteArray(fileFullPath));
            using (ZipArchive a = new ZipArchive(st, ZipArchiveMode.Read, false, System.Text.Encoding.UTF8))
            {
                foreach (ZipArchiveEntry e in a.Entries)
                {
                    e.ExtractToFile(fileExtractFolder + fileExtractFileName, true);
                }
            }
        }
    }
}
