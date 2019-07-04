using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static DCMLIB.TransferSyntax;

namespace DCMLIB
{
    public abstract class DCMAbstractType
    {
        public ushort gtag;
        public ushort etag;
        public string name;
        public string vr;
        public string vm;
        public uint length;
        public object value;
        public VR vrparser;
        public abstract string ToString(string head);
        public virtual T GetValue<T>()
        {
            return vrparser.GetValue<T>((byte[])value, 0, length);
        }
        public uint UIntTag
        {
            get
            {
                return (uint)(gtag << 16) + etag;
            }
        }


    }

    public class DCMDataElement : DCMAbstractType
    {
        public override string ToString(string head)
        {
            string str = head;
            str += gtag.ToString("X4") + "," + etag.ToString("X4") + "\t";
            str += vr + "\t";
            str += name + "\t";
            //str += length.ToString();
            if (length == 0xffffffff)
                str += "Undefined\n";
            else
                str += length.ToString() + "\t";
            if (vr == "SQ")
                str += ((DCMDataSequnce)value).ToString(head + ">");
            else
                if (length == 0)
                str += "";
            else
                str += vrparser.ToString((byte[])value, 0, length);
            return str;

            //value怎么返回字符串需要根据不同VR
            //str += vrparser.ToString((byte[])value, 0, head);
            //return str;
        }
    }
    public class DCMDataSet : DCMAbstractType
    {
        public List<DCMAbstractType> items = new List<DCMAbstractType>();
        public TransferSyntax TransferSyntax;
        public DCMAbstractType this[uint tag]
        {
            get
            {
                return items.Find(elem => elem.UIntTag == tag); //查找对应元素
            }
            set
            {
                int idx = items.FindIndex(elem => elem.UIntTag == tag); //查找对应下标
                if (idx != -1)     //找到直接替换
                    items[idx] = value;
                else
                {
                    items.Add(value);              //否则添加
                    items.Sort((left, right) => left.UIntTag.CompareTo(right.UIntTag)); //组号递增元素号递增排序
                }
            }
        }


        public DCMDataSet(TransferSyntax TransferSyntax)
        {
            this.TransferSyntax = TransferSyntax;
        }

        public virtual List<DCMAbstractType> Decode(byte[] data, ref uint idx)
        {

            while (idx < data.Length)
            {
                //此处调用传输语法对象解码一条数据元素
                //DCMAbstractType item = TransferSyntax.Decode(data, ref idx);
                DCMAbstractType item = TransferSyntax.Decode(data, ref idx);
                //判断特殊标记
                if (item.gtag == 0xfffe && item.etag == 0xe0dd)
                { break; }
                if (item.gtag == 0xfffe && item.etag == 0xe00d)
                { break; }


                if (item.vr == "SQ")
                {
                    DCMDataSequnce sq = new DCMDataSequnce(TransferSyntax);
                    uint ulidx = 0;
                    byte[] val = (byte[])item.value;
                    sq.Decode(val, ref ulidx);
                    item.value = sq;
                    //todo：修正idx位置
                    if (item.length == 0xffffffff)  //修正idx位置
                        idx -= (uint)(item.length - ulidx);
                }

                items.Add(item);
            }
            return items;
        }

        public override string ToString(string head)
        {
            string str = "";
            foreach (DCMAbstractType item in items)
            {
                if (item != null)
                {
                    if (str != "") str += "\n";  //两个数据元素之间用换行符分割
                    item.vrparser = this.TransferSyntax.vrfactory.GetVR(item.vr);
                    str += item.ToString(head);
                }
            }
            return str;
        }
    }



    public class DCMDataItem : DCMDataSet
    {
        public DCMDataItem(TransferSyntax TransferSyntax) : base(TransferSyntax)
        {
        }
        public override List<DCMAbstractType> Decode(byte[] data, ref uint idx)
        {
            DCMAbstractType item = TransferSyntax.Decode(data, ref idx);
            if (item.gtag == 0xfffe && item.etag == 0xe000)  //item start
            {
                uint ulidx = 0;
                byte[] val = (byte[])item.value;
                base.Decode(val, ref ulidx);
                //tudo：修正idx位置
                if (item.length == 0xffffffff)  //修正idx位置
                    idx -= (uint)(item.length - ulidx);
            }
            return items;
        }
    }

    public class DCMDataSequnce : DCMDataSet
    {
        public DCMDataSequnce(TransferSyntax TransferSyntax) : base(TransferSyntax)
        { }
        public override List<DCMAbstractType> Decode(byte[] data, ref uint idx)
        {
            while (idx < data.Length)
            {
                DCMDataItem item = new DCMDataItem(TransferSyntax);
                item.Decode(data, ref idx);  //解码一个item，加入items列表
                if (item.items.Count > 0)
                    items.Add(item);
                else
                    break;
            }
            return items;

        }

        public override string ToString(string head)
        {
            string str = "";
            int i = 1;
            foreach (DCMAbstractType item in items)
            {
                str += "\n" + head + "ITEM" + i.ToString() + "\n";
                str += item.ToString(head);
                i++;
            }
            return str;
        }
    }

    public class DCMFileMeta : DCMDataSet
    {
        public DCMFileMeta(TransferSyntax syntax) : base(syntax) { }
        public override List<DCMAbstractType> Decode(byte[] data, ref uint idx)
        {
            while (idx < data.Length)
            {
                DCMAbstractType item = null;
                //此处调用传输语法对象解码一条数据元素
                item = TransferSyntax.Decode(data, ref idx);
                if (item.gtag == 0x0002)
                    items.Add(item);
                else
                {
                    idx -= item.length;
                    idx -= 8;
                    break;
                }
            }
            return items;
        }
    }

    public class DCMFile : DCMDataSet
    {
        private string filename;
        public DCMFileMeta filemete;
        public DCMDataSet Data;
        public DCMFile() : base(new ImplicitVRLittleEndian())
        {
        }

        public override List<DCMAbstractType> Decode(byte[] data, ref uint idx)
        {
            filemete = new DCMFileMeta(new ExplicitVRLittleEndian());
            DCMAbstractType uid = filemete.Decode(data, ref idx).Find((DCMAbstractType type) => type.etag == 0x0010 && type.gtag == 0x0002); 
            string uid1 = uid.GetValue<string>();
            TransferSyntax syn = TransferSyntaxs.All[uid1];  
            Data = new DCMDataSet(syn);
            Data.Decode(data, ref idx); 

            return items;
        }
        public List<DCMAbstractType> Decode(string fname)
        {
            filename = fname;
            byte[] data = File.ReadAllBytes(filename);  
            filename = fname;
            uint idx = 128;
            string mark = Encoding.Default.GetString(data, (int)idx, 4);
            if (mark == "DICM")
                idx += 4;
            Decode(data, ref idx);
            return items;
        }
        public override string ToString(string head)
        {
            string a = "";
            a += filemete.ToString(">");
            a += "\n";
            a += Data.ToString(">>"); //字符串拼接起来
            return a;
        }
    }
}