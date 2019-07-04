using DCMLIB;
using System;
using System.Collections;
using System.Text;

namespace DCMLIB
{
    public static class CxEndian
    {
        public static byte[] ReverseForBigEndian(this byte[] byteArray, int startIndex, int count)
        {
            byte[] ret = new byte[count];
            if (BitConverter.IsLittleEndian)
            {
                for (int i = startIndex + (count - 1);
                    i >= startIndex; --i) ret[(startIndex + (count - 1)) - i] = byteArray[i];
            }
            else
            {
                for (int i = 0; i < count; ++i) ret[i] = byteArray[i + startIndex];
            }
            return ret;
        }
    }


    public abstract class VR
    {
        public bool isBE;
        public bool isLongVR;
        protected byte padChar = 0x20;      //填充字符,默认0x20

        protected VR(bool isBE, bool isLongVR)
        {
            this.isBE = isBE;
            this.isLongVR = isLongVR;
        }

        public virtual string ToString(byte[] data, int startIndex, uint length)
        {
            throw new System.NotImplementedException();
        }
        public abstract T GetValue<T>(byte[] data, int startIndex, uint length);
    }

    public class SS : VR
    {
        public SS(bool isBE, bool isLongVR)
           : base(isBE, isLongVR) { }

        public override T GetValue<T>(byte[] data, int startIndex, uint length)
        {
            if (typeof(T) == typeof(Int16))
            {
                byte[] val = data;
                int idx;
                if (isBE)
                {
                    val = data.ReverseForBigEndian(startIndex, 2);
                    idx = 0;

                }
                else
                    idx = startIndex;
                return (T)(object)BitConverter.ToInt16(val, idx);
            }
            else
                throw new NotSupportedException();
        }
        public override string ToString(byte[] data, int startIndex, uint length)
        {
            Int16 value = GetValue<Int16>(data, startIndex, length);
            return value.ToString();
        }
    }

    public class US : VR
    {
        public US(bool isBE) : base(isBE, false)
        { }
        public override T GetValue<T>(byte[] data, int startIndex, uint length)
        {
            if (typeof(T) == typeof(UInt16) && length == 2)
            {
                byte[] val = data;
                int idx;
                if (isBE)
                {
                    val = data.ReverseForBigEndian(startIndex, 2);
                    idx = 0;
                }
                else
                    idx = startIndex;
                return (T)(object)BitConverter.ToUInt16(val, idx);
            }
            else
                throw new NotSupportedException();
        }
        public override string ToString(byte[] data, int startIndex, uint length)
        {
            UInt16 value = GetValue<UInt16>(data, startIndex, length);
            return value.ToString();
        }
    }


    public class UL : VR
    {
        public UL(bool isBE, bool isLVR) : base(isBE, isLVR) { }
        public override T GetValue<T>(byte[] data, int startIndex, uint length)
        {
            if (typeof(T) == typeof(UInt32))
            {
                byte[] val = data;
                int idx;
                if (isBE)
                {
                    val = data.ReverseForBigEndian(startIndex, 4);
                    idx = 0;
                }
                else
                    idx = startIndex;
                return (T)(object)BitConverter.ToUInt32(val, idx);
            }
            else
                throw new NotSupportedException();
        }
        public override string ToString(byte[] data, int startIndex, uint length)
        {
            UInt32 value = GetValue<UInt32>(data, startIndex, length);
            return value.ToString();
        }
    }

}
public class SL : VR
{
    public SL(bool isBE, bool isLongVR)
        : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(Int32))
        {
            byte[] val = data;
            int idx;
            if (isBE)
            {
                val = data.ReverseForBigEndian(startIndex, 4);
                idx = 0;

            }
            else
                idx = startIndex;
            return (T)(object)BitConverter.ToInt32(val, idx);
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        Int32 value = GetValue<Int32>(data, startIndex, length);
        return value.ToString();
    }
}

public class FL : VR
{
    public FL(bool isBE, bool isLongVR)
       : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(Single))
        {
            byte[] val = data;
            int idx;
            if (isBE)
            {
                val = data.ReverseForBigEndian(startIndex, 4);
                idx = 0;

            }
            else
                idx = startIndex;
            return (T)(object)BitConverter.ToSingle(val, idx);
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        Single value = GetValue<Single>(data, startIndex, length);
        return value.ToString();
    }
}

public class FD : VR
{
    public FD(bool isBE, bool isLongVR)
       : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(Double))
        {
            byte[] val = data;
            int idx;
            if (isBE)
            {
                val = data.ReverseForBigEndian(startIndex, 8);
                idx = 0;

            }
            else
                idx = startIndex;
            return (T)(object)BitConverter.ToDouble(val, idx);
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        Double value = GetValue<Double>(data, startIndex, length);
        return value.ToString();
    }
}

public class OB : VR
{
    public OB(bool isBE) : base(isBE, true)
    {
        padChar = 0x00;
    }
    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(byte[]))                    //支持byte数组值类型
        {
            if (data[startIndex + length - 1] == padChar)  //去除填充字符
                length--;
            byte[] val = new byte[length];
            Array.Copy(data, startIndex, val, 0, length);  //复制
            return (T)(object)val;
        }
        else
            throw new NotSupportedException();              //不支持其他值类型
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string str = "";
        byte[] value = GetValue<byte[]>(data, startIndex, length);
        int cnt = (int)length;
        if (cnt > 10) cnt = 10;   //限制长度为10
        for (int i = 0; i < cnt; i++)
            str += value[i].ToString("X2") + " ";      //每个数组元素显示为2位16进制数
        return str;
    }
}

public class OW : VR
{
    public OW(bool isBE) : base(isBE, true)
    { }
    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(Int16[]))               //支持Int16数组值类型
        {
            Int16[] intVal = new Int16[length / 2];       //每个Int16值占2字节
            for (int idx = 0; idx < intVal.Length; idx++)   //循环解码每个Int16值
            {
                if (isBE)
                {
                    byte[] val = data.ReverseForBigEndian(startIndex + idx * 2, 2);  //如为BE则先转换为对应的2字节LE编码
                    intVal[idx] = BitConverter.ToInt16(val, 0);                //解码为Int16值
                }
                else
                    intVal[idx] = BitConverter.ToInt16(data, startIndex + idx * 2);//LE则直接解码为Int16值
            }
            return (T)(object)intVal;
        }
        else
            throw new NotSupportedException();          //不支持其他值类型
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string str = "";
        Int16[] value = GetValue<Int16[]>(data, startIndex, length);  //值域解码为Int16数组
        int cnt = value.Length;
        if (cnt > 10) cnt = 10;   //限制长度为10
        for (int i = 0; i < cnt; i++)
            str += value[i].ToString("X4") + " ";   //每个数组元素显示为4位16进制数
        return str;
    }
}
public class DA : VR
{
    public DA(bool isBE, bool isLongVR)
         : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(DateTime))
        {
            byte[] val = data;
            int idx = startIndex;
            string sd = Encoding.Default.GetString(data);
            int year = int.Parse(sd.Substring(0, 4));
            int month = int.Parse(sd.Substring(4, 2));
            int day = int.Parse(sd.Substring(6, 2));
            DateTime dateTime = new DateTime(year, month, day);
            return (T)(object)dateTime;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        DateTime value = GetValue<DateTime>(data, startIndex, length);
        //使用ToShortDateString（）该方法，是为了去掉datatime类型后面的时间，因为DA类型只要前忙的日期
        return value.ToShortDateString().ToString();
    }
}

public class DT : VR
{
    public DT(bool isBE, bool isLongVR)
        : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(string))
        {
            byte[] val = data;
            int idx = startIndex;
            string sdt = Encoding.Default.GetString(data);
            int yearT = int.Parse(sdt.Substring(0, 4));
            int monthT = int.Parse(sdt.Substring(4, 2));
            int dayT = int.Parse(sdt.Substring(6, 2));
            int hourT = int.Parse(sdt.Substring(8, 2));
            int minuteT = int.Parse(sdt.Substring(10, 2));
            int secondT = int.Parse(sdt.Substring(12, 2));
            int milliT = int.Parse(sdt.Substring(15, 6));
            char cT = char.Parse(sdt.Substring(21, 1));
            int scT = int.Parse(sdt.Substring(22, 2));
            int pyT = int.Parse(sdt.Substring(24, 2));
            DateTime date = new DateTime(yearT, monthT, dayT);
            TimeSpan time = new TimeSpan(0, hourT, minuteT, secondT, milliT);
            string datetime = date.ToShortDateString().ToString() + time.ToString() + cT + scT + pyT;
            return (T)(object)datetime;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string value = GetValue<string>(data, startIndex, length);
        return value;
    }
}

public class OF : VR
{
    public OF(bool isBE, bool isLongVR)
: base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(Single[]))
        {
            Single[] vals = new Single[(data.Length - startIndex) / 4];
            byte[] val = data;
            int idx, i = 0;
            while (startIndex < data.Length)
            {
                if (isBE)
                {
                    val = data.ReverseForBigEndian(startIndex, 4);
                    idx = 0;
                }
                else
                    idx = startIndex;
                vals[i++] = BitConverter.ToSingle(val, idx);
                startIndex += 2;
            }
            return (T)(object)vals;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        Single[] value = GetValue<Single[]>(data, startIndex, length);
        string result = "";
        foreach (Single item in value)
        {
            result += (item.ToString() + ",");
        }
        return result;
    }
}

public class SQ : VR
{
    public SQ(bool isBE, bool isLVR) : base(isBE, isLVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(UInt16[]))
        {
            UInt16[] vals = new UInt16[(data.Length - startIndex) / 2];
            byte[] val = data;
            int idx, i = 0;
            while (startIndex < data.Length)
            {
                if (isBE)
                {
                    val = data.ReverseForBigEndian(startIndex, 2);
                    idx = 0;
                }
                else
                    idx = startIndex;
                vals[i++] = BitConverter.ToUInt16(val, idx);
                startIndex += 2;
            }
            return (T)(object)vals;
        }
        else
            throw new NotSupportedException();
    }

    public override string ToString(byte[] data, int startIndex, uint length)
    {
        UInt16[] value = GetValue<UInt16[]>(data, startIndex, length);
        return value.ToString();
    }
}


public class UN : VR
{
    public UN(bool isBE, bool isLongVR)
    : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(string))
        {
            byte[] val = data;
            int idx = startIndex;
            string value = Encoding.Default.GetString(data);
            return (T)(object)value;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string value = GetValue<string>(data, startIndex, length);
        return value.ToString();

    }
}

public class TM : VR
{
    public TM(bool isBE, bool isLongVR)
        : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(TimeSpan))
        {
            byte[] val = data;
            int idx = startIndex;
            string st = Encoding.Default.GetString(data);
            int hour = int.Parse(st.Substring(0, 2));
            int minute = int.Parse(st.Substring(2, 2));
            int second = int.Parse(st.Substring(4, 2));
            int milli = int.Parse(st.Substring(7, 6));
            TimeSpan timeSpan = new TimeSpan(0, hour, minute, second, milli);
            return (T)(object)timeSpan;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        TimeSpan value = GetValue<TimeSpan>(data, startIndex, length);
        return value.ToString();
    }
}

public class AS : VR
{
    public AS(bool isBE, bool isLongVR)
        : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(string))
        {
            byte[] val = data;
            int idx = startIndex;
            string value = Encoding.Default.GetString(data);
            return (T)(object)value;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string value = GetValue<string>(data, startIndex, length);
        return value.ToString();
    }
}

public class LT : VR
{
    public LT(bool isBE, bool isLongVR)
    : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        throw new NotImplementedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        throw new System.NotImplementedException();
    }
}

public class UT : VR
{
    public UT(bool isBE, bool isLongVR)
    : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(string))
        {
            byte[] val = data;
            int idx = startIndex;
            string value = Encoding.Default.GetString(data);
            return (T)(object)value;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string value = GetValue<string>(data, startIndex, length);
        return value.ToString();

    }
}

public class LO : VR
{
    public LO(bool isBE, bool isLongVR)
        : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(string))
        {
            byte[] val = data;
            int idx = startIndex;
            string value = Encoding.Default.GetString(data);
            if (value[value.Length - 1] == 0x00)
            {
                value = value.Substring(0, value.Length - 1);
            }
            return (T)(object)value;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string value = GetValue<string>(data, startIndex, length);
        return value;
    }
}

public class UI : VR
{
    public UI(bool isBE, bool isLongVR)
: base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(string))
        {
            byte[] val = data;
            int idx = startIndex;
            string value = Encoding.Default.GetString(data);
            if (value[value.Length - 1] == 0x00)
            {
                value = value.Substring(0, value.Length - 1);
            }
            return (T)(object)value;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string value = GetValue<string>(data, startIndex, length);
        return value;
    }
}
public class SH : VR
{
    public SH(bool isBE, bool isLongVR)
: base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(string))
        {
            byte[] val = data;
            int idx = startIndex;
            string value = Encoding.Default.GetString(data);
            if (value[value.Length - 1] == 0x00)
            {
                value = value.Substring(0, value.Length - 1);
            }
            return (T)(object)value;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string value = GetValue<string>(data, startIndex, length);
        return value;
    }
}
public class AE : VR
{
    public AE(bool isBE, bool isLongVR)
: base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(string))
        {
            byte[] val = data;
            int idx = startIndex;
            string value = Encoding.Default.GetString(data);
            if (value[value.Length - 1] == 0x00)
            {
                value = value.Substring(0, value.Length - 1);
            }
            return (T)(object)value;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string value = GetValue<string>(data, startIndex, length);
        return value;
    }
}
public class PN : VR
{
    public PN(bool isBE, bool isLongVR)
    : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(string))
        {
            byte[] val = data;
            int idx = startIndex;
            string value = Encoding.Default.GetString(data);
            return (T)(object)value;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string value = GetValue<string>(data, startIndex, length);
        return value.ToString();

    }
}
public class CS : VR
{
    public CS(bool isBE, bool isLongVR)
    : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(string))
        {
            byte[] val = data;
            int idx = startIndex;
            string value = Encoding.Default.GetString(data);
            return (T)(object)value;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string value = GetValue<string>(data, startIndex, length);
        return value.ToString();

    }
}
public class DS : VR
{
    public DS(bool isBE) : base(isBE, false)
    { }
    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(double))
        {
            if (data[startIndex + length - 1] == padChar)  //去除填充
                length--;
            string str = Encoding.Default.GetString(data, startIndex, (int)length);
            double dblVal = double.Parse(str);
            return (T)(object)dblVal;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        double value = GetValue<double>(data, startIndex, length);
        return value.ToString();
    }
}
public class IS : VR
{
    public IS(bool isBE, bool isLongVR)
    : base(isBE, isLongVR) { }

    public override T GetValue<T>(byte[] data, int startIndex, uint length)
    {
        if (typeof(T) == typeof(string))
        {
            byte[] val = data;
            int idx = startIndex;
            string value = Encoding.Default.GetString(data);
            return (T)(object)value;
        }
        else
            throw new NotSupportedException();
    }
    public override string ToString(byte[] data, int startIndex, uint length)
    {
        string value = GetValue<string>(data, startIndex, length);
        return value.ToString();

    }
}

public class VRFactory
{

    bool isBE;
    //定义一个Hashtable用于存储享元对象，实现享元池
    private Hashtable VRs = new Hashtable();
    public VRFactory(bool isBE)
    {
        this.isBE = isBE;
    }
    public VR GetVR(string key)
    {
        //如果对象存在，则直接从享元池获取
        if (VRs.ContainsKey(key))
        {
            return (VR)VRs[key];
        }
        //如果对象不存在，先创建一个新的对象添加到享元池中，然后返回
        else
        {
            VR fw = null;
            switch (key)
            {
                case "SS": fw = new SS(isBE, false); break;
                case "US": fw = new US(isBE); break;
                case "SL": fw = new SL(isBE, false); break;
                case "UL": fw = new UL(isBE, false); break;
                case "FL": fw = new FL(isBE, false); break;
                case "FD": fw = new FD(isBE, false); break;
                case "DA": fw = new DA(isBE, false); break;
                case "TM": fw = new TM(isBE, false); break;
                case "DT": fw = new DT(isBE, false); break;
                case "AS": fw = new AS(isBE, false); break;
                case "OB": fw = new OB(isBE); break;
                case "OF": fw = new OF(isBE, true); break;
                case "OW": fw = new OW(isBE); break;
                case "SQ": fw = new SQ(isBE, true); break;
                case "LO": fw = new LO(isBE, true); break;
                case "UI": fw = new UI(isBE, true); break;
                case "SH": fw = new SH(isBE, true); break;
                case "AE": fw = new AE(isBE, true); break;
                case "CS": fw = new CS(isBE, true); break;
                case "PN": fw = new PN(isBE, true); break;
                case "DS": fw = new DS(isBE); break;
                case "IS": fw = new IS(isBE, true); break;
                    //default for text
                    //default: fw = new ST(isBE, false); break;
            }
            VRs.Add(key, fw);
            return fw;
        }
    }

}

