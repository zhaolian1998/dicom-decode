using System.Collections.Generic;
using System.IO;

namespace DCMLIB
{
    public class DicomDictionaryEntry //定义结构数组
    {


        public string Tag { get; set; }
        public string Name { get; set; }
        public string Keyword { get; set; }
        public string VR { get; set; }
        public string VM { get; set; }
    };


    public class DicomDictionary
    {

        List<DicomDictionaryEntry> data = new List<DicomDictionaryEntry>();
        public DicomDictionary(string Path) //将dicom.dic文件读取到结构数组中并保存
        {

            Path = "dicom.dic";
            StreamReader sr = File.OpenText(Path); //打开文件
            string line = "";
            while ((line = sr.ReadLine()) != null) //获取每一行
            {
                DicomDictionaryEntry DicomDictionaryEntry = new DicomDictionaryEntry();
                string[] str = new string[5], tag = new string[2];
                char[] sp = { '\t' }, sp1 = { ',' };
                str = line.Split(sp); //将每一行中的各个属性提出来
                DicomDictionaryEntry.Tag = str[0].Replace("\"", "");     //保存Tag到DicomDictionaryEntry
                DicomDictionaryEntry.Name = str[1].Replace("?", "");    //保存Name到DicomDictionaryEntry
                DicomDictionaryEntry.Keyword = str[2].Replace("?", ""); //保存Keyword到DicomDictionaryEntry
                DicomDictionaryEntry.VR = str[3];                       //保存VR到DicomDictionaryEntry
                DicomDictionaryEntry.VM = str[4];                      //保存VM到DicomDictionaryEntry
                data.Add(DicomDictionaryEntry);
            }
            sr.Close();                                                 //关闭文件
        }


        public DicomDictionaryEntry GetEntry(ushort gtag, ushort etag)                                  //搜索方法
        {

            DicomDictionaryEntry dicom = new DicomDictionaryEntry();
            string Tag = "(" + gtag.ToString("X4") + "," + etag.ToString("X4") + ")";
            foreach (DicomDictionaryEntry item in data)
            {
                if (item.Tag == Tag)
                {
                    dicom = item;
                    break;
                }
            }
            return dicom;
        }

    }
}