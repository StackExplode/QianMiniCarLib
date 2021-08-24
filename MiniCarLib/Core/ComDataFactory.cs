using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCarLib.Core
{
    public class QianComDataFactory
    {
        public static QianComData CreateInstance(DataFunctionType fun)
        {
            switch(fun)
            {
                case DataFunctionType.RegisterRequest: return new RegisterRequestData();
                case DataFunctionType.RegisterResponse: return new RegisterResponseData();
                case DataFunctionType.RequestCarState: return new RequestCarStateData();
                case DataFunctionType.ReportCarState: return new ReportCarStateData();
                case DataFunctionType.SetCarRoute: return new SetCarRouteData();
                case DataFunctionType.ApplyForEnter:return new ApplyForEnterData();
                case DataFunctionType.CallCarEnter:return new CallCarEnterData();
                case DataFunctionType.CarEnterConfirm:return new CarEnterConfirmData();
                case DataFunctionType.ApplyForLeave:return new ApplyForLeaveData();
                case DataFunctionType.CallCarLeave:return new CallCarLeaveData();
                case DataFunctionType.CarLeaveConfirm:return new CarLeaveConfirmData();
                case DataFunctionType.UnregisterRequest:return new UnregisterRequestData();
                case DataFunctionType.UnregisterResponse:return new UnregisterResponseData();
                case DataFunctionType.CustomData:return new CustomData();
                default:return null;
            }
        }

 
    }
}
