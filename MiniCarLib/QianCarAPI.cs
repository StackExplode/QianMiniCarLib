
using MiniCarLib.Core;

namespace MiniCarLib
{
    public delegate void OnCarRegisteredHandler(QianCar car);
    public delegate void OnStateReportedHandler(QianCar car,bool IsACK);
    public delegate void OnApplyForEnterHandler(QianCar car, QianMapPoint point);
    public delegate void OnCarEnterConfirmedHandler(QianCar car, QianMapPoint point);
    public delegate void OnApplyForLeaveHandler(QianCar car, QianMapPoint point);
    public delegate void OnCarLeaveConfirmedHandler(QianCar car);
    public delegate void OnCarUnregisterHandler(QianCar car);
    public static class QianCarAPI
    {
        static QianCarController controller;
        static QianCarController Controller => controller;
        static bool _inited = false;
        public static bool IsInited => _inited;

        public static event OnCustomDataEventHandler OnCustomData;
        public static event OnCarRegisteredHandler OnCarRegistered;
        public static event OnStateReportedHandler OnCarStateReported;
        public static event OnApplyForEnterHandler OnCarApplyForEnter;
        public static event OnCarEnterConfirmedHandler OnCarEntered;
        public static event OnApplyForLeaveHandler OnCarApplyForLeave;
        public static event OnCarLeaveConfirmedHandler OnCarLeft;
        public static event OnCarUnregisterHandler OnCarUnregisterRequest;
        public static event OnCarUnregisterHandler OnCarUnregisterResponse;

        private static void InitEvents()
        {
            controller.OnCustomData += OnCustomData;
            controller.AfterRegistered += (car, data) => OnCarRegistered?.Invoke((QianCar)car);
            controller.AfterReportState += (car, data) => OnCarStateReported?.Invoke((QianCar)car,((ReportCarStateData)data).IsACK);
            controller.OnApplyForEnter += (car, data) => OnCarApplyForEnter?.Invoke(
                (QianCar)car, 
                controller.Map[((ApplyForEnterData)data).PointID]
            );
            controller.AfterEnterConfirm += (car, data) => OnCarEntered?.Invoke(
                (QianCar)car,
                controller.Map[((CarEnterConfirmData)data).PointID]
            );
            controller.AfterApplyForLeave += (car, data) => OnCarApplyForLeave?.Invoke(
               (QianCar)car,
               controller.Map[((ApplyForLeaveData)data).PointID]
           );
            controller.OnCarLeaveConfirm += (car, data) => OnCarLeft?.Invoke((QianCar)car);
            controller.OnUnregisterRequest += (car, data) => OnCarUnregisterRequest?.Invoke((QianCar)car);
            controller.OnUnregisterResponse += (car, data) => OnCarUnregisterResponse?.Invoke((QianCar)car);
        }

        public static void Init(ushort serverID,ushort regport,ushort dataport, string mapfile, byte[] password = null)
        {
            if (!_inited)
            {
                controller = new QianCarController(serverID, regport, dataport);
                controller.SetMap(mapfile);
                if (password != null)
                    controller.PassWord = password;

                InitEvents();
            }
        }
        public static void Init(QianCarController ct, string mapfile, byte[] password = null)
        {
            if (!_inited)
            {
                controller = ct;
                controller.SetMap(mapfile);
                if (password != null)
                    controller.PassWord = password;

                InitEvents();
            }
        }


        public static void StartServer() => controller.StartServer();
        public static void StopServer() => controller.StopServer();
        public static void SendCarDataPack(QianCar car, QianComData data) => controller.SendCarDataPack(car, data);
        public static void ConfirmRegisteration(QianCar car, bool conf, ushort id) => controller.ConfirmRegisteration(car, conf, id);
        public static void QueryCarState(QianCar car) => controller.QueryCarState(car);
        public static void SendRoutes(QianCar car, bool isappend, byte[] routes = null) => controller.SendRoutes(car, isappend, routes);
        public static void CallCarEnter(QianCar car, bool allow, QianMapPoint point = null) => controller.CallCarEnter(car, allow, point);
        public static void CallCarLeave(QianCar car, bool allow) => controller.CallCarLeave(car, allow);
        public static void UnregisterCar(QianCar car) => controller.UnregisterCar(car);
        public static void ConfirmUnregteration(QianCar car, bool allow, bool removecar = true) => controller.ConfirmUnregteration(car, allow, removecar);
    }
}
