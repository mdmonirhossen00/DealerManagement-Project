using System;

namespace DealerManagement.Model.Common
{
    public class ResultModel<TModel>
    {
        public TModel? Data { get; set; }
        public Status Status { get; set; }
        public Exception? Exception { get; set; }
        public string Message { get; set; } = string.Empty;
        public int EffectedRows { get; set; }
        public bool Success
        {
            get => Status == Status.Success;
            set => Status = value ? Status.Success : Status.Fail;
        }
        public Audit? Audit { get; set; }
    }
}
