using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DCMLIB
{

    public class TransferSyntax
    {
        public bool isBE;
        public bool isExplicit;
        public string name;
        public string uid;
        protected int vrDecoder;
        public VRFactory vrfactory;
        protected DicomDictionary dictionary;
        protected VR vrdecoder;

        public class TransferSyntaxs
        {
            public static Dictionary<string, TransferSyntax> TSs = null;
            public static Dictionary<string, TransferSyntax> All
            {
                get
                {
                    if (TSs == null)
                    {
                        TSs = new Dictionary<string, TransferSyntax>();
                        TransferSyntax ts = new ImplicitVRLittleEndian();
                        TSs.Add(ts.uid, ts);
                        ts = new ExplicitVRLittleEndian();
                        TSs.Add(ts.uid, ts);
                        ts = new ExplicitVRBigEndian();
                        TSs.Add(ts.uid, ts);
                    }
                    return TSs;
                }
            }
        }

        public TransferSyntax(bool isBE, bool isExplicit)
        {
            this.isBE = isBE;
            this.isExplicit = isExplicit;
            vrfactory = new VRFactory(isBE);
            dictionary = new DicomDictionary(".\\dicom.dic");
        }

        protected void LookupDictionary(DCMDataElement element)
        {
            //查数据字典得到VR,Name,VM
            DicomDictionaryEntry entry = dictionary.GetEntry(element.gtag, element.etag);
            if (entry != null)
            {
                if (element.vr == "" || element.vr == null) element.vr = entry.VR;
                element.name = entry.Name;
                element.vm = entry.VM;
            }
            else if (element.vr == "" && element.etag == 0)
                element.vr = "UL";
            //得到VR对象实例
            element.vrparser = vrfactory.GetVR(element.vr);
        }

        public virtual DCMAbstractType Decode(byte[] data, ref uint idx)
        {
            MemoryStream ms = new MemoryStream();//创建流
            ms.Write(data, 0, data.Length);
            ms.Position = idx;
            BinaryReader reader = new BinaryReader(ms);
            DCMDataElement element = new DCMDataElement();
            element.gtag = reader.ReadUInt16();//组号
            element.etag = reader.ReadUInt16();//元素号
            LookupDictionary(element);
            element.length = reader.ReadUInt32();//值长度
            if (element.length == 0xffffffff) //undefined length
                element.value = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            else
                element.value = reader.ReadBytes((int)element.length);//获取值的内容
            idx = (uint)ms.Position;
            reader.Close();//关闭内存流
            ms.Close(); //关闭二进制读写
            return element;

        }


    }
    public class ImplicitVRLittleEndian : TransferSyntax
    {
        public ImplicitVRLittleEndian() : base(false, false)
        {
            name = "ImplicitVRLittleEndian";
            uid = "1.2.840.10008.1.2";
        }

    }
    public class ExplicitVRLittleEndian : TransferSyntax
    {
        public ExplicitVRLittleEndian() : base(false, true)
        {
            name = "ExplicitVRLittleEndian";
            uid = "1.2.840.10008.1.2.1";
        }
        public override DCMAbstractType Decode(byte[] data, ref uint idx)
        {
            MemoryStream ms = new MemoryStream();//创建流
            ms.Write(data, 0, data.Length);
            ms.Position = idx;
            BinaryReader reader = new BinaryReader(ms);
            DCMDataElement element = new DCMDataElement();
            element.gtag = reader.ReadUInt16();//组号
            element.etag = reader.ReadUInt16();//元素号
            element.vr = Encoding.Default.GetString(reader.ReadBytes(2));//解码vr
            LookupDictionary(element);
            //判断（OB, OW, OF, SQ, UT, UN )
            if (element.vr == "OB" || element.vr == "OW" || element.vr == "OF" || element.vr == "SQ" || element.vr == "UT" || element.vr == "UN")
            {
                ms.Position += 2;//跳过两字节0000H
                element.length = reader.ReadUInt32();//值长度
            }
            else
            {
                element.length = reader.ReadUInt16();//值长度
            }
            //element.value = reader.ReadBytes((int)element.length);//获取值的内容 
            if (element.length == 0xffffffff) //undefined length
                element.value = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            else
                element.value = reader.ReadBytes((int)element.length);
            idx = (uint)ms.Position;
            reader.Close();
            ms.Close();
            return element;

        }

    }


    public class ExplicitVRBigEndian : TransferSyntax
    {
        public ExplicitVRBigEndian() : base(true, true)
        {
            name = "ExplicitVRBigEndian";
            uid = "1.2.840.10008.1.2.2";
        }
        public override DCMAbstractType Decode(byte[] data, ref uint idx)
        {
            MemoryStream ms = new MemoryStream();//创建流
            ms.Write(data, 0, data.Length);
            ms.Position = idx;
            BinaryReaderBE reader = new BinaryReaderBE(ms);
            DCMDataElement element = new DCMDataElement();
            element.gtag = reader.ReadUInt16();//组号
            element.etag = reader.ReadUInt16();//元素号
            LookupDictionary(element);
            element.vr = Encoding.Default.GetString(reader.ReadBytes(2));
            //判断OB, OW, OF, SQ, UT, UN 
            if (element.vr == "OB" || element.vr == "OW" || element.vr == "OF" || element.vr == "SQ" || element.vr == "UT" || element.vr == "UN")
            {
                ms.Position += 2;//跳过两字节0000H
                element.length = reader.ReadUInt32();//值长度
            }
            else
            {
                element.length = reader.ReadUInt16();//值长度
            }
            //element.value = reader.ReadBytes((int)element.length);//值的内容
            if (element.length == 0xffffffff) //undefined length
                element.value = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            else
                element.value = reader.ReadBytes((int)element.length);
            idx = (uint)ms.Position;
            reader.Close();//关闭内存流
            ms.Close(); //关闭二进制读写
            return element;
        }

    }
}



