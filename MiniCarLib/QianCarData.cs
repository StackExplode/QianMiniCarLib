using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using MiniCarLib.Core;

namespace MiniCarLib
{
  
    public enum CarState : byte
    {
        Unknown = 0,
        UnInited = 1,
        UnRegistered = 2,
        OutSide = 3,
        WaitingInbound = 4,
        Idle = 5,
        Running = 6,
        WaitingOutbound = 7,
        EmergencyStop = 8,
        Error = 0xFF,
    }
    
    public enum CarErrorState : byte
    {
        None = 0,
        ComError = 1,
        RunError = 2,
        ServerError = 3
    }

    public enum DataFunctionType : byte
    {
        ErrorReport = 0,
        RegisterRequest = 0x01,
        RegisterResponse = 0x02,
        RequestCarState = 0x03,
        ReportCarState = 0x04,
        SetCarRoute = 0x05,
        ApplyForEnter = 0x06,
        CallCarEnter = 0x07,
        CarEnterConfirm = 0x08,
        ApplyForLeave = 0x09,
        CallCarLeave = 0x0A,
        CarLeaveConfirm = 0x0B,
        UnregisterRequest = 0x0C,
        UnregisterResponse = 0x0D,
        EmergencyStop = 0x0E,
        

        CustomData = 0xFF,
    }

    public class QianComHeader
    {

        public const byte DataHead = 0xA5;
        public const byte HeaderLen = 7;
        public byte DataLength { get; set; }
        public ushort LocalAddress { get; set; }
        public ushort RemoteAddress { get; set; }
        public DataFunctionType FuncType
        {
            get;set;
        }

        public void FillByteData(byte[] data)
        {
            data[0] = QianComHeader.DataHead;
            data[1] = DataLength;
            data[2] = (byte)(RemoteAddress >> 8);
            data[3] = (byte)(RemoteAddress & 0xFF);
            data[4] = (byte)(LocalAddress >> 8);
            data[5] = (byte)(LocalAddress & 0xFF);
            data[6] = (byte)FuncType;
        }

        public void ParseByteData(byte[] data)
        {
            DataLength = data[1];
            RemoteAddress = (ushort)(data[2] << 8 | data[3]);
            LocalAddress = (ushort)(data[4] << 8 | data[5]);
            FuncType = (DataFunctionType)data[6];
        }
    }

    //public abstract class QianComData : IComData
    //{
    //    public QianComHeader Header { get; } = new QianComHeader();
    //    protected abstract byte _FuncCode { get; }
    //    public abstract byte CalculateDataLen();

    //    protected abstract byte[] DataFillByte(int offset);
    //    protected abstract void ByteFillData(byte[] data, int offset);
    //    public void SetHeader(ushort remote,ushort local,bool setLen = true)
    //    {
    //        Header.RemoteAddress = remote;
    //        Header.LocalAddress = local;
    //        Header.FuncCode = _FuncCode;
    //        if (setLen)
    //            SetDataLen();
    //    }

    //    public void SetDataLen()
    //    {
    //        Header.DataLength = (byte)(CalculateDataLen() + QianComHeader.HeaderLen);
    //    }

    //    public void SetDataLen(byte len)
    //    {
    //        Header.DataLength = len;
    //    }

    //    public void ParseByteData(byte[] data)
    //    {
    //        Header.DataLength = data[1];
    //        //...
    //        ByteFillData(data, QianComHeader.HeaderLen);
    //    }

    //    public byte[] ToByteData()
    //    {
    //        byte[] rst = new byte[Header.DataLength];
    //        rst[0] = QianComHeader.DataHead;
    //        rst[1] = Header.DataLength;
    //        //...
    //        DataFillByte(Header.DataLength);

    //        return rst;
    //    }
    //}

    public class RegisterRequestData : QianComData
    {
        public CarType carType { get; set; } 
        public byte TokenLen => (byte)(Token?.Length ?? 0);
        public byte[] Token { get; set; }
        public bool NeedAllocID { get; set; }

        public override byte DataLen => (byte)(TokenLen + 3);

        public override DataFunctionType FuncType => DataFunctionType.RegisterRequest;

        public override void FillByteData(byte[] data, int offset)
        {
            ComDataWriter dw = new ComDataWriter(data, offset);
            dw.WriteEnum<CarType>(carType);
            dw.WriteByte(TokenLen);
            if (TokenLen > 0)
                dw.WriteByteArray(Token, 0, Token.Length);
            dw.WriteBoolean(NeedAllocID);
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            ComDataReader dr = new ComDataReader(data, offset);
            carType = dr.ReadEnum<CarType>();
            byte len = dr.ReadByte();
            if (len > 0)
                dr.ReadByteArray(Token = new byte[len], 0, len);
        }
    }

    public class RegisterResponseData : QianComData
    {
        public bool AllowRegister { get; set; }

        public ushort AllocatedID { get; set; }

        public IPAddress ServerIP { get; set; }
        public ushort ServerPort { get; set; }

        public override byte DataLen => 9;

        public override DataFunctionType FuncType => DataFunctionType.RegisterResponse;

        public override void FillByteData(byte[] data, int offset)
        {
            ComDataWriter dw = new ComDataWriter(data, offset);
            dw.WriteBoolean(AllowRegister);
            dw.WriteHalfWord(AllocatedID);
            if (ServerIP != null)
                dw.WriteByteArray(ServerIP.GetAddressBytes(), 0, 4);
            else
                dw.SeekRelevant(4);
            dw.WriteHalfWord(ServerPort);
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            ComDataReader dr = new ComDataReader(data, offset);
            AllowRegister = dr.ReadBoolean();
            AllocatedID = dr.ReadHalfWord();
            byte[] arr = new byte[4];
            dr.ReadByteArray(arr, 0, 4);
            ServerIP = new IPAddress(arr);
            ServerPort = dr.ReadHalfWord();
        }
    }

    public class RequestCarStateData : QianComData
    {
        public override byte DataLen => 0;

        public override DataFunctionType FuncType => DataFunctionType.RequestCarState;

        public override void FillByteData(byte[] data, int offset)
        {
            
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            
        }
    }

    public class ReportCarStateData : QianComData
    {
        public bool IsACK { get; set; }
        public CarState State { get; set; }

        public byte Direction { get; set; }
        public byte PointIDLen => (byte)(PointID?.Length ?? 0);
        public string PointID { get; set; }
        public ushort Speed { get; set; }
        public ushort RouteRemain { get; set; }

        public override byte DataLen => (byte)(PointIDLen + 8);

        public override DataFunctionType FuncType => DataFunctionType.ReportCarState;

        public override void FillByteData(byte[] data, int offset)
        {
            ComDataWriter dw = new ComDataWriter(data, offset);
            dw.WriteBoolean(IsACK);
            dw.WriteEnum<CarState>(State);
            dw.WriteByte(PointIDLen);
            dw.WriteByte(Direction);
            if (PointIDLen > 0)
                dw.WriteString(PointID, Encoding.ASCII);
            dw.WriteHalfWord(Speed);
            dw.WriteHalfWord(RouteRemain);
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            ComDataReader dr = new ComDataReader(data, offset);
            IsACK = dr.ReadBoolean();
            State = dr.ReadEnum<CarState>();
            byte len = dr.ReadByte();
            if (len > 0)
                PointID = dr.ReadString(len, Encoding.ASCII);
            Speed = dr.ReadHalfWord();
            RouteRemain = dr.ReadHalfWord();
        }
    }

    public class SetCarRouteData : QianComData
    {
        public bool IsAppend { get; set; }
        public byte RouteLen => (byte)(Routes?.Length ?? 0);
        public byte[] Routes { get; set; }

        public override byte DataLen => (byte)(RouteLen + 2);

        public override DataFunctionType FuncType => DataFunctionType.SetCarRoute;

        public override void FillByteData(byte[] data, int offset)
        {
            ComDataWriter dw = new ComDataWriter(data, offset);
            dw.WriteBoolean(IsAppend);
            dw.WriteByte(RouteLen);
            if (RouteLen > 0)
                dw.WriteByteArray(Routes, 0, Routes.Length);
        }


        public override void ParseByteData(byte[] data, int offset)
        {
            ComDataReader dr = new ComDataReader(data, offset);
            IsAppend = dr.ReadBoolean();
            byte len = dr.ReadByte();
            if (len > 0)
                dr.ReadByteArray(Routes = new byte[len], 0, len);
        }
    }

    public class ApplyForEnterData : QianComData
    {
        public byte PointIDLen => (byte)(PointID?.Length ?? 0);
        public string PointID { get; set; }
        public override byte DataLen => (byte)(PointIDLen + 1);

        public override DataFunctionType FuncType => DataFunctionType.ApplyForEnter;

        public override void FillByteData(byte[] data, int offset)
        {
            ComDataWriter dw = new ComDataWriter(data, offset);
            dw.WriteByte(PointIDLen);
            if (PointIDLen > 0)
                dw.WriteString(PointID, Encoding.ASCII);
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            ComDataReader dr = new ComDataReader(data, offset);
            byte len = dr.ReadByte();
            if (len > 0)
                PointID = dr.ReadString(len, Encoding.ASCII);
        }
    }

    public class CallCarEnterData : QianComData
    {
        public bool AllowEnter { get; set; }
        public byte PointIDLen => (byte)(PointID?.Length ?? 0);
        public string PointID { get; set; }
        public override byte DataLen => (byte)(PointIDLen + 2);

        public override DataFunctionType FuncType => DataFunctionType.CallCarEnter;

        public override void FillByteData(byte[] data, int offset)
        {
            ComDataWriter dw = new ComDataWriter(data, offset);
            dw.WriteBoolean(AllowEnter);
            dw.WriteByte(PointIDLen);
            if (PointIDLen > 0)
                dw.WriteString(PointID, Encoding.ASCII);
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            ComDataReader dr = new ComDataReader(data, offset);
            AllowEnter = dr.ReadBoolean();
            byte len = dr.ReadByte();
            if (len > 0)
                PointID = dr.ReadString(len, Encoding.ASCII);
        }
    }

    public class CarEnterConfirmData : QianComData
    {
        public byte PointIDLen => (byte)(PointID?.Length ?? 0);
        public string PointID { get; set; }
        public override byte DataLen => (byte)(PointIDLen + 1);

        public override DataFunctionType FuncType => DataFunctionType.CarEnterConfirm;

        public override void FillByteData(byte[] data, int offset)
        {
            ComDataWriter dw = new ComDataWriter(data, offset);
            dw.WriteByte(PointIDLen);
            if (PointIDLen > 0)
                dw.WriteString(PointID, Encoding.ASCII);
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            ComDataReader dr = new ComDataReader(data, offset);
            byte len = dr.ReadByte();
            if (len > 0)
                PointID = dr.ReadString(len, Encoding.ASCII);
        }
    }

    public class ApplyForLeaveData : QianComData
    {
        public byte PointIDLen => (byte)(PointID?.Length ?? 0);
        public string PointID { get; set; }
        public override byte DataLen => (byte)(PointIDLen + 1);

        public override DataFunctionType FuncType => DataFunctionType.ApplyForLeave;

        public override void FillByteData(byte[] data, int offset)
        {
            ComDataWriter dw = new ComDataWriter(data, offset);
            dw.WriteByte(PointIDLen);
            if (PointIDLen > 0)
                dw.WriteString(PointID, Encoding.ASCII);
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            ComDataReader dr = new ComDataReader(data, offset);
            byte len = dr.ReadByte();
            if (len > 0)
                PointID = dr.ReadString(len, Encoding.ASCII);
        }
    }

    public class CallCarLeaveData : QianComData
    {
        public bool AllowLeave { get; set; }
        public override byte DataLen => 1;

        public override DataFunctionType FuncType => DataFunctionType.CallCarLeave;

        public override void FillByteData(byte[] data, int offset)
        {
            data[offset] = (byte)(AllowLeave ? 1 : 0);
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            AllowLeave = data[offset] == 0 ? false : true;
        }
    }

    public class CarLeaveConfirmData : QianComData
    {
        public override byte DataLen => 0;

        public override DataFunctionType FuncType => DataFunctionType.CarLeaveConfirm;

        public override void FillByteData(byte[] data, int offset)
        {
            
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            
        }
    }

    public class UnregisterRequestData : QianComData
    {
        public override byte DataLen => 0;

        public override DataFunctionType FuncType => DataFunctionType.UnregisterRequest;

        public override void FillByteData(byte[] data, int offset)
        {

        }

        public override void ParseByteData(byte[] data, int offset)
        {

        }
    }

    public class UnregisterResponseData : QianComData
    {
        public bool AllowUnregiser{ get; set; }
        public override byte DataLen => 1;

        public override DataFunctionType FuncType => DataFunctionType.UnregisterResponse;

        public override void FillByteData(byte[] data, int offset)
        {
            data[offset] = (byte)(AllowUnregiser ? 1 : 0);
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            AllowUnregiser = data[offset] == 0 ? false : true;
        }
    }

    public class CustomData : QianComData
    {
        public byte[] UserData { get; set; }
        public override byte DataLen => (byte)(UserData?.Length ?? 0);

        public override DataFunctionType FuncType => DataFunctionType.CustomData;

        public override void FillByteData(byte[] data, int offset)
        {
            if (DataLen > 0)
                Array.Copy(UserData, 0, data, offset, DataLen);
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            if (data.Length > offset)
            {
                UserData = new byte[data.Length - offset];
                Array.Copy(data, offset, UserData, 0, UserData.Length);
            }
                
        }
    }

    public class EmergencyStopData : QianComData
    {
        public byte EmergencyCode { get; set; }
        public override byte DataLen => 1;

        public override DataFunctionType FuncType => DataFunctionType.EmergencyStop;

        public override void FillByteData(byte[] data, int offset)
        {
            data[offset] = EmergencyCode;
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            EmergencyCode = data[offset];
        }
    }

    public class ErrorReportData : QianComData
    {
        public CarErrorState ErrorCode { get; set; }
        public byte ErrorDataLen => (byte)(ErrorData?.Length ?? 0);

        public byte[] ErrorData { get; set; }

        public override byte DataLen => (byte)(ErrorDataLen + 2);

        public override DataFunctionType FuncType => DataFunctionType.ErrorReport;

        public override void FillByteData(byte[] data, int offset)
        {
            ComDataWriter dw = new ComDataWriter(data, offset);
            dw.WriteEnum<CarErrorState>(ErrorCode);
            dw.WriteByte(ErrorDataLen);
            if (ErrorDataLen > 0)
                dw.WriteByteArray(ErrorData, 0, ErrorDataLen);
        }

        public override void ParseByteData(byte[] data, int offset)
        {
            ComDataReader dr = new ComDataReader(data, offset);
            ErrorCode = dr.ReadEnum<CarErrorState>();
            byte len = dr.ReadByte();
            if (len > 0)
                dr.ReadByteArray(ErrorData = new byte[len], 0, len);
        }
    }

}
