using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using MiniCarLib.Core;

namespace MiniCarLib
{
    public delegate void OnCarEventHandler(ICar car, QianComData data);
    public delegate void OnCarEventHandler<in T>(ICar car, T data) where T : QianComData;
    public delegate void OnCustomDataEventHandler(ICar car, QianComHeader header, QianComData data);

    public class QianCarController
    {
        public QianCarServer Server { get; private set; }
        public QianCarMap Map { get; set; } = new QianCarMap();

        public CarList AllCars { get; } = new CarList();
        public byte[] PassWord { get; set; } = new byte[] { 0x00 };
        public Func<CarType,ICar> CreateCarFunc { get; set; } = CarFactory.CreateInstance;
        public Func<RegisterRequestData, bool> CheckRegisteration { get; set; }


        public event OnCarEventHandler BeforeRegistering;
        public event OnCarEventHandler AfterRegistered;
        public event OnCarEventHandler BeforeReportState;
        public event OnCarEventHandler AfterReportState;
        public event OnCarEventHandler OnApplyForEnter;
        public event OnCarEventHandler BeforeEnterConfirm;
        public event OnCarEventHandler AfterEnterConfirm;
        public event OnCarEventHandler BeforeApplyForLeave;
        public event OnCarEventHandler AfterApplyForLeave;
        public event OnCarEventHandler OnCarLeaveConfirm;
        public event OnCarEventHandler OnUnregisterRequest;
        public event OnCarEventHandler OnUnregisterResponse;
        public event OnCarEventHandler OnErrorReport;
        public event OnCustomDataEventHandler OnCustomData;

        protected void _init(ushort serverid)
        {
            Server = new QianCarServer(serverid);
            Server.OnCarDataReceived += Server_OnCarDataReceived;
            CheckRegisteration = ((data) => { return Util.CompareByteArray(data.Token, PassWord); });
        }

        public QianCarController(ushort serverid)
        {
            _init(serverid);
        }

        public QianCarController(ushort serverid,ushort regport,ushort dataport)
        {
            _init(serverid);
            Server.InitRegServer(IPAddress.Any, regport);
            Server.InitDataServer(IPAddress.Any, dataport);
            
        }

        public QianCarController(ushort serverid, IPAddress serverip, ushort regport, ushort dataport)
        {
            _init(serverid);
            Server.InitRegServer(serverip, regport);
            Server.InitDataServer(serverip, dataport);

        }

        public void StartServer()
        {
            Server.DataServerStart();
            Server.RegServerStart(); 
        }

        public void StopServer()
        {
            Server.RegServerStop();
            Server.DataServerStop();
        }

        public void SetMap(string filename)
        {
            Map.ParseMapFile(filename);
        }

        public void SendCarDataPack(QianCar car,QianComData data)
        {
            Server.SendPack(car.ComClient, (ushort)car.ID, data);
        }

        public virtual void ConfirmRegisteration(QianCar car,bool conf,ushort id)
        {
            RegisterResponseData res = (RegisterResponseData)QianComDataFactory.CreateInstance(DataFunctionType.RegisterResponse);
            res.AllowRegister = conf;
            res.AllocatedID = id;
            if (Server.DataServer.ListenIP != IPAddress.Any)
                res.ServerIP = Server.DataServer.ListenIP;
            else
                res.ServerIP = Util.GetLocalIpInSameSubnet(((CarUDPClient)car.ComClient).Client.Address);
            res.ServerPort = Server.DataServer.ListenPort;
            int[] ver = Util.GetLibVersion();
            int vv = ver[0] * 100 + ver[1];
            res.ServerVersion = (ushort)vv;
            SendCarDataPack(car, res);
        }

        public virtual void QueryCarState(QianCar car)
        {
            var req = (RequestCarStateData)QianComDataFactory.CreateInstance(DataFunctionType.RequestCarState);
            SendCarDataPack(car, req);
        }

        public virtual void SendRoutes(QianCar car,bool isappend, byte[] routes = null)
        {
            var res = (SetCarRouteData)QianComDataFactory.CreateInstance(DataFunctionType.SetCarRoute);
            res.IsAppend = isappend;
            res.Routes = routes;
            SendCarDataPack(car, res);
        }

        public virtual void CallCarEnter(QianCar car, bool allow,QianMapPoint point = null)
        {
            var res = (CallCarEnterData)QianComDataFactory.CreateInstance(DataFunctionType.CallCarEnter);
            res.AllowEnter = allow;
            res.PointID = point?.Name;
            car.State = CarState.WaitingInbound;
            SendCarDataPack(car, res);
        }

        public virtual void CallCarLeave(QianCar car, bool allow)
        {
            var res = (CallCarLeaveData)QianComDataFactory.CreateInstance(DataFunctionType.CallCarLeave);
            res.AllowLeave = allow;
            car.State = CarState.WaitingOutbound;
            SendCarDataPack(car, res);
        }

        public virtual void UnregisterCar(QianCar car)
        {
            var req = (UnregisterRequestData)QianComDataFactory.CreateInstance(DataFunctionType.UnregisterRequest);
            SendCarDataPack(car, req);
        }

        public virtual void ConfirmUnregteration(QianCar car,bool allow,bool removecar=true)
        {
            var res = (UnregisterResponseData)QianComDataFactory.CreateInstance(DataFunctionType.UnregisterResponse);
            res.AllowUnregiser = allow;
            if(allow)
            {
                car.State = CarState.UnRegistered;
                if (removecar)
                    AllCars.RemoveCar(car);
            }
            
            SendCarDataPack(car, res);
        }

        public virtual void EmergencyStopCar(QianCar car,byte code)
        {
            var data = (EmergencyStopData)QianComDataFactory.CreateInstance(DataFunctionType.EmergencyStop);
            data.EmergencyCode = code;
            car.State = CarState.EmergencyStop;
            SendCarDataPack(car, data);
        }

        protected virtual void Server_OnCarDataReceived(IComClient client, QianComHeader header, QianComData data)
        {
            if(header.FuncType == DataFunctionType.RegisterRequest)
            {
                BeforeRegistering?.Invoke(null, data);
                var c = RegisrationHandler(client, header, (RegisterRequestData)data);
                AfterRegistered?.Invoke(c, data);
                return;
            }

            QianCar car = (QianCar)AllCars[header.LocalAddress];
            if (car is null)
                return;

            switch (header.FuncType)
            {
                case DataFunctionType.ReportCarState:
                    BeforeReportState?.Invoke(car, data);
                    QueryDataHandler(car, (ReportCarStateData)data);
                    AfterReportState?.Invoke(car, data);
                    break;
                case DataFunctionType.ApplyForEnter:
                    OnApplyForEnter?.Invoke(car, data);
                    break;
                case DataFunctionType.CarEnterConfirm:
                    BeforeEnterConfirm?.Invoke(car, data);
                    CarEnterConfirmHandler(car, (CarEnterConfirmData)data);
                    AfterEnterConfirm?.Invoke(car, data);
                    break;
                case DataFunctionType.ApplyForLeave:
                    BeforeApplyForLeave?.Invoke(car, data);
                    ApplyForLeaveHandler(car, (ApplyForLeaveData)data);
                    AfterApplyForLeave?.Invoke(car, data);
                    break;
                case DataFunctionType.CarLeaveConfirm:
                    car.State = CarState.OutSide;
                    OnCarLeaveConfirm?.Invoke(car, data);
                    break;
                case DataFunctionType.UnregisterRequest:
                    OnUnregisterRequest?.Invoke(car, data);
                    break;
                case DataFunctionType.UnregisterResponse:
                    OnUnregisterResponse?.Invoke(car, data);
                    UnregisterResponseHandler(car, (UnregisterResponseData)data);
                    break;
                case DataFunctionType.ErrorReport:
                    ErrorReportHandler(car, (ErrorReportData)data);
                    OnErrorReport?.Invoke(car, data);
                    break;
                default:
                    OnCustomData?.Invoke(car, header, data);
                    break;
            }
        }

        public virtual void ErrorReportHandler(QianCar car,ErrorReportData data)
        {
            car.State = CarState.Error;
            car.ErrorState = data.ErrorCode;
            car.ErrorInfo = data.ErrorData;
        }

        public virtual void UnregisterResponseHandler(QianCar car, UnregisterResponseData data)
        {
            if(data.AllowUnregiser)
            {
                car.State = CarState.UnRegistered;
                AllCars.RemoveCar(car);
            }
        }

        public virtual void ApplyForLeaveHandler(QianCar car, ApplyForLeaveData data)
        {
            car.CurrentPoint = Map[data.PointID];
        }

        public virtual void CarEnterConfirmHandler(QianCar car,CarEnterConfirmData data)
        {
            car.CurrentPoint = Map[data.PointID];
            car.Direction = data.Direction;
            car.State = CarState.Idle;
        }

        public virtual void SendCustomData(QianCar car,CustomData data)
        {
            SendCarDataPack(car, data);
        }       

        public virtual ICar RegisrationHandler(IComClient client, QianComHeader header, RegisterRequestData data)
        {
            if (CheckRegisteration(data))
            {
                ICar car = CreateCarFunc(data.carType);
                if (data.NeedAllocID)
                    car.ID = AllCars.GetAvailibleID(ushort.MaxValue,Server.ServerID);
                else
                    car.ID = header.LocalAddress;
                ((QianCar)car).State = CarState.OutSide;
                ((QianCar)car).CarVersion = data.CarVersion;
                AllCars.AddorAlterCar(car);
                ConfirmRegisteration((QianCar)car, true, (ushort)car.ID);
                return car;
            }
            else
            {
                Server.SendPack(client, header.LocalAddress, new RegisterResponseData { AllowRegister = false });
                return null;
            }
        }

        public virtual void QueryDataHandler(QianCar car, ReportCarStateData data)
        {           
            car.Speed = data.Speed;
            car.State = data.State;
            car.CurrentPoint = Map[data.PointID];
            car.Direction = data.Direction;
            car.RouteRemain = data.RouteRemain;
           
        }

        
    }
}
